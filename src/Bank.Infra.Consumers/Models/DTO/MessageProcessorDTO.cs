using Bank.Domain.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Bank.Infra.Consumers.Models.DTO
{
    public class MessageProcessorDTO
    {
        public BasicDeliverEventArgs EventArgs { get; set; }
        public Transaction TransactionMessage { get; set; }
        public string ContentString { get; set; }
        public IModel Model { get; set; }
    }
}
