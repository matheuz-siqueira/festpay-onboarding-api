using Carter;
using Festpay.Onboarding.Application.Common.Constants;
using Festpay.Onboarding.Application.Common.Exceptions;
using Festpay.Onboarding.Application.Common.Models;
using Festpay.Onboarding.Domain.Entities;
using Festpay.Onboarding.Infra.Context;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Festpay.Onboarding.Application.Features.V1.Transaction;

public sealed record AccountSummary(
    Guid Id,
    string Name,
    string Document,
    string Email,
    string Phone
);

public sealed record GetTransactionQueryResponse(
    Guid Id,
    decimal Amount,
    bool Canceled,
    DateTime CreatedAt,
    AccountSummary SourceAccount,
    AccountSummary DestinationAccount
);

public sealed record GetTransactionByIdQuery(Guid Id) : IRequest<GetTransactionQueryResponse>;

public sealed class GetTransactionByIdHandler(FestpayContext context) :
    IRequestHandler<GetTransactionByIdQuery, GetTransactionQueryResponse>
{
    public async Task<GetTransactionQueryResponse> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        var transaction = await GetTransactionById(request.Id, cancellationToken);
        if (transaction is null)
            throw new NotFoundException("Transaction");

        var source = await GetAccountById(transaction.SourceAccountId, cancellationToken);
        var destination = await GetAccountById(transaction.DestinationAccountId, cancellationToken);

        return new GetTransactionQueryResponse(
            transaction.Id,
            transaction.Amount,
            transaction.Canceled,
            transaction.CreatedUtc,
            new AccountSummary(
                source.Id,
                source.Name,
                source.Document,
                source.Email,
                source.Phone
            ),
            new AccountSummary(
                destination.Id,
                destination.Name,
                destination.Document,
                destination.Email,
                destination.Phone
            )
        );
    }

    private async Task<Domain.Entities.Transaction?> GetTransactionById(Guid transactionId, CancellationToken cancellationToken)
    {
        var transaction = await context.Transactions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == transactionId);
        return transaction;
    }

    private async Task<Domain.Entities.Account?> GetAccountById(Guid accountId, CancellationToken cancellationToken)
    {
        var account = await context.Accounts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == accountId);
        return account!;
    }
}

public sealed class GetAccountsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet($"{EndpointConstants.V1}{EndpointConstants.Transaction}/{{id:guid}}",
            async ([FromServices] ISender sender, [FromRoute] Guid id) =>
            {
                var result = await sender.Send(new GetTransactionByIdQuery(id));
                return Result.Ok(result);
            }
        )
        .WithTags(SwaggerTagsConstants.Transaction);
    }
}
