using Dev.Acadmy.Entities.Courses.Entities;
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
    public class CourseCertificateConfiguration : IEntityTypeConfiguration<CourseCertificate>
    {
        public void Configure(EntityTypeBuilder<CourseCertificate> builder)
        {
            // تحديد اسم الجدول في قاعدة البيانات
            builder.ToTable(AcadmyConsts.DbTablePrefix + "CourseCertificates" + AcadmyConsts.DbSchema);

            // تطبيق الاصطلاحات القياسية لـ ABP (مثل الـ ConcurrencyStamp و ExtraProperties)
            builder.ConfigureByConvention();

            // إعدادات الخصائص
            builder.Property(x => x.NameXPosition).IsRequired();
            builder.Property(x => x.NameYPosition).IsRequired();

            // ضبط العلاقة مع الكورس (واحد لواحد)
            // كل كورس له شهادة واحدة فقط
            builder.HasOne(x => x.Course)
                   .WithOne(x => x.CourseCertificate)
                   .HasForeignKey<CourseCertificate>(x => x.CourseId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade); // حذف الشهادة عند حذف الكورس

            // إنشاء Index على CourseId لتحسين سرعة البحث
            builder.HasIndex(x => x.CourseId);
        }
    }
}