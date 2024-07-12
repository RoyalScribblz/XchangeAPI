using FluentValidation;
using XchangeAPI.Services.CurrencyService;
using XchangeAPI.Services.UserService;

namespace XchangeAPI.Endpoints.Validation;

public sealed class CreateAccountValidator : AbstractValidator<CreateAccountRequest>
{
    public CreateAccountValidator(IUserService userService, ICurrencyService currencyService)
    {
        RuleFor(x => x.UserId).CustomAsync(async (userId, context, cancellationToken) =>
        {
            var user = await userService.GetUser(userId, cancellationToken);
            
            if (user == null)
            {
                context.AddFailure("UserId does not correlate with a valid User.");
            }
        });

        RuleFor(x => x.CurrencyId).CustomAsync(async (currencyId, context, cancellationToken) =>
        {
            var currency = await currencyService.GetCurrency(currencyId, cancellationToken);

            if (currency == null)
            {
                context.AddFailure("CurrencyId does not correlate with a valid Currency.");
            }
        });
    }
}

public sealed record CreateAccountRequest(string UserId, Guid CurrencyId);