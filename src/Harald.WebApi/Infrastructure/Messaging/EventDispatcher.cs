using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Harald.WebApi.Domain.Events;
using Harald.WebApi.EventHandlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Harald.WebApi.Infrastructure.Messaging
{
    public class EventDispatcher : IEventDispatcher
    {
        private readonly ILogger<EventDispatcher> _logger;
        private readonly DomainEventRegistry _eventRegistry;
        private readonly EventHandlerFactory _eventHandlerFactory;
        
        public EventDispatcher(
            ILogger<EventDispatcher> logger,
            DomainEventRegistry eventRegistry,
            EventHandlerFactory eventHandlerFactory)
        {
            _logger = logger;
            _eventRegistry = eventRegistry;
            _eventHandlerFactory = eventHandlerFactory;
        }

        public async Task Send(string generalDomainEventJson, IServiceScope serviceScope)
        {
            GeneralDomainEvent generalDomainEventObj = new GeneralDomainEvent("", "", "", "", "");
            try
            {
                generalDomainEventObj = JsonConvert.DeserializeObject<GeneralDomainEvent>(generalDomainEventJson);
            }
            catch (JsonReaderException ex)
            {
                throw new EventMessageIncomprehensible("Received a message that could not be deserialized from expected JSON structure.");
            }
            await SendAsync(generalDomainEventObj, serviceScope);
        }

        public async Task SendAsync(GeneralDomainEvent generalDomainEvent, IServiceScope serviceScope)
        {
            if (generalDomainEvent == null)
            {
                throw new EventMessageIncomprehensible("Received a blank message");
            }
            
            var eventType = _eventRegistry.GetInstanceTypeFor(generalDomainEvent.EventName);
            dynamic domainEvent = Activator.CreateInstance(eventType, generalDomainEvent);
            dynamic handlersList = _eventHandlerFactory.GetEventHandlersFor(domainEvent, serviceScope);
            
            foreach (var handler in handlersList)
            {
                await handler.HandleAsync(domainEvent);
            }
        }
    }
}