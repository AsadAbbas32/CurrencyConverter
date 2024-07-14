using FluentValidation;

namespace CurrencyConverter.Models.Dto.Validators;
public class CurrencyExchangeRateRequestDtoValidator : AbstractValidator<CurrencyExchangeRateRequestDto>
{
    public CurrencyExchangeRateRequestDtoValidator()
    {
        RuleFor(request => request.Amount)
            .InclusiveBetween(0.01m, decimal.MaxValue) // Ensure a positive amount
            .WithMessage("Amount must be a positive value greater than zero.");

        RuleFor(request => request.From)
            .NotEmpty()
            .WithMessage("From currency code is required.");

        RuleFor(request => request.To)
            .NotEmpty()
            .WithMessage("To currency code is required.");
    }
}
