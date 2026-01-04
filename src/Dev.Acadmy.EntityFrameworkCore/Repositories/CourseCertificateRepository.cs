using Dev.Acadmy.Entities.Courses.Entities;
using Dev.Acadmy.EntityFrameworkCore;
using Dev.Acadmy.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;

namespace Dev.Acadmy.Repositories
{
    public class CourseCertificateRepository : EfCoreRepository<AcadmyDbContext, CourseCertificate, Guid>, ICourseCertificateRepository
    {
        public CourseCertificateRepository(IDbContextProvider<AcadmyDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<byte[]> GeneratePdfWithTextAsync(string templateUrl, string text, double xPercent, double yPercent)
        {
            var fileName = Path.GetFileName(templateUrl);
            var rootPath = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(rootPath, "wwwroot", "images", fileName);

            if (!File.Exists(filePath)) throw new FileNotFoundException($"Template not found at: {filePath}");

            using (var ms = new MemoryStream())
            {
                using (var reader = new PdfReader(filePath))
                using (var writer = new PdfWriter(ms))
                using (var pdfDoc = new PdfDocument(reader, writer))
                {
                    using (var document = new iText.Layout.Document(pdfDoc))
                    {
                        var page = pdfDoc.GetFirstPage();
                        var pageSize = page.GetPageSize();

                        // حساب الإحداثيات
                        // بالنسبة لـ X: لجعل التوسيط يعمل على كامل العرض، نضع x عند الصفر ونستخدم الـ Margin للتحريك
                        float x = (float)(pageSize.GetWidth() * xPercent / 100);
                        float y = (float)(pageSize.GetHeight() * (100 - yPercent) / 100);

                        // إنشاء الفقرة
                        iText.Layout.Element.Paragraph p = new iText.Layout.Element.Paragraph(text)
                            .SetFontSize(30)
                            // التعديل هنا:
                            // نجعل الصندوق يبدأ من الصفر وعرضه هو عرض الصفحة كاملاً
                            // ثم نستخدم الـ x المحسوبة كإزاحة أو نعتمد على التوسيط المطلق
                            .SetFixedPosition(0, y, pageSize.GetWidth())
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetFontColor(iText.Kernel.Colors.ColorConstants.BLACK);

                        /* ملاحظة: إذا كنت تريد أن يتبع النص الـ X المتحركة (ليس فقط السنتر 50):
                           نستخدم صندوق نصي صغير (مثلاً 400 نقطة) ونضعه حول نقطة الـ X
                        */
                        // float boxWidth = 400f;
                        // p.SetFixedPosition(x - (boxWidth / 2), y, boxWidth);

                        document.Add(p);
                        document.Close();
                    }
                }
                return ms.ToArray();
            }
        }

    }
    
}