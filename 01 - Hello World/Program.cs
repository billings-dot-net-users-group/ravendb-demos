using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client.Document;

namespace _01___Hello_World
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var documentStore = new DocumentStore { Url = "http://localhost:8080" }.Initialize())
            {
                using (var session = documentStore.OpenSession())
                {
                    session.Store(new { Id = "demo/1", Text = "Hello, World!" });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    dynamic hello = session.Load<dynamic>("demo/1");
                    Console.WriteLine(hello.Text);
                }
            }
        }
    }
}
