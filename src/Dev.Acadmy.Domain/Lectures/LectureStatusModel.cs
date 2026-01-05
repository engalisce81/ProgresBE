using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Acadmy.Lectures
{
    public class LectureStatusModel // هذا Domain Model وليس DTO
    {
        public int MyTryCount { get; set; }
        public int LectureTryCount { get; set; }
        public bool IsSucces { get; set; }
        public double SuccessQuizRate { get; set; }
        public double MyScoreRate { get; set; }
    }
}
