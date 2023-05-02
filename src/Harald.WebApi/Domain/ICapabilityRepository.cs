using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Harald.WebApi.Domain
{
    public interface ICapabilityRepository
    {
        Task<IEnumerable<Capability>> GetAll();
        
        Task<IEnumerable<Capability>> GetById(string id);
        Task<IEnumerable<Capability>> GetByFilter(Expression<Func<Capability, bool>> filter);
        Task Add(Capability capability);
        Task Update(Capability capability);
        Task Remove(Capability capability);
    }
}