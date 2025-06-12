using System.Net.Security;
using Festpay.Onboarding.Application.Features.V1.Transaction;
using Festpay.Onboarding.Domain.Entities;
using Festpay.Onboarding.Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Festpay.Onboarding.Application.Tests.Features.V1.Transaction;

public class GetTransactionsQueryHandlerTest
{
    private readonly DbContextOptions<FestpayContext> _dbOptions;
    public GetTransactionsQueryHandlerTest()
    {
        _dbOptions = new DbContextOptionsBuilder<FestpayContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task Should_Returns_All_Transactions()
    {
        using var context = new FestpayContext(_dbOptions);

        var source = CreateAccount("1");
        var destination = CreateAccount("2");
        context.Accounts.AddRange(source, destination);
        await context.SaveChangesAsync();

        var transaction1 = CreateTransaction(source.Id, destination.Id, 300m);
        var transaction2 = CreateTransaction(destination.Id, source.Id, 200m);

        context.Transactions.AddRange(transaction1, transaction2);
        await context.SaveChangesAsync();

        var handler = new GetTransactionsQueryHandler(context);
        var query = new GetTransactionsQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        var t1 = result.First(x => x.Id == transaction1.Id);
        var t2 = result.First(x => x.Id == transaction2.Id);

        Assert.Equal(300m, t1.Amount);
        Assert.Equal(200m, t2.Amount);
    }

    [Fact]
    public async Task Should_Returns_Empty_When_No_Transactions()
    {
        using var context = new FestpayContext(_dbOptions);
        var handler = new GetTransactionsQueryHandler(context);
        var query = new GetTransactionsQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
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
