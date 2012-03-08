using System.Linq;
using System.Web.Mvc;
using Raven.Client;
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
                var posts = session.Query<Posts_Search.ReduceResult, Posts_Search>().Search(x => x.Query, q).As<Post>().ToList();
                return View(posts);
            }
        }
    }
}