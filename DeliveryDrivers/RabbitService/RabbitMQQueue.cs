using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.IO;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using System.Text;

namespace DeliveryDrivers.RabbitService
{
    public class RabbitMQQueue
    {
        private IConnection _connection;
        private readonly IModel _channel;
        private const string _exchange = "mottu-service";
        private const string _queueName = "mottu-queue";

        public RabbitMQQueue()
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };
            var connection = factory.CreateConnection();
            var _channel = connection.CreateModel();

            _channel.ExchangeDeclare(_exchange, "topic", true, false);

            _channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(queue: _queueName, exchange: _exchange, routingKey: "mottu.#");
            _connection = connection;
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
            };

            _channel.BasicConsume(_queueName, false, consumer);
        }
        }
    }
