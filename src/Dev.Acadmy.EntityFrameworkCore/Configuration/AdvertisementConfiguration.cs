using Dev.Acadmy.Entities.Advertisementes.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Dev.Acadmy.EntityFrameworkCore
{
    public class AdvertisementConfiguration : IEntityTypeConfiguration<Advertisement>
    {
        public void Configure(EntityTypeBuilder<Advertisement> builder)
        {
            // 1. تحديد اسم الجدول في قاعدة البيانات مع الـ Prefix الخاص بالمشروع
            builder.ToTable(AcadmyConsts.DbTablePrefix + "Advertisements" + AcadmyConsts.DbSchema);

            // 2. تطبيق الإعدادات الافتراضية لـ ABP (مثل الـ Keys والـ Audit properties)
            builder.ConfigureByConvention();

            // 3. إعدادات الحقول (اختياري ولكن يفضل للـ Best Practices)
            builder.Property(x => x.Title).IsRequired().HasMaxLength(256);
            builder.Property(x => x.TargetUrl).IsRequired();

            // 4. إضافة Index على التواريخ والنشاط لأننا نستخدمهم كثيراً في الفلترة
            builder.HasIndex(x => new { x.IsActive, x.StartDate, x.EndDate });
        }
    }
}

