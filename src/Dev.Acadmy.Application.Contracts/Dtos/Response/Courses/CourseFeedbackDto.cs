using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Dev.Acadmy.Dtos.Response.Courses
{
    public class CourseFeedbackDto:FeedbackDto
    {
        public Guid CourseId { get; set; }
        public string CourseName { get; set; }
        public string LogoUrl { get; set; }
        public bool IsAccept { get; set; }
    }
}
