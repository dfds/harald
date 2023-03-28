using Harald.Tests.Builders;
using Harald.WebApi.Domain.Events;
using Xunit;

namespace Harald.Tests.Domain
{
    public class AWSContextAccountCreatedDomainEventTests
    {
        [Fact]
        public void can_create_valid_AWSContextAccountCreatedDomainEvent_when_given_GeneralDomainEvent()
        {
            // Arrange
            var generalDomainEvent = DomainEventBuilder.BuildAWSContextAccountCreatedEventData();
            
            // Act 
            var awsContextAccountCreatedDomainEvent = new AWSContextAccountCreatedDomainEvent(generalDomainEvent);
            
            
            // Assert
            Assert.Equal(DomainEventBuilder.ContextId, awsContextAccountCreatedDomainEvent.Payload.ContextId);
            Assert.Equal(DomainEventBuilder.ContextName, awsContextAccountCreatedDomainEvent.Payload.ContextName);
            Assert.Equal(DomainEventBuilder.CapabilityName, awsContextAccountCreatedDomainEvent.Payload.CapabilityName);
            Assert.Equal(DomainEventBuilder.CapabilityId, awsContextAccountCreatedDomainEvent.Payload.CapabilityId);
            Assert.Equal(DomainEventBuilder.CapabilityRootId, awsContextAccountCreatedDomainEvent.Payload.CapabilityRootId);
            Assert.Equal(DomainEventBuilder.RoleEmail, awsContextAccountCreatedDomainEvent.Payload.RoleEmail);
            Assert.Equal(DomainEventBuilder.AccountId, awsContextAccountCreatedDomainEvent.Payload.AccountId);
        }
    }
}