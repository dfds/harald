using System.Net.Http;

namespace Harald.Infrastructure.Slack.Http.Request.Conversation
{
    public class ListConversationRequest : SlackRequest
    {
        public ListConversationRequest(bool excludeArchived = true)
        {
            RequestUri = new System.Uri($"api/conversations.list?limit=10000&exclude_archived={excludeArchived}", System.UriKind.Relative);
            Method = HttpMethod.Get;
        }
    }
}
