using Carter;
using Festpay.Onboarding.Application.Common.Constants;
using Festpay.Onboarding.Application.Common.Exceptions;
using Festpay.Onboarding.Application.Common.Models;
using Festpay.Onboarding.Domain.Entities;
using Festpay.Onboarding.Domain.Exceptions;
using Festpay.Onboarding.Infra.Context;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Festpay.Onboarding.Application.Features.V1.Transaction;

public sealed record CancelTransactionCommand(Guid Id) : IRequest<bool>;

public sealed class CancelTransactionCommandValidator
    : AbstractValidator<CancelTransactionCommand>
{
    public CancelTransactionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}

public sealed class CancelTransactionCommandHandler(FestpayContext dbContext) :
    IRequestHandler<CancelTransactionCommand, bool>
{
    public async Task<bool> Handle(CancelTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await GetTransaction(request.Id);
        if (transaction is null)
            throw new NotFoundException("Transaction");

        if (transaction.Canceled)
            throw new AlreadyCanceledException("Transaction");

        var source = await GetAccount(transaction.SourceAccountId);
        var destination = await GetAccount(transaction.DestinationAccountId);

        if (destination!.Balance < transaction.Amount)
            throw new InsufficientBalanceException();

        destination.Debit(transaction.Amount);
        source!.Credit(transaction.Amount);

        transaction.Cancel();

        dbContext.Accounts.Update(source);
        dbContext.Accounts.Update(destination);
        dbContext.Transactions.Update(transaction);

        return await dbContext.SaveChangesAsync(cancellationToken) > 0;
    }

    private async Task<Domain.Entities.Transaction?> GetTransaction(Guid id)
    {
        var transaction = await dbContext.Transactions.FirstOrDefaultAsync(x => x.Id == id);
        return transaction;

    }

    private async Task<Domain.Entities.Account?> GetAccount(Guid accountId)
    {
        var account = await dbContext.Accounts.FirstOrDefaultAsync(x => x.Id == accountId);
        return account;
    }
}


public sealed class CancelTransactionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut($"{EndpointConstants.V1}{EndpointConstants.Transaction}/{{id:guid}}/cancel",
            async ([FromServices] ISender sender, [FromRoute] Guid id) =>
            {
                var result = await sender.Send(new CancelTransactionCommand(id));
                if (!result)
                    return Result.BadRequest("Error canceling transaction.");

                return Results.Ok(Result.Ok("Transaction cancelled successfully"));
            }
        )
        .WithTags(SwaggerTagsConstants.Transaction);
    }
}
