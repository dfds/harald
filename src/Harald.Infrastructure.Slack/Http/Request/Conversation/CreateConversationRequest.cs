﻿using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Harald.Infrastructure.Slack.Http.Request.Conversation
{
    public class CreateConversationRequest : SlackRequest
    {
        public CreateConversationRequest(string channelName)
        {
            var serializedContent = JsonConvert.SerializeObject(new { name = channelName }, _serializerSettings);

            Content = new StringContent(serializedContent, Encoding.UTF8, "application/json");
            RequestUri = new System.Uri("api/conversations.create", System.UriKind.Relative);
            Method = HttpMethod.Post;
        }
    }
}
