using Dev.Acadmy.Entities.Posts.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Dev.Acadmy.Configuration
{
    public class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.ToTable(AcadmyConsts.DbTablePrefix + "Posts", AcadmyConsts.DbSchema);
            builder.ConfigureByConvention(); // مهم جداً لـ ABP لضبط الـ Audit properties

            builder.Property(x => x.Title).IsRequired().HasMaxLength(256);
            builder.Property(x => x.Content).IsRequired();

            // علاقة المنشور بالمستخدم
            builder.HasOne(x => x.User)
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Restrict); // منع حذف المستخدم إذا كان له منشورات (أو حسب رغبتك)
        }
    }
}
