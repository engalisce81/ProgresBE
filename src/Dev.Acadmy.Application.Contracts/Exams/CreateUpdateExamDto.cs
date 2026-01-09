using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Acadmy.Exams
{
    public class CreateUpdateExamDto
    {
        public string Name { get; set; }
        public int TimeExam { get; set; }
        public int Score { get; set; }
        public bool IsActive { get; set; }
        public Guid CourseId { get; set; }
        public double PassScore { get; set; }

        //public ICollection<Guid> QuestionBankIds { get; set; } = new List<Guid>();
        //  public ICollection<Guid> QuestionIds { get; set; } = new List<Guid>();
    }
}
