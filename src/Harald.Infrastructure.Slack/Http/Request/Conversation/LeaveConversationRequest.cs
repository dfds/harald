﻿using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Harald.Infrastructure.Slack.Http.Request.Conversation
{
    public class LeaveConversationRequest : SlackRequest
    {
        public LeaveConversationRequest(string channelIdentifier)
        {
            var serializedContent = JsonConvert.SerializeObject(new { channel = channelIdentifier }, _serializerSettings);

            Content = new StringContent(serializedContent, Encoding.UTF8, "application/json");
            RequestUri = new System.Uri("api/conversations.leave", System.UriKind.Relative);
            Method = HttpMethod.Post;
        }
    }
}
