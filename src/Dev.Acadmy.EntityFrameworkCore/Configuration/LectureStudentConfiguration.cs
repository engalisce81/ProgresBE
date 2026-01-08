using Dev.Acadmy.Lectures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Dev.Acadmy.Configuration
{
    public class LectureStudentConfiguration:IEntityTypeConfiguration<LectureStudent>
    {
        public void Configure(EntityTypeBuilder<LectureStudent> builder)
        {
            builder.ToTable(AcadmyConsts.DbTablePrefix + "LectureStudents" + AcadmyConsts.DbTablePrefix);
            builder.ConfigureByConvention();
            // ضبط الحذف المتتالي للمحاضرة
            builder.HasOne(x => x.Lecture)
                   .WithMany() // أو .WithMany(l => l.LectureStudents) إذا كانت الـ Collection معرفة
                   .HasForeignKey(x => x.LectureId)
                   .OnDelete(DeleteBehavior.Cascade); // هذا السطر هو المطلوب لمسح السجل تلقائياً

            // بالنسبة للمستخدم، يفضل غالباً استخدام Restrict أو NoAction 
            // لمنع حذف المستخدم إذا كان له سجلات حضور (أمان للبيانات)
            builder.HasOne(x => x.User)
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.NoAction);

        }
    }
}
