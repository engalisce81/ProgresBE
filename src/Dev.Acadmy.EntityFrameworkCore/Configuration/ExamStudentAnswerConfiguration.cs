using Dev.Acadmy.Exams;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Dev.Acadmy.Configuration
{
    internal class ExamStudentAnswerConfiguration : IEntityTypeConfiguration<ExamStudentAnswer>
    {
        public void Configure(EntityTypeBuilder<ExamStudentAnswer> builder)
        {
            // 1. إعداد اسم الجدول (Prefix + TableName + Schema)
            builder.ToTable(AcadmyConsts.DbTablePrefix + "ExamStudentAnswers" + AcadmyConsts.DbSchema);

            // 2. تطبيق الاتفاقيات الافتراضية لـ ABP
            builder.ConfigureByConvention();

            // 3. علاقة محاولة الامتحان (ExamStudent)
            // عند حذف محاولة الامتحان بالكامل، يتم حذف جميع الإجابات المرتبطة بها (Cascade)
            builder.HasOne(x => x.ExamStudent)
                   .WithMany() // يمكنك إضافة ICollection<ExamStudentAnswer> في كلاس ExamStudent إذا أردت
                   .HasForeignKey(x => x.ExamStudentId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);

            // 4. علاقة السؤال (Question)
            builder.HasOne(x => x.Question)
                   .WithMany()
                   .HasForeignKey(x => x.QuestionId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict);

            // 5. ضبط الخصائص (Properties)
            builder.Property(x => x.TextAnswer)
                   .HasMaxLength(2000); // تحديد طول النص للإجابات المقالية

            builder.Property(x => x.ScoreObtained)
                   .IsRequired()
                   .HasDefaultValue(0);

            builder.Property(x => x.IsCorrect)
                   .IsRequired()
                   .HasDefaultValue(false);

            // 6. الفهارس (Indexes)
            // تسريع استرجاع إجابات محاولة معينة
            builder.HasIndex(x => x.ExamStudentId);
        }
    }
}
