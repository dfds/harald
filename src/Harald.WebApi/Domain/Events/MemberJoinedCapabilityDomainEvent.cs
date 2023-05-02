using System;
using Newtonsoft.Json.Linq;

namespace Harald.WebApi.Domain.Events
{
    public class MemberJoinedCapabilityDomainEvent : IDomainEvent<MemberJoinedCapabilityData>
    {
        public MemberJoinedCapabilityData Payload { get; }

        private MemberJoinedCapabilityDomainEvent(MemberJoinedCapabilityData payload)
        {
            Payload = payload;
        }
        public MemberJoinedCapabilityDomainEvent(IntegrationEvent integrationEvent)
        {
            Payload = (integrationEvent.Payload as JObject)?.ToObject<MemberJoinedCapabilityData>();
        }

        public static MemberJoinedCapabilityDomainEvent Create(
            string capabilityId,
            string memberEmail)
        {
            var data = new MemberJoinedCapabilityData(capabilityId, memberEmail);
            
            return new MemberJoinedCapabilityDomainEvent(data);
        }
    }

    public class MemberJoinedCapabilityData
    {
        public string CapabilityId { get; }
        public string MemberEmail { get; }

        public MemberJoinedCapabilityData(string capabilityId, string memberEmail)
        {
            CapabilityId = capabilityId;
            MemberEmail = memberEmail;
        }
    }
}