using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Dev.Acadmy.Exams
{
    public class ExamDto:EntityDto<Guid>
    {
        public string Name { get; set; }
        public int TimeExam { get; set; }
        public int Score { get; set; }
        public bool IsActive { get; set; }
        public Guid CourseId { get; set; }
        public string CourseName { get; set; }
        public double PassScore { get; set; }

        // public ICollection<ExamQuestions> ExamQuestions { get; set; } = new List<ExamQuestions>();
    }
}
