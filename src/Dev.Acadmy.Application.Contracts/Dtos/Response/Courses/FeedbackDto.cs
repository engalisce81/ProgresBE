using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Dev.Acadmy.Dtos.Response.Courses
{
    public class FeedbackDto:EntityDto<Guid>
    {
        public Guid UserId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public string UserName { get; set; }
        public string LogoUrl { get; set; }
    }
}
