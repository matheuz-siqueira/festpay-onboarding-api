
using Festpay.Onboarding.Application.Features.V1.Account;
using Festpay.Onboarding.Application.Features.V1.Transaction;
using Festpay.Onboarding.Domain.Entities;
using Festpay.Onboarding.Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Festpay.Onboarding.Application.Tests.Features.V1.Transaction;

public class CancelTransactionCommandHandlerTest
{
    private readonly DbContextOptions<FestpayContext> _dbOptions;

    public CancelTransactionCommandHandlerTest()
    {
        _dbOptions = new DbContextOptionsBuilder<FestpayContext>()
           .UseInMemoryDatabase(Guid.NewGuid().ToString())
           .Options;
    }

    [Fact]
    public async Task Should_Cancel_Transaction_Successfully()
    {
        using var context = new FestpayContext(_dbOptions);
        var source = CreateAccount("1");
        var destination = CreateAccount("2");

        context.Accounts.AddRange(source, destination);
        await context.SaveChangesAsync();

        var handlerUpdateBalance = new UpdateAccountBalanceCommandHandler(context);
        var commandAccount = new UpdateAccountBalanceCommand(source.Id, 500m);
        var resultUpdateBalance = await handlerUpdateBalance.Handle(commandAccount, CancellationToken.None);

        var transaction = CreateTransaction(source.Id, destination.Id, 50m);
        context.Transactions.Add(transaction);

        await context.SaveChangesAsync();

        var handlerCancelTransaction = new CancelTransactionCommandHandler(context);
        var commandCancel = new CancelTransactionCommand(transaction.Id);
        var result = await handlerCancelTransaction.Handle(commandCancel, CancellationToken.None);

        Assert.True(result);
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
