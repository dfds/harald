using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Harald.Infrastructure.Slack.Http.Request.Conversation
{
    public class RemoveFromConversationRequest : SlackRequest
    {
        public RemoveFromConversationRequest(string channelIdentifier, string userId)
        {
            var serializedContent = JsonConvert.SerializeObject(new { channel = channelIdentifier, user = userId }, _serializerSettings);

            Content = new StringContent(serializedContent, Encoding.UTF8, "application/json");
            RequestUri = new System.Uri("api/conversations.kick", System.UriKind.Relative);
            Method = HttpMethod.Post;
        }
    }
}
