namespace CurrencyConverter.Models;

public record CurrencyExchangeRateRequestDto
{
    public decimal Amount { get; set; }
    public string From { get; set; }
    public string To { get; set; }
}
