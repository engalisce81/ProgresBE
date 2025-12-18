using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Acadmy.Entities.Courses.Entities
{
    public class CourseLookupDto
    {
        public Guid CourseId { get; set; }
        public string Name { get; set; }
        public bool IsSelect { get; set; }

    }
}
