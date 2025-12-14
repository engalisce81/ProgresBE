using Dev.Acadmy.Dtos.Response.Teachers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Acadmy.Dtos.Response.Subjects
{
    public class SubjectWithTeachersDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<TeacherTopDto> Teachers { get; set; } = new List<TeacherTopDto>();

    }
}
