using System;
using System.Linq;
using System.Threading.Tasks;
using Harald.Infrastructure.Slack;
using Harald.Tests.Builders;
using Harald.Tests.TestDoubles;
using Harald.WebApi.Application.EventHandlers;
using Harald.WebApi.Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Harald.Tests.Scenarios
{
    public class CapabilityDeletedScenario
    {
        private Guid _capabilityId;
        private string _capabilityName;
        
        private IServiceProvider _serviceProvider;

        [Fact]
        public async Task CapabilityDeletedScenarioRecipe()
        {
                  Given_a_service_collection_with_a_imMemoryDb_and_SlackFacadeSpy();
            await And_a_existing_capability_slack_channel();
            await When_Capability_deleted_event_is_raised();
            Then_a_message_with_channel_info_should_be_posted_in_the_ded_channel();
        }

        private void Given_a_service_collection_with_a_imMemoryDb_and_SlackFacadeSpy()
        {
            var serviceProviderBuilder = new ServiceProviderBuilder();
            serviceProviderBuilder = serviceProviderBuilder
                .WithServicesFromStartup()
                .WithInMemoryDb()
                .RemoveType(typeof(ISlackFacade));

            var slackFacadeSpy = new SlackFacadeSpy();
            serviceProviderBuilder.Services.AddSingleton<ISlackFacade>(slackFacadeSpy);


            _serviceProvider = serviceProviderBuilder.Build();
        }

        private async Task And_a_existing_capability_slack_channel()
        {
            _capabilityId = Guid.NewGuid();
            _capabilityName = "aFineCapability";
            var capabilityCreatedDomainEvent = CapabilityCreatedDomainEvent.Create(
                _capabilityId,
                _capabilityName
            );
            var handler = _serviceProvider.GetService<IEventHandler<CapabilityCreatedDomainEvent>>();

            // Act
            await handler.HandleAsync(capabilityCreatedDomainEvent);
        }

        private async Task When_Capability_deleted_event_is_raised()
        {
            var capabilityDeletedEvent = CapabilityDeletedDomainEvent.Create(
                _capabilityId,
                _capabilityName
            );
            
            var handler = _serviceProvider.GetService<IEventHandler<CapabilityDeletedDomainEvent>>();

            // Act
            await handler.HandleAsync(capabilityDeletedEvent);
        }

        private void Then_a_message_with_channel_info_should_be_posted_in_the_ded_channel()
        {
            var slackFacadeSpy = (SlackFacadeSpy)_serviceProvider.GetService<ISlackFacade>();

           var dedMessages = slackFacadeSpy.ChannelsMessages.Where(pair => pair.Key == slackFacadeSpy.GetDefaultNotificationChannelId());
           
           Assert.Contains(dedMessages, pair =>
           {
               var channelMessages = pair.Value;
               return channelMessages.Any(m => m.Contains("deleted"));
           });
        }
    }
}