using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Harald.Infrastructure.Slack.Http.Request.Notification
{
    public class SendNotificationRequest : SlackRequest
    {
        //https://api.slack.com/methods/chat.postMessage#authorship
        public SendNotificationRequest(string channelIdentifier, string message)
        {
            var serializedContent = JsonConvert.SerializeObject(new { channel = channelIdentifier, text = message }, _serializerSettings);

            Content = new StringContent(serializedContent, Encoding.UTF8, "application/json");
            RequestUri = new System.Uri("api/chat.postMessage", System.UriKind.Relative);
            Method = HttpMethod.Post;
        }
    }
}
