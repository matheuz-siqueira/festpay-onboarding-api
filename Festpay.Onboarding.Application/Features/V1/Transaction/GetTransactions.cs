using Carter;
using Festpay.Onboarding.Application.Common.Constants;
using Festpay.Onboarding.Application.Common.Models;
using Festpay.Onboarding.Infra.Context;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Festpay.Onboarding.Application.Features.V1.Transaction;

public sealed record GetTransactionsQueryResponse(
    Guid Id,
    DateTime CreatedUtc,
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount
);

public sealed record GetTransactionsQuery : IRequest<ICollection<GetTransactionsQueryResponse>>;

public sealed class GetTransactionsQueryHandler(FestpayContext context) :
    IRequestHandler<GetTransactionsQuery, ICollection<GetTransactionsQueryResponse>>
{
    public async Task<ICollection<GetTransactionsQueryResponse>> Handle(
        GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var transactions = await context.Transactions.AsNoTracking().ToListAsync(cancellationToken);
        return transactions.Select(t => new GetTransactionsQueryResponse(
            t.Id,
            t.CreatedUtc,
            t.SourceAccountId,
            t.DestinationAccountId,
            t.Amount
        )).ToList();
    }
}

public sealed class GetTransactionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet($"{EndpointConstants.V1}{EndpointConstants.Transaction}", async ([FromServices] ISender sender) =>
        {
            var result = await sender.Send(new GetTransactionsQuery());
            return Result.Ok(result);
        })
        .WithTags(SwaggerTagsConstants.Transaction);
    }
}
