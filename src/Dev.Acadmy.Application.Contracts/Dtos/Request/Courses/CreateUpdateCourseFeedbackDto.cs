using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Acadmy.Dtos.Request.Courses
{
    public class CreateUpdateCourseFeedbackDto
    {
        [Required]
        public Guid CourseId { get; set; }
        [Range(1, 5)]
        public int Rating { get; set; }
        [Required]
        [StringLength(500)]
        public string Comment { get; set; }
        public bool IsAccept { get; set; }
    }
}
