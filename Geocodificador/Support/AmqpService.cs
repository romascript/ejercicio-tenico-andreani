using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geocodificador.Support
{
    public class AmqpService
    {
        public void PublishMessage(string QueueName, object message)
        {

            var connectionFactory = new ConnectionFactory{Uri = new Uri("amqps://ltkpeshz:a-JalXkhVBLedr6UDWaVhgwbC8ub80DA@hornet.rmq.cloudamqp.com/ltkpeshz")};
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

                    channel.BasicPublish(exchange: "ApiGeo.exchange",
                        routingKey: "",
                        basicProperties: null,
                        body: body
                    );
                }
            }
        }
    }
}
