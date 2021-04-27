using ExtensionMethods;
using Harald.Infrastructure.Slack.Dto;
using Harald.Infrastructure.Slack.Exceptions;
using Harald.Infrastructure.Slack.Http.Request.Channel;
using Harald.Infrastructure.Slack.Http.Request.Conversation;
using Harald.Infrastructure.Slack.Http.Request.Notification;
using Harald.Infrastructure.Slack.Http.Request.User;
using Harald.Infrastructure.Slack.Http.Request.UserGroup;
using Harald.Infrastructure.Slack.Http.Response;
using Harald.Infrastructure.Slack.Http.Response.Channel;
using Harald.Infrastructure.Slack.Http.Response.Conversation;
using Harald.Infrastructure.Slack.Http.Response.Notification;
using Harald.Infrastructure.Slack.Http.Response.User;
using Harald.Infrastructure.Slack.Http.Response.UserGroup;
using Harald.Infrastructure.Slack.Model;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Harald.Infrastructure.Slack
{
    public class SlackFacade : ISlackFacade
    {
        private readonly HttpClient _client;
        private readonly IDistributedCache _cache;
        private readonly IMemoryCache _tokenCache;
        private readonly string _botUserId;
        private readonly ILogger<SlackFacade> _logger;

        public SlackFacade( HttpClient client = null, IOptions<SlackOptions> options = null, IDistributedCache cache = null, IMemoryCache tokenCache = null, ILogger<SlackFacade> logger = null)
        {
            _client = client ?? new HttpClient() { BaseAddress = new System.Uri("https://slack.com", System.UriKind.Absolute) };
            _cache = cache;
            _tokenCache = tokenCache;
            _botUserId = options?.Value.SLACK_API_BOT_USER_ID ?? throw new SlackFacadeException("No SLACK_API_BOT_USER_ID was provided.");
            _logger = logger;
        }

        public string GetBotUserId() => _botUserId;

        public async Task<CreateChannelResponse> CreateChannel(SlackChannelName channelName)
        {
            using (var response = await SendAsync(new CreateConversationRequest(channelName)))
            { 
                return await Parse<CreateChannelResponse>(response);
            }
        }

        // This uses an undocumented API, tread carefully.
        public async Task DeleteChannel(SlackChannelIdentifier channelIdentifier, string token)
        {
            using (var response = await SendAsync(new DeleteChannelRequest(channelIdentifier, token)))
            {
                await Parse<SlackResponse>(response);
            }
        }
        

        public async Task LeaveChannel(SlackChannelIdentifier channelIdentifier)
        {
            using (var response = await SendAsync(new LeaveConversationRequest(channelIdentifier)))
            {
                await Parse<SlackResponse>(response);
            }
        }

        public async Task ArchiveChannel(SlackChannelIdentifier channelIdentifier)
        {
            using (var response = await SendAsync(new ArchiveConversationRequest(channelIdentifier)))
            {
                await Parse<SlackResponse>(response);
            }
        }

        public async Task RenameChannel(SlackChannelIdentifier channelIdentifier, SlackChannelName channelName)
        {
            using (var response = await SendAsync(new RenameConversationRequest(channelIdentifier, channelName)))
            {
                await Parse<SlackResponse>(response);
            }
        }

        public async Task<JoinChannelResponse> JoinChannel(SlackChannelName channelName)
        {
            using (var response = await SendAsync(new JoinConversationRequest(channelName)))
            {
                return await Parse<JoinChannelResponse>(response);
            }
        }

        public async Task<IEnumerable<ChannelDto>> GetChannels()
        {
            var tokenCacheKey = $"SlackGetChannels.{_client.DefaultRequestHeaders.Authorization.Parameter}";

            if (_tokenCache.Get(tokenCacheKey) == null)
            {
                using (var response = await SendAsync(new ListConversationRequest()))
                {
                    var result = await Parse<ListChannelsResponse>(response);
                    _tokenCache.Set(tokenCacheKey, result.Channels, DateTimeOffset.Now.AddSeconds(10));
                    return result.Channels;
                }
            }
            
            return _tokenCache.Get<IEnumerable<ChannelDto>>(tokenCacheKey);
        }

        public async Task<SendNotificationResponse> SendNotificationToChannel(SlackChannelIdentifier channelIdentifier, string message)
        {
            using (var response = await SendAsync(new SendNotificationRequest(channelIdentifier, message)))
            {
                return await Parse<SendNotificationResponse>(response);
            }
        }

        public async Task<SendNotificationResponse> SendNotificationToUser(string email, string message)
        {
            var userId = await GetUserId(email);

            return await SendNotificationToChannel(userId, message);
        }

        public async Task<SendNotificationResponse> SendDelayedNotificationToChannel(SlackChannelIdentifier channelIdentifier, string message, long delayTimeInEpoch)
        {
            using (var response = await SendAsync(new SendDelayedNotificationRequest(channelIdentifier, message, delayTimeInEpoch)))
            {
                return await Parse<SendNotificationResponse>(response);
            }
        }

        public async Task<SlackResponse> PinMessageToChannel(SlackChannelIdentifier channelIdentifier, string messageTimeStamp)
        {
            using (var response = await SendAsync(new PinMessageToChannelRequest(channelIdentifier, messageTimeStamp)))
            {
                return await Parse<SlackResponse>(response);
            }
        }

        public async Task InviteToChannel(string email, SlackChannelIdentifier channelIdentifier)
        {
            var userId = await GetUserId(email);

            using (var response = await SendAsync(new InviteToConversationRequest(channelIdentifier, new []{ userId })))
            {
                await Parse<SlackResponse>(response);
            }
        }

        public async Task RemoveFromChannel(string email, SlackChannelIdentifier channelIdentifier)
        {
            var userId = await GetUserId(email);
            
            using (var response = await SendAsync(new RemoveFromConversationRequest(channelIdentifier, userId)))
            {
                await Parse<SlackResponse>(response);
            }
        }

        public async Task<CreateUserGroupResponse> CreateUserGroup(string name, string handle, string description)
        {
            using (var response = await SendAsync(new CreateUserGroupRequest(name, handle, description)))
            {
                return await Parse<CreateUserGroupResponse>(response);
            }
        }

        public async Task RenameUserGroup(string userGroupId, string name, string handle)
        {
            using (var response = await SendAsync(new UpdateUserGroupRequest(userGroupId, name, handle)))
            {
                await Parse<SlackResponse>(response);
            }
        }

        public async Task AddUserGroupUser(string userGroupId, string name, string handle)
        {
            using (var response = await SendAsync(new UpdateUserGroupRequest(userGroupId, name, handle)))
            {
                await Parse<SlackResponse>(response);
            }
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

            // TODO: Implement proper behaviour for disabling/enabling usergroups, because Slack doesn't allow usergroups with less than 1 member.
            if (users.Count != 0)
            {
                await UpdateUserGroupUsers(userGroupId, users);
            }
        }

        public async Task<GetConversationsResponse> GetConversations()
        {
            const string tokenCacheKey = "SlackGetConversations";
            
            if (_tokenCache.Get(tokenCacheKey) == null)
            {
                using (var response = await SendAsync(new GetConversationsRequest()))
                {
                    var parsedResp = await Parse<GetConversationsResponse>(response);
                    _tokenCache.Set(tokenCacheKey, parsedResp, DateTimeOffset.Now.AddSeconds(10));
                    return parsedResp;
                }
            }
            
            return _tokenCache.Get<GetConversationsResponse>(tokenCacheKey);
        }

        public async Task<IEnumerable<UserGroupDto>> GetUserGroups()
        {
            using (var response = await SendAsync(new GetUserGroupsRequest()))
            {
                var data = await Parse<ListUserGroupsResponse>(response);

                return data.UserGroups;
            }
        }

        public async Task DisableUserGroup(string userGroupId)
        {
            using (var response = await SendAsync(new DisableUserGroupRequest(userGroupId)))
            {
                await Parse<SlackResponse>(response);
            }
        }

        private async Task UpdateUserGroupUsers(string userGroupId, List<string> users)
        {
            using (var response = await SendAsync(new UpdateUserGroupUsersRequest(userGroupId, users)))
            {
                await Parse<SlackResponse>(response);
            }
        }

        private async Task<List<string>> GetUserGroupUsers(string userGroupId)
        {
            using (var response = await SendAsync(new GetUserGroupUsersRequest(userGroupId)))
            {
                var data = await Parse<ListUsersInUserGroupResponse>(response);

                return data.Users;
            }
        }

        private async Task<string> GetUserId(string email)
        {
            string userId = string.Empty;
            
            if(_cache != null)
                userId = await _cache?.GetStringAsync(email);

            if (string.IsNullOrEmpty(userId))
            {
                using (var response = await SendAsync(new GetUserByEmailRequest(email)))
                {
                    var lookup = await Parse<LookupUserResponse>(response);

                    userId = lookup?.User.Id;

                    if(_cache != null && !string.IsNullOrEmpty(userId))
                    {
                        await _cache.SetStringAsync(email, userId);
                    }
                }
            }

            return userId;
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage, int retryPeriod = 0, int retryAttempts = 0)
        {
            const int mils = 1000;
            const int tooManyRequestsStatusCode = 429;
            const string retryAfterHeader = "retry-after";
            const int maximumRetryAttempts = 5;
            
            if (retryPeriod != 0 && retryAttempts <= maximumRetryAttempts)
            {
                await Task.Delay(retryPeriod * mils);
            }
            
            var resp = await _client.SendAsync(requestMessage);

            if (resp.StatusCode == (HttpStatusCode) tooManyRequestsStatusCode)
            {
                int retryAfter = 0;
                {
                    resp.Headers.TryGetValues(retryAfterHeader, out var retryRaw);
                    retryAfter = int.Parse(retryRaw.First());
                }
                
                _logger.Log(LogLevel.Information, $"URI '{requestMessage.RequestUri}' returned 429 TooManyRequests, waiting given period before trying again: {retryAfter} seconds.");
                return await SendAsync(await requestMessage.CloneAsync(), retryAfter, retryAttempts + 1);
            }

            return resp;
        }

        private async Task<T> Parse<T>(HttpResponseMessage response) where T : SlackResponse
        {
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<T>(content);

            switch (data)
            {
                case T msg when msg.Ok == true:
                    return data;
                case T msg when msg.Error == "channel_not_found":
                    throw new ChannelNotFoundException($"API error: {content}");
                case T msg when msg.Error == "not_authed":
                    throw new NotAuthorizedException($"API error: {content}");
                default:
                    throw new SlackFacadeException($"API error: {content}");
            }
        }
    }
}

namespace ExtensionMethods
{
    public static class HttpRequestMessageExtensions
    {
        public static async Task<HttpRequestMessage> CloneAsync(this HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Content = await request.Content.CloneAsync().ConfigureAwait(false),
                Version = request.Version
            };
            foreach (KeyValuePair<string, object> prop in request.Properties)
            {
                clone.Properties.Add(prop);
            }
            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }

        public static async Task<HttpContent> CloneAsync(this HttpContent content)
        {
            if (content == null) return null;

            var ms = new MemoryStream();
            await content.CopyToAsync(ms).ConfigureAwait(false);
            ms.Position = 0;

            var clone = new StreamContent(ms);
            foreach (KeyValuePair<string, IEnumerable<string>> header in content.Headers)
            {
                clone.Headers.Add(header.Key, header.Value);
            }
            return clone;
        }
    }
}