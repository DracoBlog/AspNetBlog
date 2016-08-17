using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Blog.Models
{
    using System;
    using System.Linq.Expressions;

    public class PostViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public string Body { get; set; }
        
        public static Expression<Func<Post, PostViewModel>> FromPost
        {
            get
            {
                return post => new PostViewModel
                {
                    Id = post.Id,
                    Title = post.Title,
                    Author = post.Author.FullName,
                    Body = post.Body
                };
            }
        }

    }
}