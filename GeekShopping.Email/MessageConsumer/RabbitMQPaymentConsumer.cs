using GeekShopping.Email.Messages;
using GeekShopping.Email.Repository;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace GeekShopping.Email.MessageConsumer
{
    public class RabbitMQPaymentConsumer : BackgroundService
    {
        private readonly EmailRepository _emailRepository;
        private readonly string _hostName;
        private readonly string _password;
        private readonly string _username;
        private IConnection _connection;
        private IModel _channel;
        private const string ExchangeName = "DirectPaymentUpdateExchange";
        private const string PaymentEmailUpdateQueueName = "PaymentEmailUpdateQueueName";

        public RabbitMQPaymentConsumer(EmailRepository emailRepository)
        {
            _emailRepository = emailRepository;

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
            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct);//queue: "orderpaymentresultqueue", false, false, false, arguments: null);
            _channel.QueueDeclare(PaymentEmailUpdateQueueName, false, false, false, null);
            _channel.QueueBind(PaymentEmailUpdateQueueName, ExchangeName, "PaymentEmail");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (chanel, evt) =>
            {
                var content = Encoding.UTF8.GetString(evt.Body.ToArray());
                UpdatePaymentResultMessage dto = JsonSerializer.Deserialize<UpdatePaymentResultMessage>(content);
                ProcessLogs(dto).GetAwaiter().GetResult();
                _channel.BasicAck(evt.DeliveryTag, false);
            };
            _channel.BasicConsume(PaymentEmailUpdateQueueName, false, consumer);
            return Task.CompletedTask;
        }

        private async Task ProcessLogs(UpdatePaymentResultMessage message)
        {            
            try
            {
                await _emailRepository.LogEmail(message);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
