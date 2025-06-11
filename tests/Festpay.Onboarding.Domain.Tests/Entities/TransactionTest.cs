using Festpay.Onboarding.Domain.Entities;
using Festpay.Onboarding.Domain.Exceptions;


namespace Festpay.Onboarding.Domain.Tests.Entities;

public class TransactionTest
{
    private readonly Guid _sourceAccountId = Guid.NewGuid();
    private readonly Guid _destinationAccountId = Guid.NewGuid();

    [Fact]
    public void Shoud_Create_Transaction_When_Data_Is_Valid()
    {
        var transaction = new Transaction.Builder()
            .WithSourceAccount(_sourceAccountId)
            .WithDestinationAccount(_destinationAccountId)
            .WithAmount(100m)
            .Build();

        Assert.Equal(_sourceAccountId, transaction.SourceAccountId);
        Assert.Equal(_destinationAccountId, transaction.DestinationAccountId);
        Assert.Equal(100m, transaction.Amount);
        Assert.False(transaction.Canceled);
    }

    [Fact]
    public void Should_Throw_RequiredFieldException_When_SourceAccountId_Is_Empty()
    {
        var exception = Assert.Throws<RequiredFieldException>(() =>
        {
            new Transaction.Builder()
                .WithSourceAccount(Guid.Empty)
                .WithDestinationAccount(_destinationAccountId)
                .WithAmount(100m)
                .Build();
        });

        Assert.Equal(nameof(Transaction.SourceAccountId), exception.FieldName);
    }

    [Fact]
    public void Should_Throw_RequiredFieldException_When_DestinationAccountId_Is_Empty()
    {
        var exception = Assert.Throws<RequiredFieldException>(() =>
        {
            new Transaction.Builder()
                .WithSourceAccount(_sourceAccountId)
                .WithDestinationAccount(Guid.Empty)
                .WithAmount(100m)
                .Build();
        });

        Assert.Equal(nameof(Transaction.DestinationAccountId), exception.FieldName);
    }

    [Fact]
    public void Should_Throw_EqualAccountException_When_Source_And_Destination_Are_The_Same()
    {
        var id = Guid.NewGuid();

        var exception = Assert.Throws<EqualAccountsException>(() =>
        {
            new Transaction.Builder()
                .WithSourceAccount(id)
                .WithDestinationAccount(id)
                .WithAmount(100m)
                .Build();
        });

        Assert.Equal(nameof(Transaction.DestinationAccountId), exception.FieldName);
    }

    [Fact]
    public void Should_Throw_InvalidAmountException_When_Amount_Is_Less_Than_Or_Equal_To_Zero()
    {
        var invalidValue = 0;
        var exception = Assert.Throws<InvalidAmountException>(() =>
            new Transaction.Builder()
                .WithSourceAccount(Guid.NewGuid())
                .WithDestinationAccount(Guid.NewGuid())
                .WithAmount(invalidValue)
                .Build()
        );

        Assert.Equal(invalidValue, exception.Amount);
    }

    [Fact]
    public void Should_Throw_TransactionAlreadyCanceledException_When_Trying_To_Cancel_Twice()
    {
        var transaction = new Transaction.Builder()
            .WithSourceAccount(_sourceAccountId)
            .WithDestinationAccount(_destinationAccountId)
            .WithAmount(100m)
            .Build();

        transaction.Cancel();

        var exception = Assert.Throws<TransactionAlreadyCanceledException>(() => transaction.Cancel());
        Assert.Equal(transaction.Id, exception.TransactionId);
    }
}
