using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Acadmy.Dtos.Response.Exams
{
    public class ExamAnswerDetailDto
    {
        public Guid QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }
        public string LogoUrl { get; set; }
        // بيانات الطالب
        public string? StudentTextAnswer { get; set; }
        public Guid? StudentSelectedAnswerId { get; set; }

        // البيانات الصحيحة
        public string? CorrectTextAnswer { get; set; }
        public Guid? CorrectSelectedAnswerId { get; set; }

        // قائمة الخيارات (اختياري لعرض الخيارات كاملة في الواجهة)
        public List<ExamQuestionAnswerDto> AllOptions { get; set; } = new();

        public double ScoreObtained { get; set; }
        public bool IsCorrect { get; set; }
    }
}
