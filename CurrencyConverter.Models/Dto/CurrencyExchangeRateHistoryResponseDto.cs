using Newtonsoft.Json;
namespace CurrencyConverter.Models;
public record CurrencyExchangeRateHistoryResponseDto
{
    public decimal Amount { get; set; }
    public string Base { get; set; }

    [JsonProperty("start_date")]
    public DateTime StartDate { get; set; }

    [JsonProperty("end_date")]
    public DateTime EndDate { get; set; }
    public Dictionary<string, Dictionary<string, decimal>> Rates { get; set; }
}
