using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Geocodificador
{
    class Program
    {
        private const string QueueName = "Geocodificador";
        public static IConfigurationRoot configuration;
        public static Support.AmqpService amqpService;

        static void Main(string[] args)
        {
            Console.WriteLine(" [*] Iniciando Geocodificador.");

            ServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            var factory = new ConnectionFactory() { Uri = new Uri(configuration.GetSection("amqp")["uri"]) };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                Console.WriteLine(" [*] Escuchando solicitudes...");

                channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Fanout);
                channel.QueueBind(queue: QueueName, exchange: "logs", routingKey: "");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    try
                    {
                        string url = configuration.GetSection("openstreenmap").Value;

                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        Models.Solicitud data = JsonConvert.DeserializeObject<Models.Solicitud>(message);
                        
                        Console.WriteLine("Solicitud recibida: [x] {0}", data);

                        if (data.Estado == Estados.PROCESANDO)
                        {

                            Console.WriteLine("ESTADO VALIDO PARA PROCESAR");

                            string getParams = String.Format("{0},{1},{2} {3},{4},{5}", new string[] {
                                 data.Calle,
                                 data.Numero,
                                 data.Codigo_Postal,
                                 data.Ciudad,
                                 data.Provincia,
                                 data.Pais
                            });

                            getParams = System.Web.HttpUtility.UrlEncode(getParams);
                            url += String.Format("/search?format=json&q={0}", new string[] { getParams });

                            Support.OpenStreetMapRequest.Coords coords = new Support.OpenStreetMapRequest().getCoordsFromAdress(url);
                            data.Latitud = coords.lat;
                            data.Longitud = coords.lon;

                            new Support.AmqpService().PublishMessage("ApiGeo.exchange", data);
                        }
                    
                    }
                    catch (Exception e){
                        Console.WriteLine(e.Message.ToString());
                    }

                };

                channel.BasicConsume(queue: QueueName,
                                     autoAck: true,
                                     consumer: consumer);

                Console.ReadLine();
            }

        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
         
            // Build configuration
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            // Add access to generic IConfigurationRoot
            serviceCollection.AddSingleton<IConfigurationRoot>(configuration);
        }
    }
}
