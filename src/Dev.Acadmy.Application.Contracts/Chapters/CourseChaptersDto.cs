using Dev.Acadmy.Lectures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Acadmy.Chapters
{
    public class CourseChaptersDto
    {
        
        public Guid ChapterId { get; set; }
        public string ChapterName { get; set; }
        public Guid CourseId { get; set; }
        public string CourseName { get; set; }
        public Guid UserId { get; set; }
        public Guid? ExamId { get; set; }
        public string UserName { get; set; }
        public string LogoUrl { get; set; }
        public int LectureCount { get; set; }
        public List<LectureInfoDto> Lectures { get; set; } = new();
    }
}
