using ApiGeo.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ApiGeo.BackgroundServices
{
    public class ConsumeRabbitMQHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        private IConnection _connection;
        private IModel _channel;
        private const string QueueName = "ApiGeo";
        private readonly SolicitudContext _context;

        public ConsumeRabbitMQHostedService(ILoggerFactory loggerFactory, IServiceScopeFactory factory)
        {
            this._context = factory.CreateScope().ServiceProvider.GetRequiredService<SolicitudContext>();
            this._logger = loggerFactory.CreateLogger<ConsumeRabbitMQHostedService>();
            InitRabbitMQ();
        }
        private void InitRabbitMQ()
        {
            var factory = new ConnectionFactory { Uri = new Uri("amqps://ltkpeshz:a-JalXkhVBLedr6UDWaVhgwbC8ub80DA@hornet.rmq.cloudamqp.com/ltkpeshz") };

            // create connection  
            _connection = factory.CreateConnection();

            // create channel  
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare("ApiGeo.exchange", ExchangeType.Fanout);
            _channel.QueueDeclare(QueueName, false, false, false, null);
            _channel.QueueBind(QueueName, "ApiGeo.exchange", "", null);
            //_channel.BasicQos(0, 1, false);

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                Models.Solicitud data = JsonConvert.DeserializeObject<Models.Solicitud>(content);

                Models.Solicitud row = _context.SolicitudItems.FirstOrDefault(item => item.Id == data.Id);
                if (row != null)
                {
                    if (row.Estado == Estados.PROCESANDO)
                    {
                        row.Latitud = data.Latitud;
                        row.Longitud = data.Longitud;
                        row.Estado = Estados.TERMINADO;

                        _context.SaveChanges();
                    }
                }

                HandleMessage(content);
                _channel.BasicAck(ea.DeliveryTag, false);
            };

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume(QueueName, false, consumer);
            return Task.CompletedTask;
        }
        private void HandleMessage(string content)
        {
            _logger.LogInformation(content);
        }
        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }
        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
