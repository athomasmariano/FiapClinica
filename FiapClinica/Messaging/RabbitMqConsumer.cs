using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using FiapClinica.Models;

namespace FiapClinica.Messaging;

public class RabbitMqConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqConsumer()
    {
        try
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "paciente-criado",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }
        catch (Exception ex)
        {
            // Evita que a API crashe se o RabbitMQ estiver offline ao iniciar
            Console.WriteLine($"\n [ALERTA WORKER] RabbitMQ fora do ar na inicialização. Consumer inativo. Erro: {ex.Message}\n");
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        // Trava de segurança: Se a conexão falhou no construtor, nem tenta escutar a fila
        if (_channel == null)
        {
            return Task.CompletedTask;
        }

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine($"\n [x] Mensagem Recebida do RabbitMQ (Worker): {message}");

            //Desafio Extra 4.2 Lógica de E-mail
            try
            {
                var paciente = JsonSerializer.Deserialize<Paciente>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                Console.WriteLine("\n=======================================================");
                Console.WriteLine("     DISPARO DE E-MAIL AUTOMÁTICO (Desafio 4.2)");
                Console.WriteLine($"    Para: {paciente?.Email}");
                Console.WriteLine($"    Assunto: Bem-vindo(a) à FIAP Clínica, {paciente?.Nome}!");
                Console.WriteLine("     Corpo: Seu cadastro foi realizado com sucesso. Aguardamos sua visita.");
                Console.WriteLine("=======================================================\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n [x] Erro ao processar lógica do e-mail no consumer: {ex.Message}");
            }

            _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        };

        _channel.BasicConsume(queue: "paciente-criado",
                             autoAck: false,
                             consumer: consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        // Só fecha se realmente conseguiu abrir
        if (_channel != null && _channel.IsOpen) _channel.Close();
        if (_connection != null && _connection.IsOpen) _connection.Close();
        base.Dispose();
    }
}