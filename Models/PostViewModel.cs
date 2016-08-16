namespace Blog.Models
{
    using System;
    using System.Linq.Expressions;

    public class PostViewModel
    {
        public static Expression<Func<Post, PostViewModel>> FromPost
        {
            get
            {
                return post => new PostViewModel
                {
                    Id = post.Id,
                    Title = post.Title,
                    Author = post.Author.FullName
                };
            }
        }

        public int Id { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }
    }
}