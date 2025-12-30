using System;

namespace Dev.Acadmy.Teachers
{
    public class TeacherDto
    {
        public Guid Id { get; set; }
        public string? PhoneNumber { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool Gender { get; set; }
        public Guid CollegeId { get; set; }
        public string CollegeName { get; set; }
        public Guid UniversityId { get; set; }
        public string UniversityName { get; set; }          
        public int AccountTypeKey { get; set; }
        public string LogoUrl { get; set; }
    }
}
