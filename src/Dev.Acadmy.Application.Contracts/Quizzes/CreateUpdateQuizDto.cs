using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Acadmy.Quizzes
{
    public class CreateUpdateQuizDto
    {
        public Guid? CreaterId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int QuizTime { get; set; }
        public int QuizTryCount { get; set; }
        public Guid? LectureId { get; set; }
        public Guid? CourseId { get; set; }
    }
}
