using System;
using Volo.Abp.Application.Dtos;

namespace Dev.Acadmy.Dtos.Response.Courses
{
    public class CourseCertificateDto : EntityDto<Guid>
    {
        public Guid CourseId { get; set; }
        public string TemplateUrl { get; set; }
        public double NameXPosition { get; set; } // النسبة المئوية X
        public double NameYPosition { get; set; } // النسبة المئوية Y

    }
}
