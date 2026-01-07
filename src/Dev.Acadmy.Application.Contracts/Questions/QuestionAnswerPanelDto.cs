using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Acadmy.Questions
{
    public class QuestionAnswerPanelDto
    {
        public Guid Id { get; set; }
        public string Answer { get; set; }
        public bool IsCorrect { get; set; }
        public bool IsSelectedByStudent { get; set; } // هل اختاره الطالب؟
    }
}
