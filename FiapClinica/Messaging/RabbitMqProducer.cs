using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace FiapClinica.Messaging;

public static class RabbitMqProducer
{
    public static void EnviarMensagem<T>(T mensagem)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "fila_pacientes",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var json = JsonSerializer.Serialize(mensagem);
        var body = Encoding.UTF8.GetBytes(json);

        channel.BasicPublish(exchange: "",
                             routingKey: "fila_pacientes",
                             basicProperties: null,
                             body: body);
    }
}