using System;
using System.Collections.Generic;
using Harald.Tests.Builders;
using Harald.Tests.TestDoubles;
using Harald.WebApi.Application.EventHandlers;
using Harald.WebApi.Domain.Events;
using Xunit;

namespace Harald.Tests.Application.EventHandlers
{
    public class SlackAWSContextAccountCreatedEventHandlerTests
    {

        [Fact]
        public async void can_handle_domain_event()
        {
            var slackStub = new SlackFacadeStub(false);
            var capabilityRepositoryStub = new StubCapabilityRepository(new List<Guid>());
            var sut = new SlackAwsContextAccountCreatedEventHandler(slackStub, capabilityRepositoryStub);
            var eventData = DomainEventBuilder.BuildAWSContextAccountCreatedEventData();
            var @event = new AWSContextAccountCreatedDomainEvent(eventData);

            await sut.HandleAsync(@event);

            Assert.True(slackStub.SendNotificationToChannelCalled);
        }
    }
}