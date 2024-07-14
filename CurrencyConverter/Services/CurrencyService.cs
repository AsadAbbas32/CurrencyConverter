using AutoMapper;
using CurrencyConverter.Data.Interface;
using CurrencyConverter.Models;
using CurrencyConverter.Models.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
namespace CurrencyConverter.WebAPI.Services;

public class CurrencyService : ICurrencyService
{
    private readonly ICurrencyProvider _currencyProvider;
    private readonly IMemoryCache _cache;
    private readonly IMapper _mapper;

    public readonly List<string> ExcludedCurrencyCodes = new();


    public CurrencyService(ICurrencyProvider currencyProvider, IMemoryCache cache, IMapper mapper, IConfiguration configuration)
    {
        _currencyProvider = currencyProvider;
        _cache = cache;
        _mapper = mapper;

        var excludedcurrencies = configuration["ExcludedCurrencies"];
        if (excludedcurrencies != null)
            ExcludedCurrencyCodes = excludedcurrencies.Split(",").ToList();
    }

    public async Task<RequestResponseDto<CurrencyExchangeRateResponseDto>> ConvertCurrency(CurrencyExchangeRateRequestDto request)
    {

        var response = new RequestResponseDto<CurrencyExchangeRateResponseDto>();
        var cacheKey = $"exchange-rate-{request.From.ToLower()}-{request.To.ToLower()}";

        if (ExcludedCurrencyCodes.Any(cur => cur == request.To.ToLower() || cur == request.From.ToLower()))
        {
            response.Success = false;
            response.ErrorMessage = "supplied currency is not allowed for conversion";
            return response;
        }
        try
        {
            // Check if rates are already cached
            var cachedRates = _cache.Get<CurrencyExchangeRateCacheDto>(cacheKey);
            if (cachedRates != null)
            {
                response.Data = new()
                {

                    Amount = request.Amount,
                    Base = request.From,
                    Rates = new() { [cachedRates.ToCurrency] = cachedRates.UnitRate * request.Amount }
                };
                return response;
            }

            var exchangeRateData = await _currencyProvider.ConvertCurrency(request);

            var dto = _mapper.Map<CurrencyExchangeRateCacheDto>(exchangeRateData);
            // Cache the rates for a specific duration
            _cache.Set(cacheKey, dto);
            response.Data = exchangeRateData;
            return response;
        }
        catch (Exception ex)
        {
            // Handle HTTP request related errors (e.g., network issues)
            response.Success = false;
            response.ErrorMessage = ex.Message;
            return response;
        }
    }

    public async Task<RequestResponseDto<CurrencyExchangeRateResponseDto>> GetLatestRates(string baseCurrency)
    {
        var cacheKey = $"latest-rates-{baseCurrency.ToLower()}";
        var response = new RequestResponseDto<CurrencyExchangeRateResponseDto>();

        // Check if rates are already cached
        var cachedRates = _cache.Get<CurrencyExchangeRateResponseDto>(cacheKey);

        if (cachedRates != null)
        {
            response.Data = cachedRates;
            return response;
        }

        try
        {
            var exchangeRateData = await _currencyProvider.GetLatestRates(baseCurrency);

            // Cache the rates for a specific duration
            _cache.Set(cacheKey, exchangeRateData);
            response.Data = exchangeRateData;
            return response;
        }
        catch (Exception ex)
        {
            // Handle HTTP request related errors (e.g., network issues)
            response.Success = false;
            response.ErrorMessage = ex.Message;
            return response;
        }
    }
    public async Task<PaginatedApiResponseDto<CurrencyExchangeRateHistoryResponseDto>> GetCurrencyHistory(CurrencyExchangeRateHistoryRequestDto request)
    {
        var cacheKey = $"currency-history-{request.BaseCurrency.ToLower()}" +
            $"-{request.StartDate.ToString("yyyy-MM-dd")}-{request.EndDate.ToString("yyyy-MM-dd")}";
        var response = new PaginatedApiResponseDto<CurrencyExchangeRateHistoryResponseDto>();
        response.PageSize = request.Take;
        response.PageNo = request.Page;

        int skip = (request.Page - 1) * request.Take;
        int take = request.Take;
        // Check if rates are already cached
        var cachedRates = _cache.Get<CurrencyExchangeRateHistoryResponseDto>(cacheKey);

        if (cachedRates != null)
        {
            response.Data = cachedRates;
            response.TotalRecords = response.Data.Rates.Count();
            response.Data.Rates = response.Data.Rates.Skip(skip).Take(take).ToDictionary();
            return response;
        }


        try
        {
            var exchangeRateData = await _currencyProvider.GetCurrencyHistory(request);

            // Cache the rates for a specific duration
            _cache.Set(cacheKey, exchangeRateData);
            response.TotalRecords = exchangeRateData.Rates.Count();
            exchangeRateData.Rates=exchangeRateData.Rates.Skip(skip).Take(take).ToDictionary();
            response.Data = exchangeRateData;
            return response;
        }

        catch (Exception ex)
        {
            // Handle HTTP request related errors (e.g., network issues)
            response.Success= false;
            response.ErrorMessage= ex.Message;
            return response;
        }
    }
}
