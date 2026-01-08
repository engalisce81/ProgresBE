using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Dev.Acadmy.Quizzes;
using Dev.Acadmy.Lectures;


namespace Dev.Acadmy.Configuration
{
    public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
    {
        public void Configure(EntityTypeBuilder<Quiz> builder)
        {
            builder.ToTable(AcadmyConsts.DbTablePrefix + "Quizes" + AcadmyConsts.DbTablePrefix);
            builder.ConfigureByConvention();

            builder.Property(x => x.Title)
                   .IsRequired()
                   .HasMaxLength(250);

            builder.Property(x => x.Description)
                   .HasMaxLength(2000);

            builder.Property(x => x.QuizTime)
                   .IsRequired();

            builder.HasOne(q => q.Lecture)
                 .WithMany(l => l.Quizzes)
                 .HasForeignKey(l => l.LectureId).IsRequired(false)
                 .OnDelete(DeleteBehavior.SetNull);
        }
    }

    public class QuizStudentConfiguration : IEntityTypeConfiguration<QuizStudent>
    {
        public void Configure(EntityTypeBuilder<QuizStudent> builder)
        {
            builder.ToTable(AcadmyConsts.DbTablePrefix + "QuizStudent" + AcadmyConsts.DbTablePrefix);

            builder.HasIndex(x => new { x.UserId, x.QuizId }).IsUnique();

            builder.HasOne(x => x.User)
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Quiz)
                   .WithMany()
                   .HasForeignKey(x => x.QuizId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.Score).HasDefaultValue(0);
        }
    }
}
