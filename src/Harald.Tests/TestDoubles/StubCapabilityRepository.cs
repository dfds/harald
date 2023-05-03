using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harald.WebApi.Domain;
using Harald.Infrastructure.Slack;
using System.Linq.Expressions;

namespace Harald.Tests.TestDoubles
{
    public class StubCapabilityRepository : ICapabilityRepository
    {
        private readonly List<Capability> _capabilities = new List<Capability>();

        public StubCapabilityRepository(List<string> capabilityIds)
        {
            foreach (var capabilityId in capabilityIds)
            {
                _capabilities.Add(
                    Capability.Create(
                        id: capabilityId,
                        name: "FooCapability",
                        slackChannelId: "FooChannelId",
                        slackUserGroupId: "FooUserGroupId"
                    ));
            }
        }

        public StubCapabilityRepository()
        {
        }

        public Task<IEnumerable<Capability>> GetById(string id)
        {
            var capabilities = _capabilities.Where(c => c.Id == id);

            return Task.FromResult(capabilities);
        }

        public Task Add(Capability capability)
        {
            _capabilities.Add(capability);
            return Task.CompletedTask;
        }

        public Task Update(Capability capability)
        {
            return Task.CompletedTask;
        }

        public Task Remove(Capability capability)
        {
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Capability>> GetAll()
        {
            return Task.FromResult(_capabilities.AsEnumerable());
        }

        public async Task<IEnumerable<Capability>> GetByFilter(Expression<Func<Capability, bool>> filter)
        {
            var matches = await GetAll();

            return matches.AsQueryable().Where(filter);
        }
    }
}