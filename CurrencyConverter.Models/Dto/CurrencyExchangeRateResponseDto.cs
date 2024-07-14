namespace CurrencyConverter.Models;

public record CurrencyExchangeRateResponseDto
{
    public decimal Amount { get; set; }
    public string Base { get; set; }
    public Dictionary<string, decimal> Rates { get; set; }

    public CurrencyExchangeRateResponseDto()
    {
        Rates = new();
    }
}
