using Dev.Acadmy.Quizzes;
using System;
using System.Collections.Generic;

namespace Dev.Acadmy.Lectures
{
    public class LectureWithQuizzesDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public List<QuizWithQuestionsDto> Quizzes { get; set; } = new();
    }
}
