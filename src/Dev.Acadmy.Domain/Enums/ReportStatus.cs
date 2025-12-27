using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Acadmy.Enums
{
    public enum ReportStatus
    {
        Pending = 1,      // قيد الانتظار
        UnderReview = 2,  // تحت المراجعة
        Resolved = 3,     // تم الحل / المعالجة
        Rejected = 4      // مرفوض
    }
}
