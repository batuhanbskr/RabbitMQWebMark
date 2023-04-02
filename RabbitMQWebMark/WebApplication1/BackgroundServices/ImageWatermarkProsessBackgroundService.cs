using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Drawing;
using System.Text;
using System.Text.Json;
using UdemyRabbitMQWeb.Webmark.Services;

namespace UdemyRabbitMQWeb.Webmark.BackgroundServices
{
    public class ImageWatermarkProsessBackgroundService : BackgroundService
    {
        private readonly RabbitMQClientService _rabbitMQClientService; //channel'a eriştik
        private readonly ILogger<ImageWatermarkProsessBackgroundService> _logger;
        private IModel _channel;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ImageWatermarkProsessBackgroundService(RabbitMQClientService rabbitMQClientService, ILogger<ImageWatermarkProsessBackgroundService> logger)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            _rabbitMQClientService = rabbitMQClientService;
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _rabbitMQClientService.Connect();
            //kaçar kaçar alacağımızı belirliyoruz
            _channel.BasicQos(0, 1, false);

            return base.StartAsync(cancellationToken);
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            _channel.BasicConsume(RabbitMQClientService.QueueName, false, consumer);

            consumer.Received += Consumer_Received;

            return Task.CompletedTask;

            //throw new NotImplementedException();
        }

        private Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {

            try
            {
                var productImageCreatedEvent = JsonSerializer.Deserialize<productImageCreatedEvent>
                (Encoding.UTF8.GetString(@event.Body.ToArray()));


                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images", productImageCreatedEvent.ImageName);


                var siteName = "batuhanbskr.com";

                using Image? img = Image.FromFile(path);


                using Graphics? graphic = Graphics.FromImage(img);

                Font? font = new Font(FontFamily.GenericMonospace, 32, FontStyle.Bold, GraphicsUnit.Pixel);

                SizeF textSize = graphic.MeasureString(siteName, font);

                var color = Color.FromArgb(128, 255, 255, 255);
                SolidBrush? brush = new SolidBrush(color);

                Point position = new Point(100,100);

                //@event.Graphics.DrawImage(siteName, font, brush, position);
                graphic.DrawString(siteName, font, brush, position);


                img.Save("wwwroot/Images/watermarks/" + productImageCreatedEvent.ImageName);



                img.Dispose(); //bellekte yer kaplamaması için
                graphic.Dispose();

                _channel.BasicAck(@event.DeliveryTag, false);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.Message);
            }
            
            return Task.CompletedTask;
            throw new NotImplementedException();
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}
        
       
