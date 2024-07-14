namespace CurrencyConverter.Models;

public record CurrencyExchangeRateCacheDto
{
    public decimal UnitRate { get; set; }
    public string FromCurrency { get; set; }
    public string ToCurrency { get; set; }
}
