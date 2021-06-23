namespace Bank.Domain.Apps.MessageQueues
{
    public interface ITransactionSendQueue
    {
        void SendQueue(string message);
    }
}
