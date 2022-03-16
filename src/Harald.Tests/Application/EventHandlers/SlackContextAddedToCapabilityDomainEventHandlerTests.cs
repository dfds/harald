using System;
using System.Threading.Tasks;
using Harald.Infrastructure.Slack.Model;
using Harald.Tests.TestDoubles;
using Harald.WebApi.Application.EventHandlers;
using Harald.WebApi.Domain;
using Harald.WebApi.Domain.Events;
using Harald.WebApi.Infrastructure.Messaging;
using Xunit;

namespace Harald.Tests.Application.EventHandlers
{
    public class SlackContextAddedToCapabilityDomainEventHandlerTests
    {
        // WHEN Context added to capability
        // THEN ded SHOULD be asked to run command
        // THEN channel SHOULD be informed that we are working on the setup
        [Fact]
        public async Task Handle_will_SendNotificationToChannel_Ded_AND_SendNotificationToChannel_CapabilityId()
        {
            // Arrange
            var capability = Capability.Create(
                Guid.NewGuid(),
                "",
                "slackChannelId",
                ""
            );
            var slackFacadeSpy = new SlackFacadeSpy();
            var stubCapabilityRepository = new StubCapabilityRepository();
            await stubCapabilityRepository.Add(capability);

            var slackContextAddedToCapabilityDomainEventHandler = new SlackContextAddedToCapabilityDomainEventHandler(
                stubCapabilityRepository,
                slackFacadeSpy,
                new ExternalEventMetaDataStore()
            );

            var contextAddedToCapabilityDomainEvent = ContextAddedToCapabilityDomainEvent.Create(
                capability.Id,
                "",
                "",
                Guid.NewGuid(),
                ""
            );
            
            
            // Act
            await slackContextAddedToCapabilityDomainEventHandler
                .HandleAsync(contextAddedToCapabilityDomainEvent);
            
            // Assert
            Assert.NotEmpty(slackFacadeSpy.GetDefaultNotificationChannelId());
            Assert.NotEmpty(slackFacadeSpy.ChannelsMessages[capability.SlackChannelId.ToString()]);
            
        }
    }
}