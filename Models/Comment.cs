using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blog.Models
{
    public class Comment
    {
        public Comment()
        {
            this.Date = DateTime.Now;
        }
        public int Id { get; set; }

        [StringLength(500)]
        [Required]
        public string Text { get; set; }

        public string Author_Id { get; set; }

        [ForeignKey("Author_Id")]
        public ApplicationUser Author { get; set; }

        [Required]
        public DateTime Date { get; set; }
        public int Post_Id { get; set; }

        [ForeignKey("Post_Id")]
        public Post Post { get; set; }
    }
}