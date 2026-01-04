using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Acadmy.Dtos.Request.Courses
{
    public class CreateUpdateCourseCertificateDto
    {
        public Guid CourseId { get; set; }
        public IFormFile TemplateFile { get; set; } // رابط ملف الـ PDF المخزن
        public double NameXPosition { get; set; } // النسبة المئوية X
        public double NameYPosition { get; set; } // النسبة المئوية Y
    }
}
