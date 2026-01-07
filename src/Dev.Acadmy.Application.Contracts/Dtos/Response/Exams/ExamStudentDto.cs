using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Acadmy.Dtos.Response.Exams
{
    public class ExamStudentDto
    {
        public Guid ExamId { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } // مضاف
        public string? LogoUrl { get; set; } // مضاف
        public double Score { get; set; }
        public int TryCount { get; set; }
        public bool IsPassed { get; set; }
        public DateTime FinishedAt { get; set; }
        public bool IsCertificateIssued { get; set; }
    }
}
