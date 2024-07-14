using CurrencyConverter.Models;
using CurrencyConverter.Models.Common;

namespace CurrencyConverter.WebAPI.Services;
public interface ICurrencyService
{
    Task<RequestResponseDto<CurrencyExchangeRateResponseDto>> GetLatestRates(string baseCurrency);
    Task<RequestResponseDto<CurrencyExchangeRateResponseDto>> ConvertCurrency(CurrencyExchangeRateRequestDto request);
    Task<PaginatedApiResponseDto<CurrencyExchangeRateHistoryResponseDto>> GetCurrencyHistory(CurrencyExchangeRateHistoryRequestDto request);
}
