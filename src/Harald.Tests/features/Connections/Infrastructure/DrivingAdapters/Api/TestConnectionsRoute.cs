using Harald.Infrastructure.Slack;
using Harald.Tests.Builders;
using Harald.Tests.TestDoubles;
using Harald.WebApi.Domain;
using Harald.WebApi.Features.Connections.Infrastructure.DrivingAdapters.Api.Model;
using Harald.WebApi.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Xunit;

namespace Harald.Tests.Features.Connections.Infrastructure.DrivingAdapters.Api
{
    public class TestConnectionsRoute
    {
        [Fact]
        public async Task delete_connection_returns_expected_status_code()
        {
            var clientId = Guid.NewGuid().ToString();

            using (var builder = new HttpClientBuilder())
            {
                var client = builder
                    .WithService<ICapabilityRepository>(new StubCapabilityRepository(new List<string> { clientId }))
                    .WithService<ISlackFacade>(new SlackFacadeStub(simulateFailOnSendMessage: false))
                    .Build();

                var response = await client.DeleteAsync($"api/v1/connections?clientId={clientId}");

                Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            }
        }

        [Fact]
        public async Task delete_connection_given_empty_clientId_returns_expected_status_code()
        {
            const string clientType = "capability";
            const string channelId = "123FooBar";

            using (var builder = new HttpClientBuilder())
            {
                var client = builder
                    .WithService<ICapabilityRepository>(new StubCapabilityRepository(new List<string> { }))
                    .WithService<ISlackFacade>(new SlackFacadeStub(simulateFailOnSendMessage: false))
                    .Build();

                var response = await client.DeleteAsync($"api/v1/connections?channelId={channelId}&clientType={clientType}");

                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [Fact]
        public async Task add_connection_returns_expected_status_code()
        {
            var slackFacadeSpy = new SlackFacadeSpy();
            var logger = new LoggerFactory().CreateLogger<SlackService>();
            var clientId = Guid.NewGuid().ToString();
            var connection = new ConnectionDto()
            {
                ClientId = Guid.NewGuid().ToString(),
                ClientName = "MyClient",
                ClientType = "capability",
                ChannelId = Guid.NewGuid().ToString(),
                ChannelName = "MyChannel",
                ChannelType = "slack"
            };

            using (var builder = new HttpClientBuilder())
            {
                var client = builder
                    .WithService<ICapabilityRepository>(new StubCapabilityRepository(new List<string> { clientId }))
                    .WithService<ISlackFacade>(new SlackFacadeStub(simulateFailOnSendMessage: false))
                    .WithService<ISlackService>(new SlackService(slackFacadeSpy, logger))
                    .Build();

                var payload = new ObjectContent(connection.GetType(), connection, new JsonMediaTypeFormatter());
                var response = await client.PostAsync($"api/v1/connections", payload);

                Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            }
        }

        [Fact]
        public async Task add_existing_connection_returns_expected_status_code()
        {
            var slackFacadeSpy = new SlackFacadeSpy();
            var logger = new LoggerFactory().CreateLogger<SlackService>();
            var clientId = Guid.NewGuid().ToString();
            var connection = new ConnectionDto()
            {
                ClientId = Guid.NewGuid().ToString(),
                ClientName = "MyClient",
                ClientType = "capability",
                ChannelId = Guid.NewGuid().ToString(),
                ChannelName = "MyChannel",
                ChannelType = "slack"
            };

            using (var builder = new HttpClientBuilder())
            {
                var client = builder
                    .WithService<ICapabilityRepository>(new StubCapabilityRepository(new List<string> { clientId }))
                    .WithService<ISlackFacade>(new SlackFacadeStub(simulateFailOnSendMessage: false))
                    .WithService<ISlackService>(new SlackService(slackFacadeSpy, logger))
                    .Build();

                var payload = new ObjectContent(connection.GetType(), connection, new JsonMediaTypeFormatter());
                var response = await client.PostAsync($"api/v1/connections", payload);
                var response2 = await client.PostAsync($"api/v1/connections", payload);

                Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
                Assert.Equal(HttpStatusCode.Accepted, response2.StatusCode);
            }
        }
    }
}
