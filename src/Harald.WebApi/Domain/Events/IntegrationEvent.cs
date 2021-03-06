using Newtonsoft.Json;

namespace Harald.WebApi.Domain.Events
{
    public class IntegrationEvent
    {

        public string Version { get; private set; }
        public string EventName { get; private set; }
        [JsonProperty(PropertyName = "x-correlationId")]
        public string XCorrelationId { get; private set; }
        [JsonProperty(PropertyName = "x-sender")]
        public string XSender { get; private set; }

        public object Payload { get; private set; }

        public IntegrationEvent(
            string version, string eventName, string xCorrelationId, string xSender, object payload)
        {
            Version = version;
            EventName = eventName;
            XCorrelationId = xCorrelationId;
            XSender = xSender;
            Payload = payload;
        }
    }
}