using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harald.WebApi.Domain;
using Harald.Infrastructure.Slack;
using Harald.Infrastructure.Slack.Dto;
using Harald.Infrastructure.Slack.Http.Response.Notification;
using Harald.Infrastructure.Slack.Http.Response;
using Harald.Infrastructure.Slack.Http.Response.Channel;
using Harald.Infrastructure.Slack.Http.Response.UserGroup;
using Harald.Infrastructure.Slack.Http.Response.Conversation;
using Harald.Infrastructure.Slack.Model;

namespace Harald.Tests.TestDoubles
{
    public class SlackFacadeSpy : ISlackFacade
    {
        public List<UserGroupDto> UserGroups;

        public SlackFacadeSpy()
        {
            UserGroups = new List<UserGroupDto>();
        }

        public Dictionary<SlackChannelIdentifier, List<string>> ChannelsMessages = new Dictionary<SlackChannelIdentifier, List<string>>();

        public Task<SendNotificationResponse> SendNotificationToChannel(SlackChannelIdentifier channelId, string message)
        {
            if (ChannelsMessages.ContainsKey(channelId) == false)
            {
                ChannelsMessages.Add(channelId, new List<string>());
            }

            ChannelsMessages[channelId].Add(message);

            var sendNotificationResponse = new SendNotificationResponse
            {
                Ok = true,
                TimeStamp = "1355517523.000005"
            };

            return Task.FromResult(sendNotificationResponse);
        }

        
        public Task<SendNotificationResponse> SendDelayedNotificationToChannel(SlackChannelIdentifier channelId, string message,
            long delayTimeInEpoch)
        {
            throw new System.NotImplementedException();
        }
        public readonly Dictionary<string, List<string>> UsersToNotifications = new Dictionary<string, List<string>>();

        public Task<SendNotificationResponse> SendNotificationToUser(string email, string message)
        {
            if (UsersToNotifications.ContainsKey(email) == false)
            {
                UsersToNotifications.Add(email, new List<string>());
            }

            UsersToNotifications[email].Add(message);

            
            var sendNotificationResponse = new SendNotificationResponse
            {
                Ok = true,
                TimeStamp = "1355517523.000005"
            };

            return Task.FromResult(sendNotificationResponse);
        }

        
        public readonly Dictionary<SlackChannelIdentifier, List<string>> ChannelsPinnedMessageTimeStamps =
            new Dictionary<SlackChannelIdentifier, List<string>>();

        public Task<SlackResponse> PinMessageToChannel(SlackChannelIdentifier channelId, string messageTimeStamp)
        {
            if (ChannelsPinnedMessageTimeStamps.ContainsKey(channelId) == false)
            {
                ChannelsPinnedMessageTimeStamps.Add(channelId, new List<string>());
            }

            ChannelsPinnedMessageTimeStamps[channelId].Add(messageTimeStamp);

            return Task.FromResult(new SlackResponse { Ok = true});
        }

        public string GetBotUserId()
        {
            return "";
        }

        public SlackChannelName CreatedChannelName { get; private set; }

        public Task<CreateChannelResponse> CreateChannel(SlackChannelName channelName)
        {
            CreatedChannelName = channelName;

            var channel = new ChannelDto
            {
                Id = new ChannelId(Guid.NewGuid().ToString()),
                Name = channelName
            };

            var createChannelResponse = new CreateChannelResponse
            {
                Ok = true,
                Channel = channel
            };


            return Task.FromResult(createChannelResponse);
        }

        public Task DeleteChannel(SlackChannelIdentifier channelId, string token)
        {
            throw new System.NotImplementedException();
        }

        public Task RenameChannel(SlackChannelIdentifier channelId, SlackChannelName name)
        {
            throw new System.NotImplementedException();
        }

        public readonly Dictionary<SlackChannelIdentifier, List<string>>
            InvitedToChannel = new Dictionary<SlackChannelIdentifier, List<string>>();

        public Task InviteToChannel(string email, SlackChannelIdentifier channelId)
        {
            if (InvitedToChannel.ContainsKey(channelId) == false)
            {
                InvitedToChannel.Add(channelId, new List<string>());
            }

            InvitedToChannel[channelId].Add(email);

            return Task.CompletedTask;
        }

        public readonly Dictionary<SlackChannelIdentifier, List<string>> RemovedFromChannel =
            new Dictionary<SlackChannelIdentifier, List<string>>();

        public Task RemoveFromChannel(string email, SlackChannelIdentifier channelId)
        {
            if (RemovedFromChannel.ContainsKey(channelId) == false)
            {
                RemovedFromChannel.Add(channelId, new List<string>());
            }

            RemovedFromChannel[channelId].Add(email);

            return Task.CompletedTask;
        }

        public Task<CreateUserGroupResponse> CreateUserGroup(string name, string handle, string description)
        {
            CreateUserGroupWasCalled = true;
            CreateUserGroupName = name;
            CreateUserGroupHandle = handle;
            CreateUserGroupDescription = description;

            var usergroup = new UserGroupDto
            {
                Handle = handle,
                Id = Guid.NewGuid().ToString(),
                Name = name
            };

            return Task.FromResult(new CreateUserGroupResponse {Ok = true, UserGroup = usergroup});
        }

        public Task RenameUserGroup(string usergroupId, string name, string handle)
        {
            throw new System.NotImplementedException();
        }

        public string CreateUserGroupDescription { get; private set; }

        public string CreateUserGroupHandle { get; private set; }

        public string CreateUserGroupName { get; private set; }
        public readonly Dictionary<string, List<string>> UserGroupsUsers = new Dictionary<string, List<string>>();

        public Task AddUserGroupUser(string userGroupId, string email)
        {
            if (UserGroupsUsers.ContainsKey(userGroupId) == false)
            {
                UserGroupsUsers.Add(userGroupId, new List<string>());
            }

            UserGroupsUsers[userGroupId].Add(email);

            return Task.CompletedTask;
        }

        public readonly Dictionary<string, List<string>> RemovedFromUsergroup = new Dictionary<string, List<string>>();
        public Task RemoveUserGroupUser(string userGroupId, string email)
        {
            if (RemovedFromUsergroup.ContainsKey(userGroupId) == false)
            {
                RemovedFromUsergroup.Add(userGroupId, new List<string>());
            }

            RemovedFromUsergroup[userGroupId].Add(email);

            return Task.CompletedTask;
        }

        public Task<IEnumerable<UserGroupDto>> GetUserGroups()
        {
            return Task.FromResult(UserGroups.AsEnumerable());
        }

        public readonly List<ChannelDto> Conversations = new List<ChannelDto>();
        public Task<GetConversationsResponse> GetConversations()
        {
            return Task.FromResult(new GetConversationsResponse
            {
                Channels = Conversations,
                Ok = true
            });
        }

        public Task LeaveChannel(SlackChannelIdentifier channelIdentifier)
        {
            throw new NotImplementedException();
        }

        public Task ArchiveChannel(SlackChannelIdentifier channelIdentifier)
        {
            throw new NotImplementedException();
        }

        public Task<JoinChannelResponse> JoinChannel(SlackChannelName channelName)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ChannelDto>> GetChannels()
        {
            throw new NotImplementedException();
        }

        public Task DisableUserGroup(string userGroupId)
        {
            throw new NotImplementedException();
        }

        public bool CreateUserGroupWasCalled { get; private set; }
    }
}