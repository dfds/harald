using System;
using System.Threading.Tasks;
using Harald.Infrastructure.Slack;
using Harald.WebApi.Domain;
using Harald.WebApi.Domain.Events;

namespace Harald.WebApi.Application.EventHandlers
{
    
    public class CapabilityDeletedEventNotifyDedHandler : IEventHandler<CapabilityDeletedDomainEvent>
    {
        private readonly ISlackFacade _slackFacade;

        public CapabilityDeletedEventNotifyDedHandler(ISlackFacade slackFacade)
        {
            _slackFacade = slackFacade;
        }

        public async Task HandleAsync(CapabilityDeletedDomainEvent domainEvent)
        {          
            var messageForDed = 
            $":x: Capability with Id: '{domainEvent.Payload.CapabilityId}' & name : '{domainEvent.Payload.CapabilityName}' have been deleted" + Environment.NewLine +
             "Please Do the needfull";
            
            await _slackFacade.SendNotificationToChannel(
                _slackFacade.GetDefaultNotificationChannelId(), 
                messageForDed
            );
        }
    }
}