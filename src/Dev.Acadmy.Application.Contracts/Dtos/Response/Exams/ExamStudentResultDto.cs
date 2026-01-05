using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Acadmy.Dtos.Response.Exams
{
    public class ExamStudentResultDto
    {
        public Guid ExamId { get; set; }
        public string ExamTitle { get; set; }
        public double StudentScore { get; set; }
        public bool IsPassed { get; set; }
        public DateTime FinishedAt { get; set; }
        public List<ExamAnswerDetailDto> Answers { get; set; } = new();
    }
}
