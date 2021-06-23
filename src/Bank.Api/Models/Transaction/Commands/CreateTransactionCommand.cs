using System.ComponentModel.DataAnnotations;

namespace Bank.Api.Models.Transaction.Commands
{
    public class CreateTransactionCommand
    {
        [Required(ErrorMessage = "The {0} must be supplied")]
        public string AccountOrigin { get; set; }

        [Required(ErrorMessage = "The {0} must be supplied")]
        public string AccountDestination { get; set; }

        [Required(ErrorMessage = "The {0} must be supplied")]
        public decimal Value { get; set; }

    }
}
