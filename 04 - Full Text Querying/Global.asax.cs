using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using FileHelpers;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Client.Indexes;
using _04___Full_Text_Querying.Models;

namespace _04___Full_Text_Querying
{
    public class MvcApplication : HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Search", action = "FullText", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            BuildContainer();
            PopulateData();
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

        private void BuildContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<AutofacWebTypesModule>();
            builder.RegisterFilterProvider();
            builder.RegisterModelBinderProvider();
            builder.Register(c =>
            {
                var documentStore = new EmbeddableDocumentStore
                {
                    RunInMemory = true,
                    UseEmbeddedHttpServer = true // Surely you jest? Nope! http://localhost:8080
                }.Initialize();

                IndexCreation.CreateIndexes(Assembly.GetExecutingAssembly(), documentStore);
                Raven.Client.MvcIntegration.RavenProfiler.InitializeFor(documentStore);

                return documentStore;
            }).As<IDocumentStore>().SingleInstance();
            builder.RegisterControllers(Assembly.GetExecutingAssembly());

            DependencyResolver.SetResolver(new AutofacDependencyResolver(builder.Build()));
        }

        [DelimitedRecord(",")]
        private class CsvPost
        {
            [FieldQuoted]
            public long Id;

            [FieldQuoted, FieldConverter(ConverterKind.Date, "yyyy-MM-dd HH:mm:ss")]
            public DateTime CreatedAt;

            [FieldQuoted]
            public int Score;

            [FieldQuoted]
            public int ViewCount;

            [FieldQuoted(MultilineMode.AllowForBoth)]
            public string Body;

            [FieldQuoted]
            public string Title;

            [FieldQuoted]
            public string Tags;

            [FieldQuoted]
            public int? AnswerCount;

            [FieldQuoted]
            public int? CommentCount;

            [FieldQuoted]
            public int? FavoriteCount;

            public Post ToPost()
            {
                return new Post
                {
                    Id = Id,
                    CreatedAt = CreatedAt,
                    Score = Score,
                    ViewCount = ViewCount,
                    Body = Body,
                    Title = Title,
                    Tags = ParseTags(),
                    AnswerCount = AnswerCount,
                    CommentCount = CommentCount,
                    FavoriteCount = FavoriteCount
                };
            }

            private static readonly Regex TagsRegex = new Regex("<(.+?)>", RegexOptions.Compiled);
            private List<string> ParseTags()
            {
                if (String.IsNullOrEmpty(Tags))
                    return new List<string>();

                var tags = (from Match m in TagsRegex.Matches(Tags)
                            from Group g in m.Groups
                            where m.Success
                            where g.Value.IndexOf("<") == -1
                            select g.Value).ToList();

                return tags;
            }
        }

        private void PopulateData()
        {
            var path = Server.MapPath("~/App_Data/so.csv");
            var engine = new FileHelperEngine<CsvPost> { Options = { IgnoreFirstLines = 1 } };
            var data = engine.ReadFile(path);
            var posts = data.Select(d => d.ToPost()).ToArray();

            var documentStore = DependencyResolver.Current.GetService<IDocumentStore>();
            using (var session = documentStore.OpenSession())
            {
                foreach (var post in posts)
                    session.Store(post);

                session.SaveChanges();
            }
        }
    }
}