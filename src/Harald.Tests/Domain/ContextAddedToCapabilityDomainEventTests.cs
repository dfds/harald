using System;
using Harald.WebApi.Domain.Events;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Harald.Tests.Domain
{
    public class ContextAddedToCapabilityDomainEventTests
    {
        [Fact]
        public void can_create_valid_ContextAddedToCapabilityDomainEvent_when_given_GeneralDomainEvent()
        {
            // Arrange
            var CapabilityId = Guid.NewGuid().ToString();
            var CapabilityName = "barbar";
            var CapabilityRootId = "barbar-Skak";
            var ContextId = Guid.NewGuid().ToString();
            var ContextName = "foo";

            dynamic data = new JObject();
            data.capabilityId = CapabilityId;
            data.capabilityName = CapabilityName;
            data.capabilityRootId = CapabilityRootId;
            data.contextId = ContextId;
            data.contextName = ContextName;


            var messageId = Guid.NewGuid();
            var generalDomainEvent = new IntegrationEvent(
                version: "1",
                eventName: "context_added_to_capability",
                xCorrelationId: Guid.NewGuid().ToString(),
                xSender: "",
                payload: data
            );


            // Act
            var contextAddedToCapabilityDomainEvent = new ContextAddedToCapabilityDomainEvent(generalDomainEvent);


            // Assert
            Assert.Equal(CapabilityId, contextAddedToCapabilityDomainEvent.Payload.CapabilityId);
            Assert.Equal(ContextId, contextAddedToCapabilityDomainEvent.Payload.ContextId);
            Assert.Equal(ContextName, contextAddedToCapabilityDomainEvent.Payload.ContextName);
            Assert.Equal(CapabilityName, contextAddedToCapabilityDomainEvent.Payload.CapabilityName);
            Assert.Equal(CapabilityRootId, contextAddedToCapabilityDomainEvent.Payload.CapabilityRootId);
        }
    }
}