using System;
using Newtonsoft.Json.Linq;

namespace Harald.WebApi.Domain.Events
{
    public class AWSContextAccountCreatedDomainEvent : IDomainEvent<AWSContextAccountCreatedData>
    {
        public AWSContextAccountCreatedDomainEvent(IntegrationEvent integrationEvent)
        {
            Payload = (integrationEvent.Payload as JObject)?.ToObject<AWSContextAccountCreatedData>();
        }

        public AWSContextAccountCreatedData Payload { get; }
    }


    public class AWSContextAccountCreatedData
    {
        public Guid CapabilityId { get; }
        public string CapabilityName { get; }
        public string CapabilityRootId { get; }
        public Guid ContextId { get; }
        public string ContextName { get; }
        public string AccountId { get;  }
        public string RoleEmail { get;  }

        public AWSContextAccountCreatedData(Guid capabilityId, string capabilityName, string capabilityRootId, Guid contextId, string contextName, string accountId, string roleEmail)
        {
            CapabilityId = capabilityId;
            CapabilityName = capabilityName;
            CapabilityRootId = capabilityRootId;
            ContextId = contextId;
            ContextName = contextName;
            AccountId = accountId;
            RoleEmail = roleEmail;

        }
    }
}