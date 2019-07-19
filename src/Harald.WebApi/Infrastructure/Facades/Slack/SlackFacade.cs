using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Harald.WebApi.Infrastructure.Serialization;

namespace Harald.WebApi.Infrastructure.Facades.Slack
{
    public class SlackFacade : ISlackFacade
    {
        private readonly HttpClient _client;
        private readonly JsonSerializer _serializer;
        private readonly SlackHelper _slackHelper;

        public SlackFacade(HttpClient client, JsonSerializer serializer, SlackHelper slackHelper)
        {
            _client = client;
            _serializer = serializer;
            _slackHelper = slackHelper;
        }

        public async Task<CreateChannelResponse> CreateChannel(string channelName)
        {
            var validChannelName = _slackHelper.FixChannelNameForSlack(channelName);
            var payload = _serializer.GetPayload(new { Name = validChannelName, Validate = true });
            var response = await _client.PostAsync("/api/channels.create", payload);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var createChannelResponse = _serializer.Deserialize<CreateChannelResponse>(content);

            return createChannelResponse;
        }

        public async Task<SendNotificationResponse> SendNotificationToChannel(string channel, string message)
        {
            var payload = _serializer.GetPayload(new { Channel = channel, Text = message });

            var response = await _client.PostAsync("/api/chat.postMessage", payload);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var sendNotificationResponse = _serializer.Deserialize<SendNotificationResponse>(content);

            return sendNotificationResponse;
        }

        public async Task<SendNotificationResponse> SendDelayedNotificationToChannel(string channel, string message, long delayTimeInEpoch)
        {
            var payload = _serializer.GetPayload(new { Channel = channel, Text = message, post_at = delayTimeInEpoch });
            var response = await _client.PostAsync("/api/chat.scheduleMessage", payload);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var sendNotificationResponse = _serializer.Deserialize<SendNotificationResponse>(content);
            return sendNotificationResponse;
        }

        public async Task<SendNotificationResponse> SendNotificationToUser(string email, string message)
        {
            var userId = await GetUserId(email);
            var payload = _serializer.GetPayload(new { Channel = userId, Text = message, As_user = true });

            var response = await _client.PostAsync("/api/chat.postMessage", payload);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var sendNotificationResponse = _serializer.Deserialize<SendNotificationResponse>(content);

            return sendNotificationResponse;
        }

        public async Task<GeneralResponse> PinMessageToChannel(string channel, string messageTimeStamp)
        {
            var payload = _serializer.GetPayload(new { Channel = channel, Timestamp = messageTimeStamp });

            var response = await _client.PostAsync("/api/pins.add", payload);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var generalResponse = _serializer.Deserialize<GeneralResponse>(content);

            return generalResponse;
        }

        public async Task InviteToChannel(string email, string channelId)
        {
            var userId = await GetUserId(email);
            var payload = _serializer.GetPayload(new { Channel = channelId, user = userId });

            var response = await _client.PostAsync("/api/channels.invite", payload);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var generalResponse = _serializer.Deserialize<GeneralResponse>(content);
            if (!generalResponse.Ok)
            {
                throw new SlackFacadeException($"API error: {generalResponse.Error}");
            }
        }

        public async Task RemoveFromChannel(string email, string channelId)
        {
            var userId = await GetUserId(email);
            var payload = _serializer.GetPayload(new { Channel = channelId, user = userId });

            var response = await _client.PostAsync("/api/channels.kick", payload);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var generalResponse = _serializer.Deserialize<GeneralResponse>(content);
            if (!generalResponse.Ok)
            {
                throw new SlackFacadeException($"API error: {generalResponse.Error}");
            }
        }

        public async Task<CreateUserGroupResponse> CreateUserGroup(string name, string handle, string description)
        {
            var validHandle = _slackHelper.FixHandleNameForSlack(handle);
            var payload = _serializer.GetPayload(new { Name = name, Handle = validHandle, Description = description });
            var response = await _client.PostAsync("/api/usergroups.create", payload);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var createUserGroupResponse = _serializer.Deserialize<CreateUserGroupResponse>(content);
            
            if (!createUserGroupResponse.Ok)
            {
                throw new SlackFacadeException($"API error: {createUserGroupResponse.Error}");
            }

            return createUserGroupResponse;
        }

        public async Task AddUserGroupUser(string userGroupId, string email)
        {
            var users = await GetUserGroupUsers(userGroupId);
            var userId = await GetUserId(email);
            users.Add(userId);

            await UpdateUserGroupUsers(userGroupId, users);
        }

        public async Task RemoveUserGroupUser(string userGroupId, string email)
        {
            var users = await GetUserGroupUsers(userGroupId);
            var userId = await GetUserId(email);
            users.Remove(userId);

            await UpdateUserGroupUsers(userGroupId, users);
        }

        private async Task<string> GetUserId(string email)
        {
            var response = await _client.GetAsync($"/api/users.lookupByEmail?email={email}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            string userId = _serializer.GetTokenValue<string>(content, "['user']['id']");

            return userId;
        }

        private async Task<List<string>> GetUserGroupUsers(string userGroupId)
        {
            var response = await
             _client.GetAsync($"/api/usergroups.users.list?usergroup={userGroupId}&include_disabled=false");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var users = _serializer.GetTokenValue<List<string>>(content, "['users']");
            
            var generalResponse = _serializer.Deserialize<GeneralResponse>(content);
            if (!generalResponse.Ok)
            {
                throw new SlackFacadeException($"API error: {generalResponse.Error}");
            }

            return users;
        }

        private async Task UpdateUserGroupUsers(string userGroupId, List<string> users)
        {
            var usersList = string.Join(",", users);
            var payload = _serializer.GetPayload(new { Usergroup = userGroupId, users = usersList });

            var response = await _client.PostAsync("/api/usergroups.users.update", payload);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var generalResponse = _serializer.Deserialize<GeneralResponse>(content);
            if (!generalResponse.Ok)
            {
                throw new SlackFacadeException($"API error: {generalResponse.Error}");
            }
        }

        public class SlackFacadeException : Exception
        {
            public SlackFacadeException() : base() {}
            public SlackFacadeException(string message) : base(message) {}
            public SlackFacadeException(string message, Exception inner) : base(message, inner) {}
        }
    }
}