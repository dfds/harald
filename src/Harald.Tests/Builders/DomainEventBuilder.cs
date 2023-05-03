using System;
using Harald.WebApi.Domain.Events;
using Newtonsoft.Json.Linq;

namespace Harald.Tests.Builders
{
    public class DomainEventBuilder
    {
        public static string ContextId { get; set; } = Guid.NewGuid().ToString();
        public static string AccountId { get; set; } = "1234567890";
        public static string RoleEmail { get; set; } = "aws.pax-bookings-a43as@dfds.com";

        public static string CapabilityId { get; set; } = Guid.NewGuid().ToString();
        public static string CapabilityName { get; set; } = "pax-bookings";
        public static string CapabilityRootId { get; set; } = "paw-bookings-A43aS";
        public static string ContextName { get; set; } = "default";



        public static IntegrationEvent BuildAWSContextAccountCreatedEventData()
        {

            dynamic data = new JObject();
            data.capabilityId = CapabilityId;
            data.capabilityName = CapabilityName;
            data.capabilityRootId = CapabilityRootId;
            data.contextId = ContextId;
            data.contextName = ContextName;
            data.accountId = AccountId;
            data.roleEmail = RoleEmail;

            return BuildGeneralDomainEvent("aws_context_account_created", data);
        }

        private static IntegrationEvent BuildGeneralDomainEvent(string type, object data)
        {
            var correlationId = Guid.NewGuid().ToString();

            var generalDomainEvent = new IntegrationEvent("1", type, correlationId, "", data);
            return generalDomainEvent;

        }

    }
}