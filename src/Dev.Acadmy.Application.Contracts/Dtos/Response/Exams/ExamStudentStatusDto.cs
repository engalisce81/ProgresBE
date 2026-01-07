using System;

namespace Dev.Acadmy.Dtos.Response.Exams
{
    public class ExamStudentStatusDto
    {
        public Guid ExamId { get; set; }    
        public bool IsPassed { get; set; }
        public double Score { get; set; }
        public bool IsCertificateIssued { get; set; }
        public bool CanRequestNow { get; set; }
        public DateTime? NextAvailableDate { get; set; }
    }
}
