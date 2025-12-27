using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Dev.Acadmy.Lectures;

namespace Dev.Acadmy.Configuration
{
    public class LectureConfiguration : IEntityTypeConfiguration<Lecture>
    {
        public void Configure(EntityTypeBuilder<Lecture> builder)
        {
            builder.ToTable(AcadmyConsts.DbTablePrefix + "Lectures" + AcadmyConsts.DbTablePrefix);
            builder.ConfigureByConvention();

            builder.Property(x => x.Title)
                   .IsRequired()
                   .HasMaxLength(250);

            builder.Property(x => x.Content)
                   .HasMaxLength(5000);

            builder.Property(x => x.YouTubeVideoUrl)
                   .HasMaxLength(1000);

            builder.Property(x => x.DriveVideoUrl)
                   .HasMaxLength(1000);

            builder.Property(x => x.IsVisible)
                   .HasDefaultValue(false);

            builder.HasOne(x => x.Chapter)
                   .WithMany(x=>x.Lectures)
                   .HasForeignKey(x => x.ChapterId)
                   .OnDelete(DeleteBehavior.Cascade);

        }
    }
    
    public class LectureTryConfiguration : IEntityTypeConfiguration<LectureTry>
    {
        public void Configure(EntityTypeBuilder<LectureTry> builder)
        {
            builder.ToTable(AcadmyConsts.DbTablePrefix + "LectureTrys" + AcadmyConsts.DbTablePrefix);
            builder.ConfigureByConvention();

            

            builder.HasOne(x => x.User)
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Lecture)
                   .WithMany()
                   .HasForeignKey(x => x.LectureId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}