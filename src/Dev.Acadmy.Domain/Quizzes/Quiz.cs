using Dev.Acadmy.Entities.Courses.Entities;
using Dev.Acadmy.Lectures;
using Dev.Acadmy.Questions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace Dev.Acadmy.Quizzes
{
    public class Quiz : AuditedAggregateRoot<Guid>
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int QuizTime { get; set; }
        public int QuizTryCount { get; set; }
        public Guid? CourseId { get; set; }
        public Guid? LectureId { get; set; }
        [ForeignKey(nameof(LectureId))] 
        public Lecture? Lecture { get; set; }
        [ForeignKey(nameof(CourseId))]
        public Entities.Courses.Entities.Course? Course { get; set; }
        public ICollection<Question> Questions { get; set; } = new List<Question>();    
    }
}
