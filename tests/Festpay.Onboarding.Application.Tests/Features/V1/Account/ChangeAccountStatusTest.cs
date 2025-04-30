using Festpay.Onboarding.Application.Common.Exceptions;
using Festpay.Onboarding.Application.Features.V1;
using Festpay.Onboarding.Domain.Entities;
using Festpay.Onboarding.Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Festpay.Onboarding.Application.Tests.Features.V1;

public class ChangeAccountStatusCommandHandlerTests
{
    private readonly DbContextOptions<FestpayContext> _dbOptions;

    public ChangeAccountStatusCommandHandlerTests()
    {
        _dbOptions = new DbContextOptionsBuilder<FestpayContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    private static Account CreateTestAccount()
    {
        return new Account.Builder()
            .WithName("Test Account")
            .WithEmail("test@example.com")
            .WithPhone("11999999999")
            .WithDocument("12345678909")
            .Build();
    }

    [Fact]
    public async Task Should_Toggle_DisabledAt_When_Account_Exists()
    {
        // Arrange
        var account = CreateTestAccount();

        using var context = new FestpayContext(_dbOptions);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var command = new ChangeAccountStatusCommand(account.Id);
        var handler = new ChangeAccountStatusCommandHandler(context);

        // Act - First toggle: disable the account
        var resultDisable = await handler.Handle(command, CancellationToken.None);
        var updatedAccountDisable = await context.Accounts.FindAsync(account.Id);

        // Assert disable
        Assert.True(resultDisable);
        Assert.NotNull(updatedAccountDisable!.DeactivatedUtc);

        // Act - Second toggle: enable the account
        var resultEnable = await handler.Handle(command, CancellationToken.None);
        var updatedAccountEnable = await context.Accounts.FindAsync(account.Id);

        // Assert enable
        Assert.True(resultEnable);
        Assert.Null(updatedAccountEnable!.DeactivatedUtc);
    }

    [Fact]
    public async Task Should_Throw_NotFoundException_When_Account_Not_Exists()
    {
        using var context = new FestpayContext(_dbOptions);
        var command = new ChangeAccountStatusCommand(Guid.NewGuid());
        var handler = new ChangeAccountStatusCommandHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None)
        );
    }
}
