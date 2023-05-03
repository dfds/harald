using System;
using System.Linq;
using System.Threading.Tasks;
using Harald.Infrastructure.Slack.Dto;
using Harald.Tests.TestDoubles;
using Harald.WebApi.Domain;
using Harald.WebApi.Features.Connections.Domain.Model;
using Harald.WebApi.Features.Connections.Domain.Queries;
using Harald.WebApi.Features.Connections.Infrastructure.Persistence;
using Xunit;

namespace Harald.Tests.features.Connections.Infrastructure.Persistence
{
    public class FindConnectionsByClientTypeClientIdChannelTypeChannelIdHandlerTests
    {
        // GIVEN query with ChannelType not equal to ChannelTypeSlack
        // EXPECT empty result
        [Fact]
        public async Task GIVEN_query_with_ChannelType_not_equal_to_ChannelTypeSlack_EXPECT_empty_result()
        {
            // Arrange
            var sut = new FindConnectionsByClientTypeClientIdChannelTypeChannelIdHandler(null, null);
            var query = new FindConnectionsByClientTypeClientIdChannelTypeChannelId(
                null,
                null,
                new ChannelTypeTest(),
                null
            );

            // Act 
            var results = await sut.HandleAsync(query);


            // Assert
            Assert.Empty(results);
        }

        // GIVEN query with SenderType not equal to SenderTypeCapability
        // EXPECT empty result
        [Fact]
        public async Task GIVEN_query_with_SenderType_not_equal_to_SenderTypeCapability_EXPECT_empty_result()
        {
            // Arrange
            var sut = new FindConnectionsByClientTypeClientIdChannelTypeChannelIdHandler(null, null);
            var query = new FindConnectionsByClientTypeClientIdChannelTypeChannelId(
                new ClientTypeTest(),
                null,
                null,
                null
            );

            // Act 
            var results = await sut.HandleAsync(query);


            // Assert
            Assert.Empty(results);
        }

        // GIVEN query with ChannelId set
        // EXPECT only capabilities with given id
        [Fact]
        public async Task GIVEN_query_with_ChannelId_set_EXPECT_only_capabilities_with_given_id()
        {
            // Arrange
            var idOfWantedChannels = "theChannelWeWant";
            var capabilityRepository = new StubCapabilityRepository();
            await capabilityRepository.Add(Capability.Create(
                Guid.NewGuid().ToString(),
                "foo",
                idOfWantedChannels,
                "slackUserGroupId"
            ));
            await capabilityRepository.Add(Capability.Create(
                Guid.NewGuid().ToString(),
                "bar",
                idOfWantedChannels,
                "slackUserGroupId"
            ));
            await capabilityRepository.Add(Capability.Create(
                Guid.NewGuid().ToString(),
                "sheep",
                "NotTheChannelWeWant",
                "slackUserGroupId"
            ));
            
            var slackFacadeSpy = new SlackFacadeSpy();

            var sut = new FindConnectionsByClientTypeClientIdChannelTypeChannelIdHandler(capabilityRepository,slackFacadeSpy);

            var channelIdOfWantedChannel = new ChannelId(idOfWantedChannels);
            var query = new FindConnectionsByClientTypeClientIdChannelTypeChannelId(
                new ClientTypeCapability(),
                null,
                new ChannelTypeSlack(),
                channelIdOfWantedChannel
            );

            // Act
            var results = await sut.HandleAsync(query);

            // Assert
            Assert.All(
                results,
                connection =>
                    Assert.Equal(channelIdOfWantedChannel, connection.ChannelId)
            );
        }

        // GIVEN query with ClientId set
        // EXPECT only capabilities with given id
        [Fact]
        public async Task GIVEN_query_with_ClientId_set_EXPECT_only_capabilities_with_given_id()
        {
            // Arrange
            var idOfWantedSender = Guid.NewGuid().ToString();
            var capabilityRepository = new StubCapabilityRepository();
            await capabilityRepository.Add(Capability.Create(
                idOfWantedSender,
                "foo",
                "slackChannelId",
                "slackUserGroupId"
            ));
            await capabilityRepository.Add(Capability.Create(
                idOfWantedSender,
                "bar",
                "slackChannelId",
                "slackUserGroupId"
            ));
            await capabilityRepository.Add(Capability.Create(
                Guid.NewGuid().ToString(),
                "sheep",
                "slackChannelId",
                "slackUserGroupId"
            ));
            var slackFacadeSpy = new SlackFacadeSpy();

            var sut = new FindConnectionsByClientTypeClientIdChannelTypeChannelIdHandler(capabilityRepository,slackFacadeSpy);

            var clientIdOfWantedSender = new ClientId(idOfWantedSender.ToString());
            var query = new FindConnectionsByClientTypeClientIdChannelTypeChannelId(
                new ClientTypeCapability(),
                clientIdOfWantedSender,
                new ChannelTypeSlack(),
                null
            );


            // Act
            var results = await sut.HandleAsync(query);

            // Assert
            Assert.All(
                results,
                connection =>
                    Assert.Equal(clientIdOfWantedSender, connection.ClientId)
            );
        }

        // GIVEN empty query
        // EXPECT connections for all capabilities
        [Fact]
        public async Task GIVEN_empty_query_EXPECT_connections_for_all_capabilities()
        {
            // Arrange
            var capabilityRepository = new StubCapabilityRepository();
            await capabilityRepository.Add(Capability.Create(
                Guid.NewGuid().ToString(),
                "foo",
                "slackChannelId",
                "slackUserGroupId"
            ));
            await capabilityRepository.Add(Capability.Create(
                Guid.NewGuid().ToString(),
                "bar",
                "slackChannelId",
                "slackUserGroupId"
            ));

            var slackFacadeSpy = new SlackFacadeSpy();
            slackFacadeSpy.Conversations.AddRange(new []{new ChannelDto{Id = "slackChannelId", Name = "slackChannelName"}});
            var sut = new FindConnectionsByClientTypeClientIdChannelTypeChannelIdHandler(capabilityRepository, slackFacadeSpy);

            var query = new FindConnectionsByClientTypeClientIdChannelTypeChannelId(
                null,
                null,
                null,
                null
            );

            // Act
            var results = await sut.HandleAsync(query);
            Assert.All(
                capabilityRepository.GetAll().Result,
                capability =>
                    results.Single(r =>
                        r.ClientId.ToString() == capability.Id.ToString()
                    )
            );
        }
    }
}