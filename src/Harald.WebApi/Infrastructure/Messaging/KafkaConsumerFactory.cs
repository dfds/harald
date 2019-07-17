using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace Harald.WebApi.Infrastructure.Messaging
{
    public class KafkaConsumerFactory
    {
        private readonly KafkaConfiguration _configuration;

        public KafkaConsumerFactory(KafkaConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IConsumer<string, string> Create()
        {
            var config = new ConsumerConfig(_configuration.GetConsumerConfiguration());
            var builder = new ConsumerBuilder<string, string>(config);
            builder.SetErrorHandler(OnKafkaError);
            return builder.Build();
        }

        private void OnKafkaError(IConsumer<string, string> producer, Error error)
        {
            if (error.IsFatal)
                Environment.FailFast($"Fatal error in Kafka producer: {error.Reason}. Shutting down...");
            else
                throw new Exception(error.Reason);
        }

        public class KafkaConfiguration
        {
            private const string KEY_PREFIX = "HARALD_KAFKA_";
            private readonly IConfiguration _configuration;

            public KafkaConfiguration(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            private string Key(string keyName) => string.Join("", KEY_PREFIX, keyName.ToUpper().Replace('.', '_'));

            private Tuple<string, string> GetConfiguration(string key)
            {
                var value = _configuration[Key(key)];

                if (string.IsNullOrWhiteSpace(value))
                {
                    return null;
                }

                return Tuple.Create<string, string>(key, value);
            }
            
            public ConsumerConfig GetConsumerConfiguration()
            {
                return new ConsumerConfig(AsEnumerable());
            }

            public IEnumerable<KeyValuePair<string, string>> AsEnumerable()
            {
                var configurationKeys = new[]
                {
                    "group.id",
                    "enable.auto.commit",
                    "bootstrap.servers",
                    "broker.version.fallback",
                    "api.version.fallback.ms",
                    "ssl.ca.location",
                    "sasl.username",
                    "sasl.password",
                    "sasl.mechanisms",
                    "security.protocol",
                };

                var config = configurationKeys
                    .Select(key => GetConfiguration(key))
                    .Where(pair => pair != null)
                    .Select(pair => new KeyValuePair<string, string>(pair.Item1, pair.Item2))
                    .ToList();
                
                config.Add(new KeyValuePair<string, string>("request.timeout.ms", "3000"));

                return config;
            }
        }
    }
}