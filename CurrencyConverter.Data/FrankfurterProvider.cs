using CurrencyConverter.Data.Interface;
using CurrencyConverter.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Data
{
    public class FrankfurterProvider : ICurrencyProvider
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public FrankfurterProvider(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            ValidateConfiguration(configuration);
            _baseUrl = configuration["FrankfurtAPIBaseUrl"];
        }

        public async Task<CurrencyExchangeRateResponseDto> ConvertCurrency(CurrencyExchangeRateRequestDto request)
        {
            var url = $"{_baseUrl}/latest?amount={request.Amount}&from={request.From}&to={request.To}";

            try
            {
                var response = await CallFrankfurtApiWithRetry(_httpClient, url);

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Failed to get Convertion from {request.From} to {request.To}. Status code: {response.StatusCode}");

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<CurrencyExchangeRateResponseDto>(content);

            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP request related errors (e.g., network issues)
                throw new Exception($"Error occurred while fetching rates: {ex.Message}");
            }
            catch (Exception)
            {
                // Handle unexpected exceptions
                throw;  // Re-throw for broader error handling in the calling code
            }
        }

        public async Task<CurrencyExchangeRateResponseDto> GetLatestRates(string baseCurrency)
        {
            var url = $"{_baseUrl}/latest?base={baseCurrency}";

            try
            {
                var response = await CallFrankfurtApiWithRetry(_httpClient, url);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to get latest rates for {baseCurrency}. Status code: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<CurrencyExchangeRateResponseDto>(content);

            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP request related errors (e.g., network issues)
                throw new Exception($"Error occurred while fetching rates: {ex.Message}");
            }
            catch (Exception)
            {
                // Handle unexpected exceptions
                throw;  // Re-throw for broader error handling in the calling code
            }
        }

        public async Task<CurrencyExchangeRateHistoryResponseDto> GetCurrencyHistory(CurrencyExchangeRateHistoryRequestDto request)
        {
            var url = $"{_baseUrl}/{request.StartDate.ToString("yyyy-MM-dd")}..{request.EndDate.ToString("yyyy-MM-dd")}?base={request.BaseCurrency}";

            try
            {
                var response = await CallFrankfurtApiWithRetry(_httpClient,url);

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Failed to get rate history for {request.BaseCurrency}. Status code: {response.StatusCode}");

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<CurrencyExchangeRateHistoryResponseDto>(content);

            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP request related errors (e.g., network issues)
                throw new Exception($"Error occurred while fetching rates: {ex.Message}");
            }
            catch (Exception)
            {
                // Handle unexpected exceptions
                throw;  // Re-throw for broader error handling in the calling code
            }
        }

        #region Private
        private void ValidateConfiguration(IConfiguration configuration)
        {
            if (string.IsNullOrEmpty(configuration["FrankfurtAPIBaseUrl"]))
                throw new ConfigurationErrorsException("Missing required configuration: ApiBaseUrl");
        }

        public async Task<HttpResponseMessage> CallFrankfurtApiWithRetry(HttpClient httpClient, string url)
        {
            var retryPolicy = Policy
                .Handle<HttpRequestException>(ex => ex.StatusCode != HttpStatusCode.OK)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))); // Exponential backoff

            return await retryPolicy.ExecuteAsync(async () =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                return await httpClient.SendAsync(request);
            });
        }
        #endregion
    }

}
