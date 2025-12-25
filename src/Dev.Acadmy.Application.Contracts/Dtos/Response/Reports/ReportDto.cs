using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Dev.Acadmy.Dtos.Response.Reports
{
    public class ReportDto : EntityDto<Guid>
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }

        // Metadata for the UI
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public DateTime CreationTime { get; set; }

        // Reference to the reported item
        public Guid? TargetEntityId { get; set; }
    }
}
