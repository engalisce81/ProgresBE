using System;

namespace Dev.Acadmy.Quizzes
{
    public class QuizInfoDto
    {
        public Guid QuizId { get; set; }
        public string Title { get; set; }
        public int QuestionsCount { get; set; }
        public int QuizTryCount { get; set; }
        public int TryedCount { get; set; }
        public bool IsSucces { get; set; }
        public bool AlreadyAnswer { get; set; }
    }
}
