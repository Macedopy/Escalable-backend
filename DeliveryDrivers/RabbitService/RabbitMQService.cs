using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Newtonsoft.Json;
using System.Text;

namespace MotorcycleRental.RabbitService
{
    public interface IRabbitBusService
    {
        void Publish (string message);
    }

    public class RabbitMQService : IRabbitBusService
    {
        ConnectionFactory _factory;
        private IConnection _connection;
        private readonly IModel _channel;
        private const string _exchange = "motorcycle-exchange";
        public RabbitMQService()
        {
            _factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            _connection = _factory.CreateConnection("Mottu_Services");
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: _exchange, type: "direct", durable: true, autoDelete: false, arguments: null);
            _channel.QueueDeclare(queue: "motorcycle-queue", 
                                durable: false, 
                                exclusive: false, 
                                autoDelete: false, 
                                arguments: null);
            _channel.QueueBind("motorcycle-queue", exchange: _exchange, routingKey: "motorcycle-queue");
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }

        public void Publish(string message)
        {
            var bytearray = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(_exchange, "motorcycle-queue", null, bytearray);
            Console.WriteLine("Publish message: {0}", message);
        }
    }
}