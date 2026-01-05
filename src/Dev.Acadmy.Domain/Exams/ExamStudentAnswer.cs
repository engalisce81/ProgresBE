using Dev.Acadmy.Questions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace Dev.Acadmy.Exams
{
    public class ExamStudentAnswer : AuditedAggregateRoot<Guid>
    {
        public Guid ExamStudentId { get; set; } // الربط مع محاولة الامتحان الشامل
        public Guid QuestionId { get; set; }
        public Guid? SelectedAnswerId { get; set; }
        public string? TextAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public double ScoreObtained { get; set; }

        [ForeignKey(nameof(ExamStudentId))]
        public ExamStudent ExamStudent { get; set; }

        [ForeignKey(nameof(QuestionId))]
        public Question Question { get; set; }
    }
}
