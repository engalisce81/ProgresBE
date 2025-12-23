using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Dev.Acadmy.Questions;

namespace Dev.Acadmy.Configuration
{
    public class QuestionConfiguration : IEntityTypeConfiguration<Question>
    {
        public void Configure(EntityTypeBuilder<Question> builder)
        {
            builder.ToTable(AcadmyConsts.DbTablePrefix + "Questiones" + AcadmyConsts.DbTablePrefix);
            builder.ConfigureByConvention();

            builder.Property(x => x.Title)
                   .IsRequired()
                   .HasMaxLength(1000);

            builder.Property(x => x.Score)
                   .IsRequired();

            builder.HasOne(x => x.Quiz)
                   .WithMany(x=>x.Questions)
                   .HasForeignKey(x => x.QuizId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.QuestionType)
                   .WithMany(qt => qt.Questions)
                   .HasForeignKey(x => x.QuestionTypeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.QuestionBank)
                   .WithMany(qb => qb.Questions)
                   .HasForeignKey(x => x.QuestionBankId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
    public class QuestionAnswerConfiguration : IEntityTypeConfiguration<QuestionAnswer>
    {
        public void Configure(EntityTypeBuilder<QuestionAnswer> builder)
        {
            builder.ToTable(AcadmyConsts.DbTablePrefix + "QuestionAnsweres" + AcadmyConsts.DbTablePrefix);

            builder.Property(x => x.Answer)
                   .IsRequired()
                   .HasMaxLength(1000);

            builder.HasOne(x => x.Question)
                   .WithMany(q => q.QuestionAnswers)
                   .HasForeignKey(x => x.QuestionId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.IsCorrect).HasDefaultValue(false);
        }
    }

    public class QuestionBankConfiguration : IEntityTypeConfiguration<QuestionBank>
    {
        public void Configure(EntityTypeBuilder<QuestionBank> builder)
        {
            builder.ToTable(AcadmyConsts.DbTablePrefix + "QuestionBanks" + AcadmyConsts.DbTablePrefix);

            builder.HasOne(qb => qb.User)
                   .WithMany()
                   .HasForeignKey(qb => qb.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(qb => qb.UserId);
        }
    }
    public class QuestionTypeConfiguration : IEntityTypeConfiguration<QuestionType>
    {
        public void Configure(EntityTypeBuilder<QuestionType> builder)
        {
            builder.ToTable(AcadmyConsts.DbTablePrefix + "QuestionTypes" + AcadmyConsts.DbTablePrefix);

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(200);
        }
    }
}
