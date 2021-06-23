using Bank.Domain.Models.MQ;
using RabbitMQ.Client;
using System.Text;

namespace Bank.Infra.Consumers.MessageQueues
{
    public class TansferSendQueue
    {
        private MQSettings _configuration;
        private IModel _model;

        public TansferSendQueue(MQSettings settings)
        {
            _configuration = settings;
        }

        public void SendQueue(string message)
        {
            _model = new QueueFactory(_configuration).CreateTransferQueue();
            //Setup properties
            var properties = _model.CreateBasicProperties();
            //Serialize
            byte[] messageBuffer = Encoding.Default.GetBytes(message);
            //Send message
            _model.BasicPublish("", _configuration.TranferQueue, properties, messageBuffer);
        }
    }
}
