using Blog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Blog.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var data = PostsData
                .GetAll()
                .AsQueryable()
                .Select(PostViewModel.FromPost)
                .ToList();

            return this.View(data);
            //var db = new ApplicationDbContext();
            //var post = db.Posts.OrderByDescending(p => p.Date);
            //return View(post.ToList());
        }

        public ActionResult Search(string query)
        {
            var result = PostsData
                .GetAll()
                .AsQueryable()
                .Where(post => post.Title.ToLower().Contains(query.ToLower()) ||
                    post.Author.FullName.ToLower().Contains(query.ToLower()))
                .Select(PostViewModel.FromPost)
                .ToList();

            return this.PartialView("_PostResult", result);
        }

        //public ActionResult ContentById(int id)
        //{
        //    if (!Request.IsAjaxRequest())
        //    {
        //        Response.StatusCode = (int)HttpStatusCode.Forbidden;
        //        return this.Content("This action can be invoke only by AJAX call");
        //    }

        //    var post = PostsData.GetAll().FirstOrDefault(x => x.Id == id);
        //    if (post == null)
        //    {
        //        Response.StatusCode = (int)HttpStatusCode.NotFound;
        //        return this.Content("Post not found");
        //    }

        //    return this.Content(post.Body);
        //}

    }
}