﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Harald.WebApi.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Harald.WebApi.Enablers.KafkaMessageConsumer.Infrastructure
{
    public class KafkaConsumerHostedService : IHostedService
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly ILogger<KafkaConsumerHostedService> _logger;
        private readonly KafkaConsumerFactory _consumerFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly DomainEventRegistry _eventRegistry;

        private Task _executingTask;

        public KafkaConsumerHostedService(
            ILogger<KafkaConsumerHostedService> logger,
            IServiceProvider serviceProvider,
            KafkaConsumerFactory kafkaConsumerFactory,
            DomainEventRegistry domainEventRegistry)
        {
            Console.WriteLine($"Starting event consumer.");

            _logger = logger;
            _consumerFactory = kafkaConsumerFactory;
            _serviceProvider = serviceProvider;
            _eventRegistry = domainEventRegistry;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _executingTask = Task.Factory.StartNew(async () =>
                {
                    using (var consumer = _consumerFactory.Create())
                    {
                        var topics = _eventRegistry.GetAllTopics();

                        _logger.LogInformation($"Event consumer started. Listening to topics: {string.Join(",", topics)}");
                                                
                        consumer.Subscribe(topics);

                        // consume loop
                        while (!_cancellationTokenSource.IsCancellationRequested)
                        {
                            ConsumeResult<string, string> msg;
                            try
                            {
                                msg = consumer.Consume(cancellationToken);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Consumption of event failed, reason: {ex}");
                                continue;
                            }
                            
                            using (var scope = _serviceProvider.CreateScope())
                            {
                                _logger.LogInformation($"Received event: Topic: {msg.Topic} Partition: {msg.Partition}, Offset: {msg.Offset} {msg.Value}");

                                try
                                {
                                    var eventDispatcher = scope.ServiceProvider.GetRequiredService<IEventDispatcher>();
                                    await eventDispatcher.Send(msg.Value, scope);
                                    await Task.Run(() => consumer.Commit(msg));
                                }
                                catch (Exception ex) when (ex is EventTypeNotFoundException || ex is EventHandlerNotFoundException )
                                {
                                    _logger.LogWarning($"Message skipped. Exception message: {ex.Message}", ex);
                                    await Task.Run(() => consumer.Commit(msg));
                                }
                                catch (EventMessageIncomprehensible ex)
                                {
                                    _logger.LogWarning(ex, $"Encountered a message that was irrecoverably incomprehensible. Skipping. Raw message included {msg} with value '{msg.Value}'");
                                    await Task.Run(() => consumer.Commit(msg));
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Error consuming event.");
                                }
                            }
                            
                        }
                    }
                }, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default)
                .ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        _logger.LogError(task.Exception, "Event loop crashed");
                    }
                }, cancellationToken);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _cancellationTokenSource.Cancel();
            }
            finally
            {
                await Task.WhenAny(_executingTask, Task.Delay(-1, cancellationToken));
            }

            _cancellationTokenSource.Dispose();
        }
    }
}