﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.Blog
{
    public class BlogDTO
    {
        public int BlogId { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
