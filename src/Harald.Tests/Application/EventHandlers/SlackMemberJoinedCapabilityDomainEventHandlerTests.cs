using System;
using System.Linq;
using System.Threading.Tasks;
using Harald.Infrastructure.Slack.Model;
using Harald.Tests.TestDoubles;
using Harald.WebApi.Application.EventHandlers;
using Harald.WebApi.Domain;
using Harald.WebApi.Domain.Events;
using Harald.WebApi.Infrastructure.Services;
using Xunit;

namespace Harald.Tests.Application.EventHandlers
{
    public class SlackMemberJoinedCapabilityDomainEventHandlerTests
    {
        // WHEN Member Joined Capability
        // THEN Email invited to channelId
        // THEN Email added to UserGroup
        [Fact]
        public async Task Handle_WILL_InviteToChannel_AND_AddUserGroupUser()
        {
            // Arrange
            var capability = Capability.Create(
                id: Guid.NewGuid().ToString(),
                name: "FooCapability",
                slackChannelId: "FooChannelId",
                slackUserGroupId: "FooUserGroupId"
            );
            var slackFacadeSpy = new SlackFacadeSpy();
            var stubCapabilityRepository = new StubCapabilityRepository();
            await stubCapabilityRepository.Add(capability);
            var slackMemberJoinedCapabilityDomainEventHandler = new SlackMemberJoinedCapabilityDomainEventHandler(
                null,
                slackFacadeSpy,
                stubCapabilityRepository,
                new SlackService(slackFacadeSpy, null)
            );
            var memberEmail = "a.person.with@aemail.com";
            var memberJoinedCapabilityDomainEvent = MemberJoinedCapabilityDomainEvent.Create(capability.Id, memberEmail);

            
            // Act
            await slackMemberJoinedCapabilityDomainEventHandler.HandleAsync(memberJoinedCapabilityDomainEvent);
            
            
            // Assert
            Assert.Equal(memberEmail,slackFacadeSpy.InvitedToChannel[new SlackChannelIdentifier(capability.SlackChannelId)].Single());
            Assert.Equal(memberEmail,slackFacadeSpy.UserGroupsUsers[capability.SlackUserGroupId].Single());

        }
    }
}