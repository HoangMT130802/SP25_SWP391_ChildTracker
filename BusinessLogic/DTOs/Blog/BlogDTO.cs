using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Models;

namespace BusinessLogic.DTOs.Blog
{
    public class BlogDTO
    {
        public int BlogId { get; set; }

        public int AuthorId { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string ImageUrl { get; set; }

        public int Views { get; set; }

        public int Likes { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
