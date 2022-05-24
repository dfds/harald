using System.Threading.Tasks;
using Harald.WebApi.Domain;
using Harald.WebApi.Domain.Events;
using Harald.Infrastructure.Slack;
using Microsoft.Extensions.Logging;
using Harald.Infrastructure.Slack.Exceptions;
using System.Linq;

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
            var capabilitiesJoinedByMember = await _capabilityRepository.GetByFilter(o => o.Members.Any(m => m.Email == domainEvent.Payload.MemberEmail));
            var capabilitiesBeingLeft = capabilitiesJoinedByMember.Where(o => o.Id.Equals(domainEvent.Payload.CapabilityId)).ToList();

            foreach (var capability in capabilitiesBeingLeft)
            {
                await capability.RemoveMember(domainEvent.Payload.MemberEmail);
                await _capabilityRepository.Update(capability);

                if (!capabilitiesJoinedByMember.Where(c => c.Id != capability.Id).Any(o => o.SlackChannelId == capability.SlackChannelId))
                {
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
                    try
                    {
                        await _slackFacade.SendNotificationToUser(
                            email: domainEvent.Payload.MemberEmail,
                            message:
                            $"Thank you for your contributions to capability {capability.Name}.\nYou have been removed from corresponding Slack channel and user group.");
                    }
                    catch (SlackFacadeException ex)
                    {
                        _logger.LogError($"Unable to send Slack message to user {domainEvent.Payload.MemberEmail}. This usually happens if the user has been disabled/removed from Slack.");
                    }

                }
            }
        }
    }
}