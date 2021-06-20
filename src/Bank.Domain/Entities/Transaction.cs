namespace Bank.Domain.Entities
{
    public class Transaction : Entity
    {
        public string AccountOrigin { get; set; }
        public string AccountDestination { get; set; }
        public decimal Value { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
    }
}
