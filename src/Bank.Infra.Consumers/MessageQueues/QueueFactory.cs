using Bank.Domain.Models.MQ;
using RabbitMQ.Client;
using System.Collections.Generic;

namespace Bank.Infra.Consumers.MessageQueues
{
    public class QueueFactory
    {
        private MQSettings _configuration;
        private ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _model;

        public QueueFactory(MQSettings settings)
        {
            _configuration = settings;
            _connectionFactory = new ConnectionFactory
            {
                HostName = _configuration.MQHostName,
                UserName = _configuration.MQUserName,
                Password = _configuration.MQPassword
            };
            _connection = _connectionFactory.CreateConnection();
            _model = _connection.CreateModel();
        }

        public IModel CreateTransactionQueue()
        {
            CreateDeadLetterQueue();
            _model.QueueDeclare(_configuration.TransactionQueue, true, false, false, new Dictionary<string, object> { { "x-dead-letter-exchange", _configuration.DeadLetterExchange } });
            _model.ExchangeDeclare(_configuration.Exchange,
                ExchangeType.Direct,
                true,
                arguments: new Dictionary<string, object> { { "x-dead-letter-exchange", _configuration.DeadLetterExchange } });
            _model.QueueBind(_configuration.TransactionQueue, _configuration.Exchange, _configuration.TransactionQueue);
            return _model;
        }

        private IModel CreateDeadLetterQueue()
        {
            _model.QueueDeclare(_configuration.DeadLetterQueue, true, false, false, null);
            _model.ExchangeDeclare(_configuration.DeadLetterExchange, ExchangeType.Fanout, true);
            _model.QueueBind(_configuration.DeadLetterQueue, _configuration.DeadLetterExchange, _configuration.DeadLetterQueue);
            return _model;
        }


    }
}
