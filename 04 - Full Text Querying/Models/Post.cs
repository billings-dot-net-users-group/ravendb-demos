using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace _04___Full_Text_Querying.Models
{
    public class Post
    {
        public Post()
        {
            Tags = new List<string>();
        }

        public long Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public int Score { get; set; }

        public int ViewCount { get; set; }

        public string Body { get; set; }

        public string Title { get; set; }

        public List<string> Tags { get; set; }

        public int? AnswerCount { get; set; }

        public int? CommentCount { get; set; }

        public int? FavoriteCount { get; set; }
    }

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