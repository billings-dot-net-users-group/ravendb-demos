using System;
using Raven.Client.Document;

namespace _01___Hello_World
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Initializing the document store is expensive - do it once!
            using (var documentStore = new DocumentStore {Url = "http://localhost:8080"}.Initialize())
            {
                // Anonymous types + dynamics
                using (var session = documentStore.OpenSession())   // sessions are cheap!
                {
                    session.Store(new {Id = "demo/1", Text = "Hello, World!"});
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var hello = session.Load<dynamic>("demo/1");
                    // RavenDB guarantees that once a document has been written it can be loaded
                    Console.WriteLine("RavenDB says " + hello.Text);
                }

                // POCOs
                string helloId;
                using (var session = documentStore.OpenSession())
                {
                    var hello = new HelloWorld {Text = "Statics work, too!"};   // The ID is generated for you!
                    session.Store(hello);   // The client generates an ID on Store without needing to go to the server!
                    helloId = hello.Id;
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var hello = session.Load<HelloWorld>(helloId);
                    Console.WriteLine(hello.Text);
                }
            }

            Console.ReadLine();
        }
    }

    internal class HelloWorld
    {
        public string Id { get; set; }
        public string Text { get; set; }
    }
}