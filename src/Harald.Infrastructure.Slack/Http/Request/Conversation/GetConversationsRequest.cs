﻿using System.Net.Http;

namespace Harald.Infrastructure.Slack.Http.Request.Conversation
{
    public class GetConversationsRequest : SlackRequest
    {
        public GetConversationsRequest(bool excludeArchived = true)
        {            
            RequestUri = new System.Uri("api/conversations.list?exclude_archived={excludeArchived}", System.UriKind.Relative);
            Method = HttpMethod.Get;
        }
    }
}
