using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Dev.Acadmy.Entities.Chats.Entites;
using Volo.Abp.Identity; // تأكد من وجود هذا الـ Namespace للوصول لـ IdentityUser

namespace Dev.Acadmy.Configuration
{
    public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            // 1. إعداد اسم الجدول (Prefix + Name + Suffix) 
            // لاحظ أنك في كود الـ Lectures استخدمت Prefix في البداية والنهاية، سأتبع نفس نمطك
            builder.ToTable(AcadmyConsts.DbTablePrefix + "ChatMessages" + AcadmyConsts.DbTablePrefix);

            // 2. إعدادات ABP الأساسية (لحقول الـ Audit مثل CreationTime, CreatorId)
            builder.ConfigureByConvention();

            // 3. إعدادات الحقول النصية
            builder.Property(x => x.Message)
                   .IsRequired()
                   .HasMaxLength(2000); // طول مناسب للرسائل

            // 4. إعدادات المعرفات (Guids)
            builder.Property(x => x.ReceverId)
                   .IsRequired();

            builder.Property(x => x.SenderId)
                   .IsRequired();

            // 5. الربط بجدول المستخدمين (IdentityUser)
            // نربط الـ SenderId بجدول المستخدمين الأساسي في ABP
            builder.HasOne<IdentityUser>()
                   .WithMany()
                   .HasForeignKey(x => x.SenderId)
                   .OnDelete(DeleteBehavior.Restrict); // Restrict لضمان عدم ضياع تاريخ الشات إذا حُذف مستخدم

            // 6. تحسين الأداء (Indexes)
            // فهرس على ReceverId لأنك ستبحث دائماً برسائل كورس معين
            builder.HasIndex(x => x.ReceverId);

            // فهرس على وقت الإنشاء لتسريع ترتيب الرسائل من الأقدم للأحدث
            builder.HasIndex(x => x.CreationTime);
        }
    }
}