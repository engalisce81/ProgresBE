using Dev.Acadmy.Exams;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Identity;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Dev.Acadmy.Configuration
{
    public class ExamStudentConfiguration : IEntityTypeConfiguration<ExamStudent>
    {
        public void Configure(EntityTypeBuilder<ExamStudent> builder)
        {
            // 1. إعداد اسم الجدول (Prefix + TableName + Schema)
            builder.ToTable(AcadmyConsts.DbTablePrefix + "ExamStudents" + AcadmyConsts.DbSchema);

            // 2. تطبيق الاتفاقيات الافتراضية لـ ABP (Audited properties, etc.)
            builder.ConfigureByConvention();

            // 3. علاقة الطالب (User)
            // هنا نربط المحاولة بالمستخدم، ونستخدم Restrict لمنع حذف المستخدم إذا كان له سجلات امتحانات
            builder.HasOne(x => x.User)
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict);

            // 4. علاقة الامتحان (Exam)
            // نربط المحاولة بالامتحان الشامل
            builder.HasOne(x => x.Exam)
                   .WithMany()
                   .HasForeignKey(x => x.ExamId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict);

            // 5. ضبط الخصائص (Properties)
            builder.Property(x => x.Score).IsRequired().HasDefaultValue(0);
            builder.Property(x => x.TryCount).IsRequired().HasDefaultValue(0);
            builder.Property(x => x.IsPassed).IsRequired().HasDefaultValue(false);
            builder.Property(x => x.FinishedAt).IsRequired();

            // 6. إضافة الفهارس (Indexes) للسرعة في البحث
            builder.HasIndex(x => new { x.UserId, x.ExamId });
        }
    }
}
