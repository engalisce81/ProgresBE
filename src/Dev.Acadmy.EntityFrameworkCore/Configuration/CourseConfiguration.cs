using Dev.Acadmy.Entities.Courses.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Dev.Acadmy.Configuration
{
    public class CourseConfiguration : IEntityTypeConfiguration<Entities.Courses.Entities.Course>
    {
        public void Configure(EntityTypeBuilder<Entities.Courses.Entities.Course> builder)
        {
            builder.ToTable(AcadmyConsts.DbTablePrefix + "Courses" + AcadmyConsts.DbSchema);

            // Indexes
            builder.HasIndex(x => x.Name); // لتسريع البحث بالاسم
            builder.HasIndex(x => new { x.IsQuiz, x.IsPdf }); // Index مركب لتسريع فلترة الأنواع
            builder.HasIndex(x => x.CreationTime); // لتسريع الترتيب OrderByDescending

            builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Description).HasMaxLength(2000);
            builder.HasOne(x => x.College).WithMany().HasForeignKey(x => x.CollegeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x=>x.Quizzes).WithOne(x=>x.Course).HasForeignKey(x=>x.CourseId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
        }

    }

    public class CourseStudentsConfiguration : IEntityTypeConfiguration<CourseStudent>
    {
        public void Configure(EntityTypeBuilder<CourseStudent> builder)
        {
            builder.ToTable(AcadmyConsts.DbTablePrefix + "CourseStudentes" + AcadmyConsts.DbTablePrefix);
            builder.ConfigureByConvention();

            builder.HasIndex(x => new { x.UserId, x.CourseId }).IsUnique();

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Course)
                .WithMany()
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.IsSubscibe).HasDefaultValue(false);
        }
    }
}
