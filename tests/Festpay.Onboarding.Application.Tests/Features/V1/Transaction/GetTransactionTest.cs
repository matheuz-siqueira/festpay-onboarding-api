using Festpay.Onboarding.Application.Common.Exceptions;
using Festpay.Onboarding.Application.Features.V1.Transaction;
using Festpay.Onboarding.Domain.Entities;
using Festpay.Onboarding.Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Festpay.Onboarding.Application.Tests.Features.V1.Transaction;

public class GetTransactionByIdHandlerTest
{
    private readonly DbContextOptions<FestpayContext> _dbOptions;

    public GetTransactionByIdHandlerTest()
    {
        _dbOptions = new DbContextOptionsBuilder<FestpayContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task Should_Return_Transaction_When_Exists()
    {
        var amount = 500m;
        using var context = new FestpayContext(_dbOptions);

        var source = CreateAccount("1");
        var destination = CreateAccount("2");

        context.Accounts.AddRange(source, destination);
        await context.SaveChangesAsync();

        var transaction = CreateTransaction(source.Id, destination.Id, amount);

        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        var handler = new GetTransactionByIdHandler(context);
        var query = new GetTransactionByIdQuery(transaction.Id);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(transaction.Id, result.Id);
        Assert.Equal(amount, result.Amount);
        Assert.False(result.Canceled);
        Assert.Equal(source.Id, result.SourceAccount.Id);
        Assert.Equal(destination.Id, result.DestinationAccount.Id);

    }

    [Fact]
    public async Task Should_Throw_NotFoundException_When_Transaction_Not_Exists()
    {
        using var context = new FestpayContext(_dbOptions);
        var handler = new GetTransactionByIdHandler(context);
        var query = new GetTransactionByIdQuery(Guid.NewGuid());

        await Assert.ThrowsAnyAsync<NotFoundException>(() =>
            handler.Handle(query, CancellationToken.None)
        );
    }

    private static Account CreateAccount(string nameSuffix)
    {
        return new Account.Builder()
            .WithName($"Account {nameSuffix}")
            .WithEmail($"test{nameSuffix}@example.com")
            .WithPhone("38998670120")
            .WithDocument($"99822293038")
            .Build();
    }

    private static Domain.Entities.Transaction CreateTransaction(Guid source, Guid destination, decimal amount)
    {
        return new Domain.Entities.Transaction.Builder()
            .WithAmount(amount)
            .WithSourceAccount(source)
            .WithDestinationAccount(destination)
            .Build();
    }
}
