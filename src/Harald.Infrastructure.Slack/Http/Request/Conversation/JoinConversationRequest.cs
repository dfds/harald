using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Harald.Infrastructure.Slack.Http.Request.Conversation
{
    public class JoinConversationRequest : SlackRequest
    {
        public JoinConversationRequest(string channelName, bool validate = false)
        {
            var serializedContent = JsonConvert.SerializeObject(new { name = channelName, validate }, _serializerSettings);

            Content = new StringContent(serializedContent, Encoding.UTF8, "application/json");
            RequestUri = new System.Uri("api/conversations.join", System.UriKind.Relative);
            Method = HttpMethod.Post;
        }
    }
}
