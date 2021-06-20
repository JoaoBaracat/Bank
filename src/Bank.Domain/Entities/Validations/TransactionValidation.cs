using FluentValidation;

namespace Bank.Domain.Entities.Validations
{
    public class TransactionValidation : AbstractValidator<Transaction>
    {
        public TransactionValidation()
        {
            RuleFor(x => x.AccountOrigin)
                .NotEmpty().WithMessage("The {PropertyName} must be supplied");

            RuleFor(x => x.AccountDestination)
                .NotEmpty().WithMessage("The {PropertyName} must be supplied");

            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("The {PropertyName} must be supplied")
                .GreaterThan(0).WithMessage("The {PropertyName} must be greater than 0");
        }
    }
}
