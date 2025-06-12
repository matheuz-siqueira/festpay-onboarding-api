using Festpay.Onboarding.Application.Common.Exceptions;
using Festpay.Onboarding.Application.Features.V1;
using Festpay.Onboarding.Application.Features.V1.Account;
using Festpay.Onboarding.Application.Features.V1.Transaction;
using Festpay.Onboarding.Domain.Entities;
using Festpay.Onboarding.Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Festpay.Onboarding.Application.Tests.Features.V1.Transaction;

public class CreateTransactionCommandHandlerTest
{
    private readonly DbContextOptions<FestpayContext> _dbOptions;

    public CreateTransactionCommandHandlerTest()
    {
        _dbOptions = new DbContextOptionsBuilder<FestpayContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task Should_Create_Transaction_Successfully()
    {
        using var context = new FestpayContext(_dbOptions);

        var source = CreateAccount("1");
        var destination = CreateAccount("2");

        context.Accounts.AddRange(source, destination);
        await context.SaveChangesAsync();

        var handlerUpdateBalance = new UpdateAccountBalanceCommandHandler(context);
        var commandAccount = new UpdateAccountBalanceCommand(source.Id, 500m);
        var resultUpdateBalance = await handlerUpdateBalance.Handle(commandAccount, CancellationToken.None);

        var handlerCreateTransaction = new CreateTransactionCommandHandler(context);
        var commandTransaction = new CreateTransactionCommand(source.Id, destination.Id, 500m);
        var resultTransaction = await handlerCreateTransaction.Handle(commandTransaction, CancellationToken.None);

        Assert.True(resultTransaction);

        var transaction = await context.Transactions.FirstOrDefaultAsync();

        Assert.NotNull(transaction);
        Assert.Equal(500m, transaction.Amount);

        var updatedSource = await context.Accounts.FindAsync(source.Id);
        var updatedDest = await context.Accounts.FindAsync(destination.Id);
        Assert.Equal(0m, updatedSource?.Balance);
        Assert.Equal(500m, updatedDest?.Balance); ;
    }
    [Fact]
    public async Task Should_Throw_NotFound_When_Source_Account_Not_Exists()
    {
        using var context = new FestpayContext(_dbOptions);
        var destination = CreateAccount("2");
        context.Accounts.Add(destination);
        await context.SaveChangesAsync();

        var handler = new CreateTransactionCommandHandler(context);
        var command = new CreateTransactionCommand(Guid.NewGuid(), destination.Id, 50m);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));

    }

    [Fact]
    public async Task Should_Throw_Exception_When_Destination_Is_Deactived()
    {
        using var context = new FestpayContext(_dbOptions);
        var source = CreateAccount("1");
        var destination = CreateAccount("2");
        context.Accounts.AddRange(source, destination);
        await context.SaveChangesAsync();

        var handlerUpdateBalance = new UpdateAccountBalanceCommandHandler(context);
        var commandAccount = new UpdateAccountBalanceCommand(source.Id, 500m);
        var resultUpdateBalance = await handlerUpdateBalance.Handle(commandAccount, CancellationToken.None);

        var handlerUpdateAccount = new ChangeAccountStatusCommandHandler(context);
        var commandUpdateAccount = new ChangeAccountStatusCommand(destination.Id);
        var resultUpdateAccount = await handlerUpdateAccount.Handle(commandUpdateAccount, CancellationToken.None);

        var handlerCreateTransaction = new CreateTransactionCommandHandler(context);
        var commandTransaction = new CreateTransactionCommand(source.Id, destination.Id, 50m);

        await Assert.ThrowsAsync<AccountDeactivatedException>(
            () => handlerCreateTransaction.Handle(commandTransaction, CancellationToken.None));

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
