using System;
using Raven.Client;
using Raven.Client.Document;

namespace _01___Hello_World
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (IDocumentStore documentStore = new DocumentStore {Url = "http://localhost:8080"}.Initialize())
            {
                // Anonymous types + dynamics
                using (IDocumentSession session = documentStore.OpenSession())
                {
                    session.Store(new {Id = "demo/1", Text = "Hello, World!"});
                    session.SaveChanges();
                }

                using (IDocumentSession session = documentStore.OpenSession())
                {
                    var hello = session.Load<dynamic>("demo/1");
                        // RavenDB guarantees that once a document has been written it can be loaded
                    Console.WriteLine("RavenDB says " + hello.Text);
                }

                // POCOs
                string helloId;
                using (IDocumentSession session = documentStore.OpenSession())
                {
                    var hello = new HelloWorld {Text = "Statics work, too!"};
                    session.Store(hello);
                    helloId = hello.Id;
                    session.SaveChanges();
                }

                using (IDocumentSession session = documentStore.OpenSession())
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