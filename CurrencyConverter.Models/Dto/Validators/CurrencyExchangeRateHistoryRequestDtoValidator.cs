using FluentValidation;

namespace CurrencyConverter.Models.Dto.Validators;
public class CurrencyExchangeRateHistoryRequestDtoValidator : AbstractValidator<CurrencyExchangeRateHistoryRequestDto>
{
    public CurrencyExchangeRateHistoryRequestDtoValidator()
    {
        RuleFor(request => request.BaseCurrency)
            .NotEmpty()
            .WithMessage("Base currency is required.");

        RuleFor(request => request.StartDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Start date cannot be in the future.");

        RuleFor(request => request.EndDate)
            .GreaterThanOrEqualTo(request => request.StartDate)
            .WithMessage("End date must be after or equal to start date.");

        RuleFor(request => request.Take)
            .InclusiveBetween(1, 100)
            .WithMessage("Take value must be between 1 and 100.");

        RuleFor(request => request.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be greater than or equal to 1.");
    }
}
