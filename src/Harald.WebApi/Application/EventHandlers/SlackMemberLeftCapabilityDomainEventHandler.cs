using System.Threading.Tasks;
using Harald.WebApi.Domain;
using Harald.WebApi.Domain.Events;
using Harald.Infrastructure.Slack;
using Microsoft.Extensions.Logging;
using Harald.Infrastructure.Slack.Exceptions;

namespace Harald.WebApi.Application.EventHandlers
{
    public class SlackMemberLeftCapabilityDomainEventHandler : IEventHandler<MemberLeftCapabilityDomainEvent>
    {
        private readonly ILogger<SlackMemberLeftCapabilityDomainEventHandler> _logger;
        private readonly ISlackFacade _slackFacade;
        private readonly ICapabilityRepository _capabilityRepository;

        public SlackMemberLeftCapabilityDomainEventHandler(
            ILogger<SlackMemberLeftCapabilityDomainEventHandler> logger,
            ISlackFacade slackFacade,
            ICapabilityRepository capabilityRepository)
        {
            _logger = logger;
            _slackFacade = slackFacade;
            _capabilityRepository = capabilityRepository;
        }

        public async Task HandleAsync(MemberLeftCapabilityDomainEvent domainEvent)
        {
            var capabilities = await _capabilityRepository.GetById(domainEvent.Payload.CapabilityId);

            foreach (var capability in capabilities)
            {
                if (capability == null)
                {
                    _logger.LogError(
                        $"Couldn't get capability with ID {domainEvent.Payload.CapabilityId}. Can't remove member {domainEvent.Payload.MemberEmail} from Slack.");
                    return;
                }

                try
                {
                    // Remove user from Slack channel:
                    await _slackFacade.RemoveFromChannel(
                        email: domainEvent.Payload.MemberEmail,
                        channelIdentifier: capability.SlackChannelId.ToString());
                }
                catch (SlackFacadeException ex)
                {
                    _logger.LogError($"Issue with Slack API during RemoveFromChannel: {ex} : {ex.Message}");
                }

                try
                {
                    // Remove user from Slack user group:
                    await _slackFacade.RemoveUserGroupUser(
                        email: domainEvent.Payload.MemberEmail,
                        userGroupId: capability.SlackUserGroupId);
                }
                catch (SlackFacadeException ex)
                {
                    _logger.LogError($"Issue with Slack API during RemoveUserGroupUser: {ex} : {ex.Message}");
                }

                // Notify user that it has been removed.
                await _slackFacade.SendNotificationToUser(
                    email: domainEvent.Payload.MemberEmail,
                    message:
                    $"Thank you for your contributions to capability {capability.Name}.\nYou have been removed from corresponding Slack channel and user group.");
            }
        }
    }
}