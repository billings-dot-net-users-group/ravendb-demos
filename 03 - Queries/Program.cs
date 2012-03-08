using System;
using System.Linq;
using System.Reflection;
using Models.Blog;
using Raven.Client.Document;
using Raven.Client.Indexes;

namespace _03___Queries
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Monotonic reads ensure that we don't get stale results during our demo!
            using (var documentStore = new DocumentStore { Url = "http://localhost:8080", Conventions = new DocumentConvention { DefaultQueryingConsistency = ConsistencyOptions.MonotonicRead } }.Initialize())
            {
                // Populate the static indexes
                IndexCreation.CreateIndexes(Assembly.GetExecutingAssembly(), documentStore);

                using (var session = documentStore.OpenSession())
                {
                    var john = new User { Name = "John" };
                    session.Store(john);

                    var mary = new User { Name = "Mary" };
                    session.Store(mary);

                    session.Store(new Post
                    {
                        Title = "Welcome to RavenDB",
                        Body = "It rocks!",
                        Tags = { "ravendb" },
                        Comments = { new Comment { UserId = john.Id, Text = "Yes it does!" } }
                    });

                    session.Store(new Post
                    {
                        Title = "Hello, Billings!",
                        Body = "What's up with this snow?!",
                        Tags = { "billings" }
                    });

                    session.Store(new Post
                    {
                        Title = "March Meeting - RavenDB",
                        Body = "Zero to running in less than 2 minutes!",
                        Tags = { "ravendb", "billings" }
                    });

                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    // Simple Linq syntax!
                    // Can bring back pieces of documents
                    Console.WriteLine("Posts tagged ravendb:");
                    var ravenDbPosts = from p in session.Query<Post>()
                                       where p.Tags.Any(t => t == "ravendb")
                                       select p.Title;

                    foreach (var title in ravenDbPosts)
                        Console.WriteLine(title);

                    // Map-reduce for doing aggregate queries
                    var tags = session.Query<Tags_Count.ReduceResult, Tags_Count>();
                    foreach (var tag in tags)
                        Console.WriteLine("{0} - {1}", tag.Tag, tag.Count);
                }

                Console.ReadLine();
            }
        }
    }

    internal class Tags_Count : AbstractIndexCreationTask<Post, Tags_Count.ReduceResult>
    {
        public class ReduceResult
        {
            public string Tag { get; set; }
            public int Count { get; set; }
        }

        public Tags_Count()
        {
            Map = posts => from p in posts
                           from t in p.Tags
                           select new { Tag = t, Count = 1 };

            Reduce = results => from r in results
                                group r by r.Tag
                                into g
                                select new { Tag = g.Key, Count = g.Sum(x => x.Count) };
        }
    }
}