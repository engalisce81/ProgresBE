using Dev.Acadmy.Entities.Courses.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
namespace Dev.Acadmy.EntityFrameworkCore.Configurations
{
    public class CourseFeedbackConfiguration : IEntityTypeConfiguration<CourseFeedback>
    {
        public void Configure(EntityTypeBuilder<CourseFeedback> builder)
        {
            // ضبط اسم الجدول بنفس نمط التسمية في مشروعك
            builder.ToTable(AcadmyConsts.DbTablePrefix + "CourseFeedbacks" + AcadmyConsts.DbTablePrefix);

            // ضبط الخصائص الأساسية لـ ABP (ExtraProperties, ConcurrencyStamp, etc.)
            builder.ConfigureByConvention();

            // ضبط الخصائص (Properties)
            builder.Property(x => x.Comment).IsRequired().HasMaxLength(1000);
            builder.Property(x => x.Rating).IsRequired();
            builder.Property(x => x.IsAccept).HasDefaultValue(false);

            // ضبط العلاقات (Relationships)

            // علاقة التقييم بالكورس
            builder.HasOne(x=>x.Course)
                   .WithMany(x => x.Feedbacks)
                   .HasForeignKey(x => x.CourseId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);

            // علاقة التقييم بالمستخدم
            builder.HasOne(x => x.User)
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.NoAction);

            // إضافة Index لمنع تكرار التقييم لنفس المستخدم على نفس الكورس
            builder.HasIndex(x => new { x.CourseId, x.UserId }).IsUnique();
        }
    }
}