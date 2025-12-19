using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace Dev.Acadmy.Entities.Advertisementes.Entities
{
    public class Advertisement : AuditedAggregateRoot<Guid>
    {
        public string Title { get; set; }
        public string TargetUrl { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }

        public Advertisement() { }
        public Advertisement(string title, string targetUrl, DateTime startDate, DateTime endDate, bool isActive)
        {
            Title = title;
            TargetUrl = targetUrl;
            StartDate = startDate;
            EndDate = endDate;
            IsActive = isActive;
        }

    }
}
