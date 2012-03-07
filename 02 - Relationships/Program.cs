using System;
using System.Linq;
using System.Reflection;
using Models.Blog;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Raven.Client.Linq;

namespace _02___Relationships
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var documentStore = new DocumentStore { Url = "http://localhost:8080" }.Initialize())
            {
                IndexCreation.CreateIndexes(Assembly.GetExecutingAssembly(), documentStore);

                string postId;
                using (var session = documentStore.OpenSession())
                {
                    var john = new User { Name = "John" };
                    session.Store(john);

                    var post = new Post
                    {
                        Title = "Welcome to RavenDB", 
                        Body = "It rocks!", 
                        Tags = { "ravendb" }, 
                        Comments = { new Comment { UserId = john.Id, Text = "Yes it does!" } }
                    };
                    session.Store(post);
                    postId = post.Id;

                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var post = session.Load<Post>(postId);
                    var user = session.Load<User>(post.Comments[0].UserId);
                    Console.WriteLine("# of requests: {0}", session.Advanced.NumberOfRequests);
                }

                // Use includes to bring back referenced docs in fewer requests
                using (var session = documentStore.OpenSession())
                {
                    var post = session.Include("Comments,UserId").Load<Post>(postId);
                    var user = session.Load<User>(post.Comments[0].UserId);
                    Console.WriteLine("# of requests: {0}", session.Advanced.NumberOfRequests);
                }

                Console.ReadLine();
            }
        }
    }
}