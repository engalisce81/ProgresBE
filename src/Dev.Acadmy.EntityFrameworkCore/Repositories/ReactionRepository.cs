using Dev.Acadmy.Entities.Posts.Entities;
using Dev.Acadmy.EntityFrameworkCore;
using Dev.Acadmy.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Dev.Acadmy.Repositories
{
    public class ReactionRepository : EfCoreRepository<AcadmyDbContext, Reaction, Guid>, IReactionRepository
    {
        public ReactionRepository(IDbContextProvider<AcadmyDbContext> dbContextProvider)
            : base(dbContextProvider) { }
    }
}
