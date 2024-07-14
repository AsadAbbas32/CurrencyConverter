namespace CurrencyConverter.Models;

public record CurrencyExchangeRateHistoryRequestDto
{
    public string BaseCurrency { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Take { get; set; }
    public int Page { get; set; }

}
