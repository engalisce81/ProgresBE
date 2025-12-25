using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Acadmy.Dtos.Request.Posts
{
    public class CreateUpdatePostDto
    {
        [Required]
        [MaxLength(256)]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        public IFormFile File { get; set; }
        public bool IsGeneral { get; set; }
    }
}
