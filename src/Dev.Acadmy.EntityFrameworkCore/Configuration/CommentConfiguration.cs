using Dev.Acadmy.Entities.Posts.Entities;
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
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.ToTable(AcadmyConsts.DbTablePrefix + "Comments", AcadmyConsts.DbSchema);
            builder.ConfigureByConvention();

            builder.Property(x => x.Text).IsRequired().HasMaxLength(1000);

            // علاقة التعليق بالمنشور
            builder.HasOne(x => x.Post)
                   .WithMany(p => p.Comments) // لاحظت وجود خطأ مطبعي في الكيان عندك (Commenfts)
                   .HasForeignKey(x => x.PostId)
                   .OnDelete(DeleteBehavior.Cascade); // إذا حُذف المنشور تُحذف تعليقاته

            // علاقة التعليق بالمستخدم
            builder.HasOne(x => x.User)
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.NoAction); // لتجنب تداخل الـ Cascade
        }
    }
}
