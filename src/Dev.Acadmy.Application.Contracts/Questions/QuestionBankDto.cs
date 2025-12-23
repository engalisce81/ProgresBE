using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Dev.Acadmy.Questions
{
    public class QuestionBankDto:EntityDto<Guid>
    {
        public string Name { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
    }
}
