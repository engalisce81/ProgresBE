using Microsoft.Extensions.Hosting;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Dev.Acadmy.Students
{
    public class StudentDto
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
        public Guid? GradeLevelId { get; set; }
        public string GradeLevelName { get; set; }
        public int AccountTypeKey { get; set; }
        public string? StudentMobileIP { get; set; }
        public string LogoUrl { get; set; }
        public ICollection<string> CoursesName { get; set; }=new List<string>();
    }
}
