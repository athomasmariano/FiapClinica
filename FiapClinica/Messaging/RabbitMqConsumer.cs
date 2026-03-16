using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace FiapClinica.Messaging;

public static class RabbitMqConsumer
{
    public static void ConsoleConsume()
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "fila_pacientes",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"\n [x] Mensagem Recebida do RabbitMQ: {message}");
        };

        channel.BasicConsume(queue: "fila_pacientes",
                             autoAck: true,
                             consumer: consumer);
    }
}