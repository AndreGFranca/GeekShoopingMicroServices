using GeekShopping.PaymentAPI.Messages;
using GeekShopping.PaymentAPI.RabbitMQSender;
using GeekShopping.PaymentProcessor;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace GeekShopping.PaymentAPI.MessageConsumer
{
    public class RabbitMQPaymentConsumer : BackgroundService
    {
        private readonly string _hostName;
        private readonly string _password;
        private readonly string _username;
        private IConnection _connection;
        private IModel _channel;
        private IProcessPayment _processPayment;
        private IRabbitMQMessageSender _rabbitMQMessageSender;
        public RabbitMQPaymentConsumer(IProcessPayment processPayment, IRabbitMQMessageSender rabbitMQMessageSender)
        {
            _processPayment = processPayment;
            _rabbitMQMessageSender = rabbitMQMessageSender;

            _hostName = "localhost";
            _password = "guest";
            _username = "guest";

            var factory = new ConnectionFactory
            {
                HostName = _hostName,
                UserName = _username,
                Password = _password,
            };
            // Conecta de forma assíncrona
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "orderpaymentprocessqueue", false, false, false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (chanel, evt) =>
            {
                var content = Encoding.UTF8.GetString(evt.Body.ToArray());
                PaymentMessage dto = JsonSerializer.Deserialize<PaymentMessage>(content);
                ProcessPayment(dto).GetAwaiter().GetResult();
                _channel.BasicAck(evt.DeliveryTag, false);
            };
            _channel.BasicConsume("orderpaymentprocessqueue", false, consumer);
            return Task.CompletedTask;
        }

        private async Task ProcessPayment(PaymentMessage dto)
        {
            var result = _processPayment.PaymentProcessor();
            UpdatePaymentResultMessage paymentResultMessage = new() 
            { 
                Status = result,
                OrderId = dto.OrderId,
                Email = dto.Email
            };
            try
            {
                //_messageSender.Send(paymentDTO, "orderpaymentprocessqueue");
                _rabbitMQMessageSender.Send(paymentResultMessage);

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
