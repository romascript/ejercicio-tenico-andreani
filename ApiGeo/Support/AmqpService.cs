using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGeo.Support
{
    public class AmqpService
    {
        private readonly AmqpInfo amqpInfo;
        private readonly ConnectionFactory connectionFactory;
        public AmqpService(IOptions<AmqpInfo> ampOptionsSnapshot)
        {
            amqpInfo = ampOptionsSnapshot.Value;

            connectionFactory = new ConnectionFactory
            {
                UserName = amqpInfo.Username,
                Password = amqpInfo.Password,
                VirtualHost = amqpInfo.VirtualHost,
                HostName = amqpInfo.HostName,
                Uri = new Uri(amqpInfo.Uri)
            };
        }
        public void PublishMessage(string QueueName, object message)
        {
            using (var conn = connectionFactory.CreateConnection())
            {
                using (var channel = conn.CreateModel())
                {
                    channel.QueueDeclare(
                        queue: QueueName,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

                    var jsonPayload = JsonConvert.SerializeObject(message);
                    var body = Encoding.UTF8.GetBytes(jsonPayload);

                    channel.BasicPublish(exchange: "",
                        routingKey: QueueName,
                        basicProperties: null,
                        body: body
                    );
                }
            }
        }
        public void ReceiveMessage(string QueueName)
        {
            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {

                channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Fanout);

                channel.QueueBind(queue: QueueName,
                                  exchange: "logs",
                                  routingKey: "");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var data = JObject.Parse(message);
                        System.Diagnostics.Debug.WriteLine("Solicitud recibida: [x] {0}", data);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message.ToString());
                    }

                };

                channel.BasicConsume(queue: QueueName,
                                     autoAck: true,
                                     consumer: consumer);
            }
        }
    }
}
