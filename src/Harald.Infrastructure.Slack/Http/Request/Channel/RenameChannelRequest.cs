using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Harald.Infrastructure.Slack.Http.Request.Channel
{
    public class RenameChannelRequest : SlackRequest
    {
        public RenameChannelRequest(string channelIdentifier, string channelName)
        {
            var serializedContent = JsonConvert.SerializeObject(new { channel = channelIdentifier, name = channelName }, _serializerSettings);

            Content = new StringContent(serializedContent, Encoding.UTF8, "application/json");
            RequestUri = new System.Uri("api/conversations.rename", System.UriKind.Relative);
            Method = HttpMethod.Post;
        }
    }
}
