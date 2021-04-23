using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Harald.Infrastructure.Slack.Http.Request.Channel
{
    public class CreateChannelRequest : SlackRequest
    {
        public CreateChannelRequest(string channelName, bool validate = true)
        {
            var serializedContent = JsonConvert.SerializeObject(new { name = channelName, validate = validate }, _serializerSettings);

            Content = new StringContent(serializedContent, Encoding.UTF8, "application/json");
            RequestUri = new System.Uri("api/conversations.create", System.UriKind.Relative);
            Method = HttpMethod.Post;
        }
    }
}
