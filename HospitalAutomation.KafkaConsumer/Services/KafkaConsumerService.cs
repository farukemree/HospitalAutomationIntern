using Confluent.Kafka;
using HospitalAutomation.DataAccess.Models;
using HospitalAutomation.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalAutomation.KafkaConsumer.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public KafkaConsumerService(ILogger<KafkaConsumerService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;

            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "hospital-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();
            _consumer.Subscribe(new List<string>
            {
                "hospital-server.dbo.Departments",
                "hospital-server.dbo.Doctors"
            });
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Kafka tüketici servisi başladı.");

            Task.Run(async () =>
            {
                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            var result = _consumer.Consume(stoppingToken);

                            if (result != null)
                            {
                                _logger.LogInformation($"Mesaj alındı: Topic={result.Topic}, Key={result.Message.Key}, Value={result.Message.Value}");

                                using var scope = _serviceProvider.CreateScope();
                                var redisCacheService = scope.ServiceProvider.GetRequiredService<IRedisCacheService>();

                                if (result.Topic == "hospital-server.dbo.Departments")
                                {
                                    var payload = JsonSerializer.Deserialize<DebeziumPayload<Department>>(result.Message.Value);
                                    if (payload?.after != null)
                                    {
                                        string redisKey = $"department:{payload.after.Id}";
                                        string redisValue = JsonSerializer.Serialize(payload.after);
                                        await redisCacheService.SetAsync(redisKey, redisValue, TimeSpan.FromMinutes(10));
                                        _logger.LogInformation($"Departman cache güncellendi: {redisKey}");
                                    }
                                    else if (payload?.before != null)
                                    {
                                        string redisKey = $"department:{payload.before.Id}";
                                        await redisCacheService.RemoveAsync(redisKey);
                                        _logger.LogInformation($"Departman cache silindi: {redisKey}");
                                    }
                                }
                                else if (result.Topic == "hospital-server.dbo.Doctors")
                                {
                                    var payload = JsonSerializer.Deserialize<DebeziumPayload<Doctor>>(result.Message.Value);
                                    if (payload?.after != null)
                                    {
                                        string redisKey = $"doctor:{payload.after.Id}";
                                        string redisValue = JsonSerializer.Serialize(payload.after);
                                        await redisCacheService.SetAsync(redisKey, redisValue, TimeSpan.FromMinutes(10));
                                        _logger.LogInformation($"Doktor cache güncellendi: {redisKey}");
                                    }
                                    else if (payload?.before != null)
                                    {
                                        string redisKey = $"doctor:{payload.before.Id}";
                                        await redisCacheService.RemoveAsync(redisKey);
                                        _logger.LogInformation($"Doktor cache silindi: {redisKey}");
                                    }
                                }
                            }
                        }
                        catch (ConsumeException ex)
                        {
                            _logger.LogError(ex, "Kafka tüketim hatası");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Kafka tüketici servisi iptal edildi.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kafka tüketici servisi genel hata");
                }
                finally
                {
                    _consumer.Close();
                    _consumer.Dispose();
                }
            });

            return Task.CompletedTask;
        }

    }
}
