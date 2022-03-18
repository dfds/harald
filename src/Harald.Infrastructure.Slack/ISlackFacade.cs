using Harald.Infrastructure.Slack.Dto;
using Harald.Infrastructure.Slack.Http.Response;
using Harald.Infrastructure.Slack.Http.Response.Conversation;
using Harald.Infrastructure.Slack.Http.Response.Notification;
using Harald.Infrastructure.Slack.Http.Response.UserGroup;
using Harald.Infrastructure.Slack.Model;
using Harald.Infrastructure.Slack.Http.Response.Channel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Harald.Infrastructure.Slack
{
    public interface ISlackFacade
    {
        Task<SendNotificationResponse> SendNotificationToChannel(SlackChannelIdentifier channelIdentifier, string message);

        Task<SendNotificationResponse> SendDelayedNotificationToChannel(SlackChannelIdentifier channelIdentifier, string message, long delayTimeInEpoch);

        Task<SendNotificationResponse> SendNotificationToUser(string email, string message);

        string GetBotUserId();
        string GetDefaultNotificationChannelId();

        Task<SlackResponse> PinMessageToChannel(SlackChannelIdentifier channelIdentifier, string messageTimeStamp);

        Task<CreateChannelResponse> CreateChannel(SlackChannelName channelName);

        Task DeleteChannel(SlackChannelIdentifier channelIdentifier, string token);

        Task RenameChannel(SlackChannelIdentifier channelIdentifier, SlackChannelName channelName);

        Task LeaveChannel(SlackChannelIdentifier channelIdentifier);

        Task ArchiveChannel(SlackChannelIdentifier channelIdentifier);

        Task<JoinChannelResponse> JoinChannel(SlackChannelName channelName);

        Task<IEnumerable<ChannelDto>> GetChannels();

        Task InviteToChannel(string email, SlackChannelIdentifier channelIdentifier);

        Task RemoveFromChannel(string email, SlackChannelIdentifier channelIdentifier);

        Task<CreateUserGroupResponse> CreateUserGroup(string name, string handle, string description);

        Task DisableUserGroup(string userGroupId);

        Task RenameUserGroup(string userGroupId, string name, string handle);

        Task AddUserGroupUser(string userGroupId, string email);

        Task RemoveUserGroupUser(string userGroupId, string email);

        Task<IEnumerable<UserGroupDto>> GetUserGroups();

        Task<GetConversationsResponse> GetConversations();
    }
}