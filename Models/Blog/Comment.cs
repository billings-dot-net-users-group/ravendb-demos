using System;

namespace Models.Blog
{
    public class Comment
    {
        public Comment()
        {
            CreatedAt = DateTimeOffset.Now;
        }

        public string UserId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string Text { get; set; }
    }
}