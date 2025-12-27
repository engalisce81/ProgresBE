using Dev.Acadmy.Entities.Posts.Entities;
using System;
using Volo.Abp.Domain.Repositories;

namespace Dev.Acadmy.Interfaces
{
    public interface IReactionRepository : IRepository<Reaction, Guid>
    {
    }
}
