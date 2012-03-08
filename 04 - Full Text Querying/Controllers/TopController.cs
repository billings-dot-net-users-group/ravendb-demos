using System.Linq;
using System.Web.Mvc;
using Raven.Abstractions.Indexing;
using Raven.Client;
using Raven.Client.Indexes;
using Raven.Client.Linq;
using _04___Full_Text_Querying.Models;

namespace _04___Full_Text_Querying.Controllers
{
    public class TopController : Controller
    {
        public IDocumentStore DocumentStore { get; set; }

        public TopController(IDocumentStore documentStore)
        {
            DocumentStore = documentStore;
        }

        public ActionResult Tags()
        {
            using (var session = DocumentStore.OpenSession())
            {
                return View(session
                    .Query<Posts_ByTag.ReduceResult, Posts_ByTag>()
                    .OrderByDescending(x => x.Count)
                    .ToList());
            }
        }
    }

    public class Posts_ByTag : AbstractIndexCreationTask<Post, Posts_ByTag.ReduceResult>
    {
        public class ReduceResult
        {
            public string Tag { get; set; }
            public int Count { get; set; }
        }

        public Posts_ByTag()
        {
            Map = posts => from p in posts
                           from t in p.Tags
                           select new { Tag = t, Count = 1 };

            Reduce = results => from r in results
                                group r by r.Tag
                                into g
                                select new { Tag = g.Key, Count = g.Sum(x => x.Count) };

            Sort(x => x.Count, SortOptions.Short);
        }
    }
}