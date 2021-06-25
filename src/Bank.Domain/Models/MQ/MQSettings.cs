namespace Bank.Domain.Models.MQ
{
    public class MQSettings
    {
        public string MQHostName { get; set; }
        public string MQUserName { get; set; }
        public string MQPassword { get; set; }
        public string TransactionQueue { get; set; }
        public string Exchange { get; set; }
        public APIContaSettings APIContaSettings { get; set; }
        public string DeadLetterQueue { get; set; }
        public string DeadLetterExchange { get; set; }
        public int RetryAttempts { get; set; }
    }
}
