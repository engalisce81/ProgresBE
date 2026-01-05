using Dev.Acadmy.Questions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Acadmy.Quizzes
{
    public class QuizFullDetailModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public int QuizTime { get; set; }
        public int TryCount { get; set; }
        public List<Question> Questions { get; set; }
    }
}
