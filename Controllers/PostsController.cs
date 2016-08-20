using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
using Blog.Models;

using Blog.Extensions;

namespace Blog.Controllers
{
    [ValidateInput(false)]
    public class PostsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Posts
        public ActionResult Index()
        {
            var postsWithAuthors = db.Posts.Include(p => p.Author).ToList();
            return View(postsWithAuthors);
        }

        public ActionResult SaveComment(string text, int id)
        {
            //int postId = ViewBag.Post.Id;

            Comment comment = new Comment();
            comment.Text = text;
            if (User.Identity.Name != null)
            {
                comment.Author = db.Users
                    .FirstOrDefault(u => u.UserName == User.Identity.Name);
            }
            comment.Post_Id = id;
            db.Comments.Add(comment);
            db.SaveChanges();

            var result = this.db.Posts
                .Where(post => post.Id == id)
                .Select(PostViewModel.FromPost)
                .FirstOrDefault();

            return this.PartialView("_CommentResult", result);
        }

        // GET: Posts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Include(p => p.Author).SingleOrDefault(p => p.Id == id);
            if (post == null)
            {
                return HttpNotFound();
            }

            post.Comments = new List<Comment>();
            post.Comments = db.Comments.Include(p => p.Author).Where(c => c.Post_Id == post.Id).ToList();

            var author = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);

            if(author != null)
            {
                post.Author = author;
            }

            var posts = db.Posts.Include(p => p.Author).OrderByDescending(p => p.Date).Take(6);
            ViewBag.Posts = posts.ToList();

            ViewBag.Post = post;

            return View();
        }

        // GET: Posts/Create
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Body")] Post post)
        {
            if (ModelState.IsValid)
            {
                post.Author = db.Users
                    .FirstOrDefault(u => u.UserName == User.Identity.Name);
                db.Posts.Add(post);
                db.SaveChanges();
                this.AddNotification("Post Created!", NotificationType.SUCCESS);
                return RedirectToAction("Index");
            }

            return View(post);
        }

        // GET: Posts/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Include(p => p.Author).SingleOrDefault(p => p.Id == id);

            if (post == null || (post.Author.UserName != User.Identity.Name && !User.IsInRole("Administrators")))
            {
                return HttpNotFound();
            }
            var authors = db.Users
                .OrderBy(u => u.FullName)
                .ToList();
            ViewBag.Authors = authors;
            return View(post);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Body,Date,Author_Id")] Post post)
        {
            if (ModelState.IsValid)
            {
                db.Entry(post).State = EntityState.Modified;
                db.SaveChanges();
                this.AddNotification("Post was edited successfully !", NotificationType.SUCCESS);
                return RedirectToAction("Index");
            }
            return View(post);
        }

        // GET: Posts/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        // POST: Posts/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Post post = db.Posts.Find(id);
            db.Posts.Remove(post);
            this.AddNotification("Post was deleted successfully", NotificationType.SUCCESS);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
