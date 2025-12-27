using Dev.Acadmy.Entities.Reports;
using Dev.Acadmy.Entities.Reports.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Dev.Acadmy.EntityFrameworkCore.Configurations
{
    public class UserReportConfiguration : IEntityTypeConfiguration<UserReport>
    {
        public void Configure(EntityTypeBuilder<UserReport> builder)
        {
            // 1. إعدادات ABP الأساسية (تتعامل مع جداول التدقيق Auditing والـ Keys)
            builder.ConfigureByConvention();

            // 2. إعداد اسم الجدول (اختياري، الافتراضي هو نفس اسم الكلاس)
            builder.ToTable("AppUserReports");

            // 3. إعداد الحقول والنصوص
            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(128); // تحديد طول العنوان

            builder.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(2000); // وصف مفصل قد يصل لـ 2000 حرف

            // 4. إعداد الـ Enums (يتم تخزينها كأرقام بشكل افتراضي، وهو الأفضل للأداء)
            builder.Property(x => x.Type).IsRequired();
            builder.Property(x => x.Status).IsRequired().HasDefaultValue(Enums.ReportStatus.Pending);

            // 5. إعداد العلاقات (اختياري إذا أردت ربط الـ Report بجدول المستخدمين بشكل صريح)
            
            builder.HasOne(x=>x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            

            // 6. إضافة الفهارس (Indexes) لسرعة البحث
            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.Status);
        }
    }
}