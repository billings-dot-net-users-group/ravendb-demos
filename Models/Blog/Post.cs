using System;
using System.Collections.Generic;

namespace Models.Blog
{
    public class Post
    {
        public Post()
        {
            CreatedAt = DateTimeOffset.Now;
            Comments = new List<Comment>();
            Tags = new List<string>();
        }

        public string Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public List<Comment> Comments { get; set; } 
        public List<string> Tags { get; set; }
    }
}