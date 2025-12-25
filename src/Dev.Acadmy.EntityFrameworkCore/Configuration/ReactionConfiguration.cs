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
    public class ReactionConfiguration : IEntityTypeConfiguration<Reaction>
    {
        public void Configure(EntityTypeBuilder<Reaction> builder)
        {
            builder.ToTable(AcadmyConsts.DbTablePrefix + "Reactions", AcadmyConsts.DbSchema);
            builder.ConfigureByConvention();

            // علاقة التفاعل بالمنشور
            builder.HasOne(x => x.Post)
                   .WithMany(p => p.Reactions)
                   .HasForeignKey(x => x.PostId)
                   .OnDelete(DeleteBehavior.Cascade);

            // علاقة التفاعل بالمستخدم
            builder.HasOne(x => x.User)
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.NoAction);

            // إضافة Index فريد لضمان أن المستخدم لا يتفاعل أكثر من مرة بنفش النوع على نفس البوست (اختياري)
            builder.HasIndex(x => new { x.PostId, x.UserId, x.Type });
        }
    }
}
