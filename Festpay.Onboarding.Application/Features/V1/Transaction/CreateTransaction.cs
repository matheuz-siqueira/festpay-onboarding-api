using Carter;
using Festpay.Onboarding.Application.Common.Constants;
using Festpay.Onboarding.Application.Common.Exceptions;
using Festpay.Onboarding.Application.Common.Models;
using Festpay.Onboarding.Domain.Entities;
using Festpay.Onboarding.Infra.Context;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Festpay.Onboarding.Application.Features.V1.Transaction;

public sealed record CreateTransactionCommand(
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount
) : IRequest<bool>;

public sealed class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(t => t.DestinationAccountId).NotEmpty().WithMessage("Source account is required.");

        RuleFor(t => t.DestinationAccountId)
            .NotEmpty().WithMessage("Destination account is required.")
            .NotEqual(t => t.SourceAccountId).WithMessage("Source and destination account must be different");

        RuleFor(t => t.Amount)
            .GreaterThan(0).WithMessage("Transaction amount must be greater than zero.");
    }
}

public sealed class CreateTransactionCommandHandler(FestpayContext dbContext) : IRequestHandler<CreateTransactionCommand, bool>
{
    public async Task<bool> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var source = await GetAccount(request.SourceAccountId);
        if (source is null)
            throw new NotFoundException("Conta");

        source.EnsureSufficientBalance(request.Amount);

        var destination = await GetAccount(request.DestinationAccountId);
        if (destination is null)
            throw new NotFoundException("Conta");

        if (destination.DeactivatedUtc is not null)
            throw new AccountDeactivatedException("Conta");

        source.Debit(request.Amount);
        destination.Credit(request.Amount);

        var transaction = new Domain.Entities.Transaction.Builder()
            .WithSourceAccount(request.SourceAccountId)
            .WithDestinationAccount(request.DestinationAccountId)
            .WithAmount(request.Amount)
            .Build();

        await dbContext.Transactions.AddAsync(transaction, cancellationToken);
        dbContext.Accounts.Update(source);
        dbContext.Accounts.Update(destination);

        return await dbContext.SaveChangesAsync(cancellationToken) > 0;
    }

    private async Task<Domain.Entities.Account?> GetAccount(Guid id)
    {
        return await dbContext.Accounts.FirstOrDefaultAsync(x => x.Id == id);
    }
}

public sealed class CreateTransactionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost($"{EndpointConstants.V1}{EndpointConstants.Transaction}",
            async ([FromServices] ISender sender, [FromBody] CreateTransactionCommand command) =>
            {
                var result = await sender.Send(command);
                if (!result)
                    return Result.BadRequest("Failed to register transaction.");
                return Result.Created(result, "created successfully");
            }
        )
        .WithTags(SwaggerTagsConstants.Transaction);
    }
}
