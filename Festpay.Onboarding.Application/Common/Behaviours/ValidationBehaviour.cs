using FluentValidation;
using MediatR;

namespace Festpay.Onboarding.Application.Common.Behaviours;

public class ValidationBehaviour<TRequest, TUnit>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TUnit>
    where TRequest : IRequest<TUnit>
{
    public Task<TUnit> Handle(
        TRequest request,
        RequestHandlerDelegate<TUnit> next,
        CancellationToken cancellationToken
    )
    {
        var failures = validators
            .Select(v => v.Validate(request))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count == 0)
            return next();

        throw new Exceptions.ValidationException(failures);
    }
}
