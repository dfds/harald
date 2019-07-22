using System;
using System.Threading.Tasks;
using Harald.WebApi.Domain.Events;
using Harald.WebApi.EventHandlers;
using Harald.WebApi.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Sdk;

namespace Harald.Tests.Messaging
{
    public class EventDispatcherTests
    {
        private IServiceProvider _serviceProvider;
        
        [Fact]
        public async void ThrowEventMessageIncomprehensibleExceptionOnBlankMessage()
        {
            var serviceProvider = GetPreconfiguredServiceProvider();
            const string message = "";
            var ex = await Assert.ThrowsAsync<EventMessageIncomprehensible>(async () =>
            {
                await serviceProvider.GetRequiredService<IEventDispatcher>().Send(message, null);
            });
            
            Assert.Equal("Received a blank message", ex.Message);
        }
        
        [Fact]
        public async void ThrowEventMessageIncomprehensibleExceptionOnInvalidJson()
        {
            var serviceProvider = GetPreconfiguredServiceProvider();
            const string message = "{::;;}";
            var ex = await Assert.ThrowsAsync<EventMessageIncomprehensible>(async () =>
            {
                await serviceProvider.GetRequiredService<IEventDispatcher>().Send(message, null);
            });
            
            Assert.Contains("Received a message that could not be deserialized from expected JSON structure. Original exception:", ex.Message);
        }

        [Theory]
        [InlineData("{}")]
        [InlineData("{\"version\":\"1\",\"eventName\":\"k8s_namespace_created_and_aws_arn_connected\",\"x-correlationId\":\"00000000-0000-0000-0000-000000000000\",\"x-sender\":\"K8sJanitor.WebApi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\",\"payload\":{\"namespaceName\":\"hellopelle\",\"contextId\":\"00000000-0000-0000-0000-000000000000\"}}")]
        [InlineData("{\"eventDataXaxa\": \"dataaaa\"}")]
        public async void ThrowEventTypeNotFoundExceptionOnUnknownOrUnregisteredEvent(string evtMsg)
        {
            var serviceProvider = GetPreconfiguredServiceProvider();
            var ex = await Assert.ThrowsAsync<EventTypeNotFoundException>(async () =>
            {
                await serviceProvider.GetRequiredService<IEventDispatcher>().Send(evtMsg, null);
            });

            Assert.Contains("Error! Could not determine \"event instance type\" due to no registration was found for type", ex.Message);
        }

        [Theory]
        [InlineData("{\"version\":\"1\",\"eventName\":\"member_joined_capability\",\"x-correlationId\":\"00000000-0000-0000-0000-000000000000\",\"x-sender\":\"K8sJanitor.WebApi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\",\"payload\":{\"namespaceName\":\"hellopelle\",\"contextId\":\"00000000-0000-0000-0000-000000000000\"}}")]
        public async void InterpretAndSendEventToHandler(string evtMsg)
        {
            var serviceProvider = GetPreconfiguredServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                var eventDispatcher = scope.ServiceProvider.GetRequiredService<IEventDispatcher>();
                await eventDispatcher.Send(evtMsg, scope);
            }
            //Assert.Contains("Error! Could not determine \"event instance type\" due to no registration was found for type", ex.Message);
        }

        private IServiceProvider GetPreconfiguredServiceProvider()
        {

            var loggerFactory = new LoggerFactory();
            var logger = new Logger<EventDispatcher>(loggerFactory);
            var domainEventRegistry = new DomainEventRegistry();
            const string topic = "build.capabilities";

            domainEventRegistry.Register<MemberJoinedCapabilityDomainEvent>(
                eventName: "member_joined_capability",
                topicName: topic);
            
            var eventHandlerFactory = new EventHandlerFactory();
            
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddTransient<IEventHandler<MemberJoinedCapabilityDomainEvent>, GenericEventHandler<MemberJoinedCapabilityDomainEvent>>()
                .AddTransient<EventHandlerFactory>()
                .AddSingleton(domainEventRegistry)
                .AddTransient<IEventDispatcher, EventDispatcher>()
                .BuildServiceProvider();

            return serviceProvider;
        }

    }

    public class GenericEventHandler<T> : IEventHandler<T>
    {
        public async Task HandleAsync(T domainEvent)
        {
            Console.WriteLine("GenericEventHandler called");
        }
    }
}