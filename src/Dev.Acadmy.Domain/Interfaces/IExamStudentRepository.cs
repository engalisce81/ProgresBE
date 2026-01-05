using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Acadmy.Interfaces
{
    using global::Dev.Acadmy.Exams;
    using System;
    using Volo.Abp.Domain.Repositories;

    namespace Dev.Acadmy.Exams
    {
        // يجب أن يكون Interface ويرث من IRepository لكي يوفر لك ميثودز ABP الجاهزة
        public interface IExamStudentRepository : IRepository<ExamStudent, Guid>
        {
            // يمكنك هنا إضافة ميثودز مخصصة غير موجودة في الـ Repository الافتراضي
            // مثال: جلب محاولة الطالب مع بيانات الامتحان في استعلام واحد
            Task<ExamStudent> GetWithDetailsAsync(Guid examId, Guid userId);
        }
    }
}
