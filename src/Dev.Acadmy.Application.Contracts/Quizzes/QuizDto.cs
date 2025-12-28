using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Dev.Acadmy.Quizzes
{
    public class QuizDto:EntityDto<Guid>
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int QuizTime { get; set; }
        public int QuizTryCount { get; set; }
        public string LeactureName { get; set; }
        public string CourseName { get; set; }

    }
}
