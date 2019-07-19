using System;
using System.Threading.Tasks;
using Harald.WebApi.Domain;
using Harald.WebApi.Domain.Events;
using Harald.WebApi.Infrastructure.Facades.Slack;
using Microsoft.Extensions.Logging;

namespace Harald.WebApi.EventHandlers
{
    public class SlackMemberJoinedCapabilityDomainEventHandler : IEventHandler<MemberJoinedCapabilityDomainEvent>
    {
        private readonly ILogger<SlackMemberJoinedCapabilityDomainEventHandler> _logger;
        private readonly ISlackFacade _slackFacade;
        private readonly ICapabilityRepository _capabilityRepository;

        public SlackMemberJoinedCapabilityDomainEventHandler(
            ILogger<SlackMemberJoinedCapabilityDomainEventHandler> logger,
            ISlackFacade slackFacade,
            ICapabilityRepository capabilityRepository)
        {
            _logger = logger;
            _slackFacade = slackFacade;
            _capabilityRepository = capabilityRepository;
        }

        public async Task HandleAsync(MemberJoinedCapabilityDomainEvent domainEvent)
        {
            var capability = await _capabilityRepository.Get(domainEvent.Payload.CapabilityId);

            if (capability == null)
            {
                _logger.LogError(
                    $"Couldn't get capability with ID {domainEvent.Payload.CapabilityId}. Can't add member {domainEvent.Payload.MemberEmail} to Slack.");
                return;    
            }

            try
            {
                // Invite user to Slack channel:
                await _slackFacade.InviteToChannel(
                    email: domainEvent.Payload.MemberEmail,
                    channelId: capability.SlackChannelId);
            }
            catch (SlackFacade.SlackFacadeException ex)
            {
                _logger.LogError($"Issue with Slack API during InviteToChannel: {ex} : {ex.Message}");
            }

            try
            {
                // Add user to Slack user group:
                await _slackFacade.AddUserGroupUser(
                    email: domainEvent.Payload.MemberEmail,
                    userGroupId: capability.SlackUserGroupId);
            }
            catch (SlackFacade.SlackFacadeException ex)
            {
                _logger.LogError($"Issue with Slack API during AddUserGroupUser: {ex} : {ex.Message}");
            }


            /*
            // Disabled for now due to redundant messages. Read commit where this line is introduced in order to find further information.
             
            // Notify user that it has been invited.
            await _slackFacade.SendNotificationToUser(
                email: domainEvent.Payload.MemberEmail, 
                message: 
                $"Thank you for joining capability {capability.Name}.\nYou have been invited to corresponding Slack channel and user group.");
            */
        }
    }
}