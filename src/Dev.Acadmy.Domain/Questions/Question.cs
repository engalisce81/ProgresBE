using Dev.Acadmy.Quizzes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace Dev.Acadmy.Questions
{
    public class Question :AuditedAggregateRoot<Guid>
    {
        public string Title { get; set; }
        public Guid QuestionTypeId { get; set; }
        public Guid QuizId { get; set; }
        public Guid? QuestionBankId { get; set; }
        public int Score { get; set; }
        [ForeignKey(nameof(QuizId))]
        public Quiz Quiz { get; set;}
        [ForeignKey(nameof(QuestionTypeId))]
        public QuestionType QuestionType { get; set; }
        [ForeignKey(nameof(QuestionBankId))]
        public QuestionBank? QuestionBank { get; set; }
        public ICollection<QuestionAnswer> QuestionAnswers { get; set; }=new List<QuestionAnswer>();
    }
}
