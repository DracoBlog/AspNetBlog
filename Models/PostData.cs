using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.Entity;

namespace Blog.Models
{
    using System.Collections.Generic;

    public static class PostsData
    {
        public static ApplicationDbContext db = new ApplicationDbContext();
        public static List<Post> GetAll()
        {
            return db.Posts.Include(p => p.Author).Include(p => p.Comments).Include(p => p.Tags).ToList();
        }
    }
}