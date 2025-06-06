using FluentValidation;
using MediatR;
using Festpay.Onboarding.Infra.Context;
using Festpay.Onboarding.Application.Common.Exceptions;
using Carter;
using Microsoft.AspNetCore.Routing;
using Festpay.Onboarding.Application.Common.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Festpay.Onboarding.Application.Features.V1;

public sealed record ChangeAccountStatusCommand(Guid Id) : IRequest<bool>;

public sealed class ChangeAccountStatusCommandValidator
    : AbstractValidator<ChangeAccountStatusCommand>
{
    public ChangeAccountStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}

public sealed class ChangeAccountStatusCommandHandler(FestpayContext dbContext)
    : IRequestHandler<ChangeAccountStatusCommand, bool>
{
    public async Task<bool> Handle(
        ChangeAccountStatusCommand request,
        CancellationToken cancellationToken
    )
    {
        var account = await dbContext.Accounts.FindAsync(request.Id) ??
            throw new NotFoundException("Account");

        account.EnableDisable();

        return await dbContext.SaveChangesAsync(cancellationToken) > 0;
    }
}

public sealed class ChangeAccountStatusCommandEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch($"{EndpointConstants.V1}{EndpointConstants.Account}/{{id:guid}}",
            async ([FromServices] ISender sender, [FromRoute] Guid id) =>
            {

                var result = await sender.Send(new ChangeAccountStatusCommand(id));
                if (!result)
                    Results.BadRequest("Error updating account data");

                return Results.NoContent();
            }
        )
        .WithTags(SwaggerTagsConstants.Account);
    }
}
