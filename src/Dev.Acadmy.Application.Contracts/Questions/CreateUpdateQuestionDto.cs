using System;
using System.Collections.Generic;

namespace Dev.Acadmy.Questions
{
    public class CreateUpdateQuestionDto
    {
        public string Title { get; set; }
        public Guid QuestionTypeId { get; set; }
        public Guid QuizId { get; set; }
        public Guid? QuestionBankId { get; set; }
        public int Score { get; set; }
        public string LogoUrl { get; set; }
        public ICollection<CreateUpdateQuestionAnswerDto> Answers { get; set; } = new List<CreateUpdateQuestionAnswerDto>();
    }
}
