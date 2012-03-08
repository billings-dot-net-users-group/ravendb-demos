using System.Linq;
using System.Web.Mvc;
using Raven.Abstractions.Indexing;
using Raven.Client;
using Raven.Client.Indexes;
using Raven.Client.Linq;
using _04___Full_Text_Querying.Models;

namespace _04___Full_Text_Querying.Controllers
{
    public class SearchController : Controller
    {
        public IDocumentStore DocumentStore { get; set; }

        public SearchController(IDocumentStore documentStore)
        {
            DocumentStore = documentStore;
        }

        public ActionResult FullText(string q)
        {
            if (string.IsNullOrEmpty(q))
                return View(Enumerable.Empty<Post>());

            ViewBag.Query = q;
            using (var session = DocumentStore.OpenSession())
            {
                var posts = session
                    .Query<Posts_Search.ReduceResult, Posts_Search>()
                    .Search(x => x.Query, q)
                    .As<Post>()
                    .ToList();
                return View(posts);
            }
        }
    }

    // Not the best .NET naming convention, but underscores in index creation tasks
    // translate to slashes in the path. Posts_Search => posts/search
    public class Posts_Search : AbstractIndexCreationTask<Post, Posts_Search.ReduceResult>
    {
        public class ReduceResult
        {
            public string Query { get; set; }
        }

        public Posts_Search()
        {
            Map = posts => from post in posts
                           select new
                           {
                               Query = new object[]
                               {
                                   post.Title,
                                   post.Body,
                                   post.Tags
                               }
                           };

            Indexes.Add(x => x.Query, FieldIndexing.Analyzed);
        }
    }
}