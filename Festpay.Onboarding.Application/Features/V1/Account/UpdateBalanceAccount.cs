using Carter;
using Festpay.Onboarding.Application.Common.Constants;
using Festpay.Onboarding.Application.Common.Exceptions;
using Festpay.Onboarding.Application.Common.Models;
using Festpay.Onboarding.Infra.Context;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Festpay.Onboarding.Application.Features.V1.Account;

public sealed record UpdateAccountBalanceCommand(
    Guid Id,
    decimal Balance
) : IRequest<bool>;

public sealed class UpdateAccountBalanceCommandValidator : AbstractValidator<UpdateAccountBalanceCommand>
{
    public UpdateAccountBalanceCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
        RuleFor(x => x.Balance)
            .GreaterThanOrEqualTo(0).WithMessage("Balance must be zero or greater.");
    }
}

public sealed class UpdateAccountBalanceCommandHandler(FestpayContext context) :
    IRequestHandler<UpdateAccountBalanceCommand, bool>
{
    public async Task<bool> Handle(UpdateAccountBalanceCommand request, CancellationToken cancellationToken)
    {
        var account = await context.Accounts.FindAsync(request.Id);

        if (account is null)
            throw new NotFoundException("Transaction");

        account.UpdateBalance(request.Balance);

        context.Accounts.Update(account);
        return await context.SaveChangesAsync(cancellationToken) > 0;
    }
}

public sealed class UpdateAccountBalanceEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch($"{EndpointConstants.V1}{EndpointConstants.Account}/{{id:guid}}/balance",
            async ([FromServices] ISender sender, [FromRoute] Guid id, [FromBody] UpdateAccountBalanceCommand command) =>
            {

                var result = await sender.Send(command);
                if (!result)
                    return Result.BadRequest("Error updating balance");
                return Result.NoContent();

            }
        )
        .WithTags(SwaggerTagsConstants.Account);
    }
}
