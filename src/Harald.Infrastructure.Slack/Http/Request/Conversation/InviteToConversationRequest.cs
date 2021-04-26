using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Harald.Infrastructure.Slack.Http.Request.Conversation
{
    public class InviteToConversationRequest : SlackRequest
    {
        public InviteToConversationRequest(string channelIdentifier, IEnumerable<string> userIds)
        {
            var serializedContent = JsonConvert.SerializeObject(new { channel = channelIdentifier, users = userIds.Aggregate((s1, s2) => $"{s1},{s2}") }, _serializerSettings);

            Content = new StringContent(serializedContent, Encoding.UTF8, "application/json");
            RequestUri = new System.Uri("api/conversations.invite", System.UriKind.Relative);
            Method = HttpMethod.Post;
        }
    }
}
