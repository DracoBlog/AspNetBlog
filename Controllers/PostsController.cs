using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
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
            Post post = db.Posts.Include(p => p.Author).Include(p => p.Comments)
                .Include(p => p.Tags).Include(p => p.Images).SingleOrDefault(p => p.Id == id);
            if (post == null)
            {
                return HttpNotFound();
            }

            post.Comments = new List<Comment>();
            post.Comments = db.Comments.Include(p => p.Author).Where(c => c.Post_Id == post.Id).ToList();

            post.Tags = new List<Tag>();
            post.Tags = db.Tags.Where(t => t.Post_Id == post.Id).ToList();

            var author = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (author != null)
            {
                post.Author = author;
            }

            var posts = db.Posts.Include(p => p.Author).OrderByDescending(p => p.Date).Take(6);
            ViewBag.Posts = posts.ToList();

            ViewBag.Post = post;
            ViewBag.Tags = post.Tags;

            var result = this.db.Posts
                .Where(p => p.Id == id)
                .Select(PostViewModel.FromPost)
                .FirstOrDefault();

            ViewBag.Result = result;

            return View(post);
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
        public ActionResult Create([Bind(Include = "Id,Title,Body,TagsString")] Post post, HttpPostedFileBase upload)
        {
            if (upload != null && upload.ContentLength > 0)
            {
                var avatar = new Image
                {
                    FileName = System.IO.Path.GetFileName(upload.FileName),
                    FileType = FileType.Avatar,
                    ContentType = upload.ContentType
                };
                using (var reader = new System.IO.BinaryReader(upload.InputStream))
                {
                    avatar.Content = reader.ReadBytes(upload.ContentLength);
                }
                post.Images = new List<Image> { avatar };
            }
            if (ModelState.IsValid)
            {
                post.Author = db.Users
                    .FirstOrDefault(u => u.UserName == User.Identity.Name);
                db.Posts.Add(post);
                db.SaveChanges();

                var postId = post.Id;

                var tags = post.TagsString.Split(' ').ToList();
                foreach (var tag in tags)
                {
                    Tag newTag = new Tag();
                    newTag.Text = tag;
                    newTag.Post_Id = postId;
                    db.Tags.Add(newTag);
                    db.SaveChanges();
                }
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
            Post post = db.Posts.Include(p => p.Author).Include(p => p.Images).SingleOrDefault(p => p.Id == id);

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
        public ActionResult Edit([Bind(Include = "Id,Title,Body,Date,Author_Id")] Post post, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {
                db.Entry(post).State = EntityState.Modified;
                db.SaveChanges();
                var postToUpdate = db.Posts.Include(p => p.Images).SingleOrDefault(p => p.Id == post.Id);

                try
                {
                    if (upload != null && upload.ContentLength > 0)
                    {
                        if (postToUpdate.Images.Any(f => f.FileType == FileType.Avatar))
                        {
                            db.Images.Remove(postToUpdate.Images.First(f => f.FileType == FileType.Avatar));
                        }
                        var avatar = new Image
                        {
                            FileName = System.IO.Path.GetFileName(upload.FileName),
                            FileType = FileType.Avatar,
                            ContentType = upload.ContentType
                        };
                        using (var reader = new System.IO.BinaryReader(upload.InputStream))
                        {
                            avatar.Content = reader.ReadBytes(upload.ContentLength);
                        }
                        postToUpdate.Images = new List<Image> { avatar };
                    }
                    db.Entry(postToUpdate).State = EntityState.Modified;
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("",
                        "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }


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
