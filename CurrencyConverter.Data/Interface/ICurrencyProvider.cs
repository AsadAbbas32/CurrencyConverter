using CurrencyConverter.Models;

namespace CurrencyConverter.Data.Interface
{
    public interface ICurrencyProvider
    {
        Task<CurrencyExchangeRateResponseDto> GetLatestRates(string baseCurrency);
        Task<CurrencyExchangeRateResponseDto> ConvertCurrency(CurrencyExchangeRateRequestDto request);
        Task<CurrencyExchangeRateHistoryResponseDto> GetCurrencyHistory(CurrencyExchangeRateHistoryRequestDto request);
    }
}
