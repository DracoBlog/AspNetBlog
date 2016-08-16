using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace Blog.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [StringLength(500)]
        public string Text { get; set; }
        public Post Post { get; set; }
    }
}