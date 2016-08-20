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

        public IEnumerable<CommentViewModel> Comments { get; set; }

        public static Expression<Func<Post, PostViewModel>> FromPost
        {
            get
            {
                return post => new PostViewModel
                {
                    Id = post.Id,
                    Title = post.Title,
                    Author = post.Author.FullName,
                    Comments = post.Comments.AsQueryable().Select(CommentViewModel.ViewModel),
                    Body = post.Body
                };
            }
        }

    }

    public class CommentViewModel
    {
        public string Text { get; set; }

        public string Author { get; set; }

        public DateTime Date { get; set; }

        public static Expression<Func<Comment, CommentViewModel>> ViewModel
        {
            get
            {
                return c => new CommentViewModel()
                {
                    Text = c.Text,
                    Author = c.Author.FullName,
                    Date = c.Date
                };
            }
        }
    }
}