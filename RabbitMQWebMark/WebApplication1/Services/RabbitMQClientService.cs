using RabbitMQ.Client;

namespace UdemyRabbitMQWeb.Webmark.Services
{
    public class RabbitMQClientService: IDisposable //implement ettik
    {
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        public static string ExchangeName = "ImageDirectExchange";
        public static string RoutingWebmark = "watermark-route-image";
        public static string QueueName = "queue-watermark-image";

        private readonly ILogger<RabbitMQClientService> _logger;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
            Connect();
        }
        public IModel Connect()
        {
            _connectionFactory.DispatchConsumersAsync = true;
            _connection = _connectionFactory.CreateConnection();

            if(_channel is { IsOpen: true })
            {
                return _channel;
            }

            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(ExchangeName, type: "direct", true, false);
            _channel.QueueDeclare(QueueName, true, false, false, null);
            _channel.QueueBind(exchange: ExchangeName, queue: QueueName, routingKey: RoutingWebmark);// ya da ( QueueName, ExchangeName, RoutingWebmark)

            _logger.LogInformation("RabbitMQ ile bağlantı kuruldu...");

            return _channel;
        }

        public void Dispose()
        {
            _channel?.Close();//channel varsa
            _channel?.Dispose(); 

            _channel?.Close();
            _connection?.Dispose();

            _logger.LogInformation("RabbitMQ ile bağlantı koptu.");
        }
    }

    
}
