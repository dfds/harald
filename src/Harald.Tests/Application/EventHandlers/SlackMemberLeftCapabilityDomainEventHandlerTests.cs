using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harald.Tests.TestDoubles;
using Harald.WebApi.Application.EventHandlers;
using Harald.WebApi.Domain;
using Harald.WebApi.Domain.Events;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Harald.Tests.Application.EventHandlers
{
    public class SlackMemberLeftCapabilityDomainEventHandlerTests
    {
        // WHEN member left capability
        // THEN user REMOVED from slack channel
        // THEN user REMOVED from user group
        // THEN notify user
        [Fact]
        public async Task Handle_WILL_remove_user_from_channel_AND_user_group_AND_notify_user()
        {
            // Arrange
            var capability = Capability.Create(
                Guid.NewGuid().ToString(),
                "",
                "slackChannelId",
                "theUserGroup"
            );
            var userEmail = "torpen@dfds.com";
            await capability.AddMember(userEmail);
            var nullLogger = new NullLogger<SlackMemberLeftCapabilityDomainEventHandler>();
            var slackFacadeSpy = new SlackFacadeSpy();
            var stubCapabilityRepository = new StubCapabilityRepository();
            await stubCapabilityRepository.Add(capability);
            var slackMemberLeftCapabilityDomainEventHandler = new SlackMemberLeftCapabilityDomainEventHandler(
                nullLogger,
                slackFacadeSpy,
                stubCapabilityRepository
            );


            var memberLeftCapabilityDomainEvent = MemberLeftCapabilityDomainEvent.Create(capability.Id, userEmail);


            // Act

            await slackMemberLeftCapabilityDomainEventHandler.HandleAsync(memberLeftCapabilityDomainEvent);

            // Assert 
            Assert.Equal(userEmail, slackFacadeSpy.RemovedFromChannel[capability.SlackChannelId.ToString()].First());
            Assert.Equal(userEmail, slackFacadeSpy.RemovedFromUsergroup[capability.SlackUserGroupId.ToString()].First());
            Assert.NotEmpty(slackFacadeSpy.UsersToNotifications[userEmail]);
        }
    }
}