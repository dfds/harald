using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Harald.Tests.Builders;
using Harald.Tests.TestDoubles;
using Harald.WebApi.Domain;
using Harald.Infrastructure.Slack;
using Harald.WebApi.Infrastructure.Serialization;
using Xunit;

namespace Harald.Tests.Controllers
{
    public class TestMessagesRoute
    {
        [Fact]
        public async Task Send_message_returns_expected_status_code()
        {
            var serializer = new JsonSerializer();
            var capabilityId = Guid.NewGuid().ToString();

            using (var builder = new HttpClientBuilder())
            {
                var client = builder
                    .WithService<ICapabilityRepository>(new StubCapabilityRepository(new List<string> { capabilityId }))
                    .WithService<ISlackFacade>(new SlackFacadeStub(simulateFailOnSendMessage: false))
                    .Build();

                var message = "Route test";

                var payload = serializer.GetPayload(new { CapabilityId = capabilityId, Message = message });
                var response = await client.PostAsync("api/v1/messages", payload);

                Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            }
        }

        [Fact]
        public async Task Send_message_given_missing_capabilityId_returns_expected_status_code()
        {
            var serializer = new JsonSerializer();
            using (var builder = new HttpClientBuilder())
            {
                var client = builder
                    .WithService<ICapabilityRepository>(new StubCapabilityRepository(new List<string>{}))
                    .WithService<ISlackFacade>(new SlackFacadeStub(simulateFailOnSendMessage: false))
                    .Build();

                var message = "Route test";

                var payload = serializer.GetPayload(new { Message = message });
                var response = await client.PostAsync("api/v1/messages", payload);

                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [Fact]
        public async Task Send_message_given_non_existing_capabilityId_returns_expected_status_code()
        {
            var serializer = new JsonSerializer();
            var capabilityId = Guid.NewGuid().ToString();

            using (var builder = new HttpClientBuilder())
            {
                var client = builder
                    .WithService<ICapabilityRepository>(new StubCapabilityRepository(new List<string> { capabilityId }))
                    .WithService<ISlackFacade>(new SlackFacadeStub(simulateFailOnSendMessage: false))
                    .Build();

                var nonExistingCapabilityId = Guid.NewGuid();
                var message = "Route test";

                var payload = serializer.GetPayload(new { CapabilityId = nonExistingCapabilityId, Message = message });
                var response = await client.PostAsync("api/v1/messages", payload);

                Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            }
        }

        [Fact]
        public async Task Send_message_given_failing_slack_integration_returns_expected_status_code()
        {
            var serializer = new JsonSerializer();
            var capabilityId = Guid.NewGuid().ToString();

            using (var builder = new HttpClientBuilder())
            {
                var client = builder
                    .WithService<ICapabilityRepository>(new StubCapabilityRepository(new List<string> { capabilityId }))
                    .WithService<ISlackFacade>(new SlackFacadeStub(simulateFailOnSendMessage: true))
                    .Build();

                var message = "Route test";

                var payload = serializer.GetPayload(new { CapabilityId = capabilityId, Message = message });
                var response = await client.PostAsync("api/v1/messages", payload);

                Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            }
        }
    }
}
