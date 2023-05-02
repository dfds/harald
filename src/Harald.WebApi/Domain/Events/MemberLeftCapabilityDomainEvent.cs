using System;
using Newtonsoft.Json.Linq;

namespace Harald.WebApi.Domain.Events
{
    public class MemberLeftCapabilityDomainEvent : IDomainEvent<MemberLeftCapabilityData>
    {
        public MemberLeftCapabilityData Payload { get; }
        public MemberLeftCapabilityDomainEvent(MemberLeftCapabilityData payload)
        {
            Payload = payload;
        }
        
        public MemberLeftCapabilityDomainEvent(IntegrationEvent IntegrationEvent)
        {
            Payload = (IntegrationEvent.Payload as JObject)?.ToObject<MemberLeftCapabilityData>();
        }

        public static MemberLeftCapabilityDomainEvent Create(string capabilityId, string memberEmail)
        {
            var payload = new MemberLeftCapabilityData(capabilityId, memberEmail);
            
            return new MemberLeftCapabilityDomainEvent(payload);
        }
    }

    public class MemberLeftCapabilityData
    {
        public string CapabilityId { get; private set; }
        public string MemberEmail { get; private set; }

        public MemberLeftCapabilityData(string capabilityId, string memberEmail)
        {
            CapabilityId = capabilityId;
            MemberEmail = memberEmail;
        }
    }
}