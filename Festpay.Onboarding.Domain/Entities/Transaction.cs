using Festpay.Onboarding.Domain.Exceptions;

namespace Festpay.Onboarding.Domain.Entities;

public class Transaction : EntityBase
{
    public Guid SourceAccountId { get; set; }
    public Guid DestinationAccountId { get; set; }
    public decimal Amount { get; set; }
    public bool Canceled { get; set; }

    public Account SourceAccount { get; set; } = default!;
    public Account DestinationAccount { get; set; } = default!;

    public override void Validate()
    {
        if (SourceAccountId == Guid.Empty)
            throw new RequiredFieldException(nameof(SourceAccountId));

        if (DestinationAccountId == Guid.Empty)
            throw new RequiredFieldException(nameof(DestinationAccountId));

        if (SourceAccountId == DestinationAccountId)
            throw new ArgumentException("Source and destination accounts must be different.");

        if (Amount <= 0)
            throw new ArgumentException("Transaction amount must be greater than zero.");
    }

    public class Builder
    {
        private readonly Transaction _transaction = new();

        public Builder WithSourceAccount(Guid sourceAccountId)
        {
            _transaction.SourceAccountId = sourceAccountId;
            return this;
        }

        public Builder WithDestinationAccount(Guid destinationAccountId)
        {
            _transaction.DestinationAccountId = destinationAccountId;
            return this;
        }

        public Builder WithAmount(decimal amount)
        {
            _transaction.Amount = amount;
            return this;
        }

        public Builder WithCanceled(bool canceled)
        {
            _transaction.Canceled = canceled;
            return this;
        }

        public Builder WithSourceAccountNavigation(Account source)
        {
            _transaction.SourceAccount = source;
            return this;
        }

        public Builder WithDestinationAccountNavigation(Account destination)
        {
            _transaction.DestinationAccount = destination;
            return this;
        }

        public Transaction Build()
        {
            _transaction.Validate();
            return _transaction;
        }
    }

    public void Cancel()
    {
        if (Canceled)
            throw new ArgumentException("Transaction already cancelled");

        Canceled = true;
    }
}
