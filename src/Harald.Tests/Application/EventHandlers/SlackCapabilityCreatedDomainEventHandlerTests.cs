using System;
using System.Threading.Tasks;
using Harald.Tests.TestDoubles;
using Harald.WebApi.Application.EventHandlers;
using Harald.WebApi.Domain.Events;
using Harald.WebApi.Infrastructure.Services;
using Xunit;

namespace Harald.Tests.Application.EventHandlers
{
    public class SlackCapabilityCreatedDomainEventHandlerTests
    {
        // WHEN Capability created
        // THEN channel SHOULD be created 
        // THEN user group SHOULD be created
        // THEN notify channel ABOUT handle
        // THEN pin message ABOUT handle
        [Fact]
        public async Task Handle_Will_CreateChannel_AND_EnsureUserGroupExists_AND_SendNotificationToChannel()
        {
            // Arrange
            var slackFacadeSpy = new SlackFacadeSpy();
            var stubCapabilityRepository = new StubCapabilityRepository();

            var slackService = new SlackService(slackFacadeSpy, null);
            var slackServiceSpy = new SlackServiceSpy(slackService);
            var nullLogger = new  Microsoft.Extensions.Logging.Abstractions.NullLogger<SlackCapabilityCreatedDomainEventHandler>();
            
            var handler = new SlackCapabilityCreatedDomainEventHandler(
                logger: nullLogger,
                slackFacadeSpy,
                stubCapabilityRepository,
                slackServiceSpy
            );

            
            var capabilityCreatedDomainEvent = CapabilityCreatedDomainEvent.Create(
                Guid.NewGuid().ToString(),
                "aFineCapability"
            );

            
            // Act
            await handler.HandleAsync(capabilityCreatedDomainEvent);
            
            // Assert
            Assert.NotNull(slackFacadeSpy.CreatedChannelName);
            Assert.True(slackServiceSpy.EnsureUserGroupExistsHasBeenCalled);
            Assert.NotEmpty(slackFacadeSpy.ChannelsMessages);
            Assert.NotEmpty(slackFacadeSpy.ChannelsPinnedMessageTimeStamps);
        }
    }
}