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
    public string CapabilityId { get; }
    public string CapabilityName { get; }
    public string CapabilityRootId { get; }
    public string ContextId { get; }
    public string ContextName { get; }
    public string AccountId { get;  }
        public string RoleArn { get; }
        public string RoleEmail { get;  }

        public AWSContextAccountCreatedData(string capabilityId, string capabilityName, string capabilityRootId, string contextId, string contextName, string accountId, string roleArn, string roleEmail)
        {
            CapabilityId = capabilityId;
            CapabilityName = capabilityName;
            CapabilityRootId = capabilityRootId;
            ContextId = contextId;
            ContextName = contextName;
            AccountId = accountId;
            RoleArn = roleArn;
            RoleEmail = roleEmail;

        }
    }
}