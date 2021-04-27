using Harald.Infrastructure.Slack;
using Harald.Infrastructure.Slack.Exceptions;
using Harald.WebApi.Domain;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace Harald.IntegrationTests.Facades.Slack
{
    public class SlackFacadeTest
    {
        private TestConfiguration _configuration;
        private HttpClient _httpClient;
        private IOptions<SlackOptions> _slackOptions;

        public SlackFacadeTest()
        {
            _configuration = TestHelper.GetApplicationConfiguration(Environment.CurrentDirectory);
            _slackOptions = Options.Create(new SlackOptions
            {
                SLACK_API_BOT_USER_ID = _configuration.SLACK_API_BOT_USER_ID
            });
            _httpClient = GetHttpClient();
        }

        [Fact]
        public async Task CreateChannel_Given_valid_input_Should_create_channel()
        {
            // Arrange
            var sut = new SlackFacade(_httpClient, _slackOptions, null);
            var channelName = ChannelName.Create(Guid.NewGuid().ToString());

            // Act
            var createChannelResponse = await sut.CreateChannel(channelName.ToString());

            // Assert
            Assert.True(createChannelResponse.Ok);
            Assert.Equal(channelName, createChannelResponse.Channel.Name);
            Assert.NotEmpty(createChannelResponse.Channel.Id.ToString());

            //Clean up integration test resources.
            await sut.ArchiveChannel(createChannelResponse.Channel.Id);
        }

        [Fact]
        public async Task InviteToChannel_Given_valid_input_Should_invite_to_channel()
        {
            // Arrange
            var sut = new SlackFacade(_httpClient, _slackOptions, null, new MemoryCache(new MemoryCacheOptions()));
            var channelName = ChannelName.Create(Guid.NewGuid().ToString());
            var userEmail = _configuration.SLACK_TESTING_USER_EMAIL;

            // Act
            var createChannelResponse = await sut.CreateChannel(channelName.ToString());
            await sut.InviteToChannel(userEmail, createChannelResponse.Channel.Id.ToString());

            // Assert
            Assert.True(createChannelResponse.Ok);

            //Clean up integration test resources.
            await sut.ArchiveChannel(createChannelResponse.Channel.Id);
        }

        [Fact]
        public async Task CreateUserGroup_And_Add_User_Given_valid_input_Should_create_group_with_user()
        {
            // Arrange
            var sut = new SlackFacade(_httpClient, _slackOptions, null, new MemoryCache(new MemoryCacheOptions()));
            var groupName = Guid.NewGuid().ToString();
            var handle = UserGroupHandle.Create(Guid.NewGuid().ToString());
            const string description = "Group created through integration test.";
            var userEmail = _configuration.SLACK_TESTING_USER_EMAIL;

            // Act
            var createUserGroupResponse = await sut.CreateUserGroup(name: groupName, handle: handle, description: description);

            if (!createUserGroupResponse.Ok)
            {
                throw new SlackFacadeException($"API error: {createUserGroupResponse.Error}");
            }
            
            await sut.AddUserGroupUser(userGroupId: createUserGroupResponse.UserGroup.Id, email: userEmail);

            // Assert
            Assert.True(createUserGroupResponse.Ok);
            Assert.Equal(groupName, createUserGroupResponse.UserGroup.Name);

            //Disable user group
            await sut.DisableUserGroup(createUserGroupResponse.UserGroup.Id);
        }

        [Fact]
        public async Task SendNotificationToChannel_Given_valid_input_Should_send_notfication_to_channel()
        {
            // Arrange
            var sut = new SlackFacade(_httpClient, _slackOptions, null, new MemoryCache(new MemoryCacheOptions()));
            var channelName = ChannelName.Create(Guid.NewGuid().ToString());
            const string message = "Integration test message.";

            // Act
            var createChannelResponse = await sut.CreateChannel(channelName.ToString());
            var sendNotificationToChannelResponse = await sut.SendNotificationToChannel(createChannelResponse.Channel.Id, message: message);

            // Assert
            Assert.True(sendNotificationToChannelResponse.Ok);
            Assert.NotEmpty(sendNotificationToChannelResponse.TimeStamp);

            //Clean up integration test resources.
            await sut.ArchiveChannel(createChannelResponse.Channel.Id);
        }

        [Fact]
        public async Task PinMessageToChannel_Given_valid_input_Should_send_notfication_to_channel()
        {
            // Arrange
            var sut = new SlackFacade(_httpClient, _slackOptions, null, new MemoryCache(new MemoryCacheOptions()));
            var channelName = ChannelName.Create(Guid.NewGuid().ToString());
            const string message = "Integration test message.";

            // Act
            var createChannelResponse = await sut.CreateChannel(channelName.ToString());
            var sendNotificationToChannelResponse = await sut.SendNotificationToChannel(createChannelResponse.Channel.Id, message: message);
            var pinMessageToChannelResponse = await sut.PinMessageToChannel(createChannelResponse.Channel.Id, messageTimeStamp: sendNotificationToChannelResponse.TimeStamp);

            // Assert
            Assert.True(sendNotificationToChannelResponse.Ok);
            Assert.NotEmpty(sendNotificationToChannelResponse.TimeStamp);
            Assert.True(pinMessageToChannelResponse.Ok);

            //Clean up integration test resources.
            await sut.ArchiveChannel(createChannelResponse.Channel.Id);
        }

        [Fact]
        public async Task Get_channels_should_return_list_of_channel_objects()
        {
            // Arrange
            var sut = new SlackFacade(_httpClient, _slackOptions, null, new MemoryCache(new MemoryCacheOptions()));

            //Act
            var channels = await sut.GetChannels();

            // Assert
            Assert.True(channels != null);
        }
        
        private HttpClient GetHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://slack.com");
            var authToken = _configuration.SLACK_API_AUTH_TOKEN;
            
            httpClient.DefaultRequestHeaders.Add(HttpRequestHeader.Authorization.ToString(), $"Bearer {authToken}");

            return httpClient;
        }
    }
}
