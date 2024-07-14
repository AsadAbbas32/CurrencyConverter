using AutoMapper;
using CurrencyConverter.Data.Interface;
using CurrencyConverter.Models;
using CurrencyConverter.WebAPI.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace CurrencyConverter.Test
{
    public class CurrencyServiceTest
    {
        private Mock<ICurrencyProvider> _mockCurrencyProvider;
        private Mock<IMapper> _mockMapper;
        private MemoryCache _cache;
        private ICurrencyService _service;

        public CurrencyServiceTest()
        {
            _mockCurrencyProvider = new Mock<ICurrencyProvider>();
            _mockMapper = new Mock<IMapper>();
            _cache = new MemoryCache(new MemoryCacheOptions());

            var inMemorySettings = new Dictionary<string, string> { { "ExcludedCurrencies", "try,pln,thb,mxn" } };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            _service = new CurrencyService(_mockCurrencyProvider.Object, _cache, _mockMapper.Object, configuration);
        }

        #region ConvertCurrency
        [Fact]
        public async Task ConvertCurrency_WithCache_ReturnsSuccess()
        {
            // Arrange
            var request = new CurrencyExchangeRateRequestDto { From = "USD", To = "EUR", Amount = 100 };
            var cachedRates = new CurrencyExchangeRateCacheDto { ToCurrency = "EUR", UnitRate = 0.9M };

            _cache.Set($"exchange-rate-{request.From.ToLower()}-{request.To.ToLower()}", cachedRates);

            // Act
            var response = await _service.ConvertCurrency(request);

            // Assert
            Assert.True(response.Success);
            Assert.Null(response.ErrorMessage);
            Assert.Equal(request.Amount, response.Data.Amount);
            Assert.Equal(request.From, response.Data.Base);
            Assert.Single(response.Data.Rates);
            Assert.Equal(cachedRates.UnitRate * request.Amount, response.Data.Rates["EUR"]);
            _mockCurrencyProvider.Verify(p => p.ConvertCurrency(It.IsAny<CurrencyExchangeRateRequestDto>()), Times.Never);
        }

        [Fact]
        public async Task ConvertCurrency_NoCache_ReturnsSuccess()
        {
            // Arrange
            var request = new CurrencyExchangeRateRequestDto { From = "USD", To = "EUR", Amount = 100 };
            var exchangeRateData = new CurrencyExchangeRateResponseDto { Base = "USD", Amount = request.Amount, Rates = { { "EUR", 0.9M } } };
            var mappedDto = new CurrencyExchangeRateCacheDto { ToCurrency = "EUR", UnitRate = 0.9M };
            _mockCurrencyProvider.Setup(p => p.ConvertCurrency(It.IsAny<CurrencyExchangeRateRequestDto>()))
                .Returns(Task.FromResult(exchangeRateData));
            _mockMapper.Setup(m => m.Map<CurrencyExchangeRateCacheDto>(It.IsAny<CurrencyExchangeRateResponseDto>()))
                .Returns(mappedDto);

            // Act
            var response = await _service.ConvertCurrency(request);

            // Assert
            Assert.True(response.Success);
            Assert.Null(response.ErrorMessage);
            Assert.Equal(request.Amount, response.Data.Amount);
            Assert.Equal(request.From, response.Data.Base);
            Assert.Single(response.Data.Rates);
            Assert.Equal(exchangeRateData.Rates["EUR"], response.Data.Rates["EUR"]);
            var cacheKey = $"exchange-rate-{request.From.ToLower()}-{request.To.ToLower()}";
            Assert.NotNull(_cache.Get<CurrencyExchangeRateCacheDto>(cacheKey));
            _mockCurrencyProvider.Verify(p => p.ConvertCurrency(request), Times.Once);
        }

        [Fact]
        public async Task ConvertCurrency_MissingExcludedCurrencyConfig_ReturnsSuccess()
        {
            // Arrange
            var request = new CurrencyExchangeRateRequestDto { From = "TRY", To = "USD" };

            // Act
            var response = await _service.ConvertCurrency(request);

            // Assert
            Assert.False(response.Success);
            Assert.NotNull(response.ErrorMessage);
        }
        #endregion

        #region GetLatestRates
        [Fact]
        public async Task GetLatestRates_WithCache_ReturnsSuccess()
        {
            // Arrange
            var baseCurrency = "USD";
            var cachedRates = new CurrencyExchangeRateResponseDto { Base = "USD", Rates = { { "EUR", 0.9M } } };
            _cache.Set($"latest-rates-{baseCurrency.ToLower()}", cachedRates);

            // Act
            var response = await _service.GetLatestRates(baseCurrency);

            // Assert
            Assert.True(response.Success);
            Assert.Null(response.ErrorMessage);
            Assert.Equal(baseCurrency, response.Data.Base);
            Assert.Single(response.Data.Rates);
            Assert.Equal(cachedRates.Rates["EUR"], response.Data.Rates["EUR"]);
            _mockCurrencyProvider.Verify(p => p.GetLatestRates(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetLatestRates_NoCache_ReturnsSuccess()
        {
            // Arrange
            var baseCurrency = "USD";
            var exchangeRateData = new CurrencyExchangeRateResponseDto { Base = "USD", Rates = { { "EUR", 0.9M } } };
            _mockCurrencyProvider.Setup(p => p.GetLatestRates(It.IsAny<string>()))
                .Returns(Task.FromResult(exchangeRateData));

            // Act
            var response = await _service.GetLatestRates(baseCurrency);

            // Assert
            Assert.True(response.Success);
            Assert.Null(response.ErrorMessage);
            Assert.Equal(baseCurrency, response.Data.Base);
            Assert.Single(response.Data.Rates);
            Assert.Equal(exchangeRateData.Rates["EUR"], response.Data.Rates["EUR"]);
            var cacheKey = $"latest-rates-{baseCurrency.ToLower()}";
            Assert.NotNull(_cache.Get<CurrencyExchangeRateResponseDto>(cacheKey));
            _mockCurrencyProvider.Verify(p => p.GetLatestRates(baseCurrency), Times.Once);
        }

        [Fact]
        public async Task GetLatestRates_HttpRequestException_ReturnsError()
        {
            // Arrange
            var baseCurrency = "USD";
            _mockCurrencyProvider.Setup(p => p.GetLatestRates(It.IsAny<string>()))
                .Throws<HttpRequestException>();

            // Act
            var response = await _service.GetLatestRates(baseCurrency);

            // Assert
            Assert.False(response.Success);
            Assert.NotNull(response.ErrorMessage);
            _mockCurrencyProvider.Verify(p => p.GetLatestRates(baseCurrency), Times.Once);
        }
        #endregion
        #region GetCurrencyHistory
        [Fact]
        public async Task GetCurrencyHistory_CacheHit_ReturnsCachedData()
        {
            // Arrange
            var request = new CurrencyExchangeRateHistoryRequestDto
            {
                BaseCurrency = "USD",
                StartDate = DateTime.UtcNow.AddDays(-7),
                EndDate = DateTime.UtcNow,
                Page = 1,
                Take = 10
            };
            var expectedData = new CurrencyExchangeRateHistoryResponseDto { Rates = new() };
            var cacheKey = $"currency-history-{request.BaseCurrency.ToLower()}" +
            $"-{request.StartDate.ToString("yyyy-MM-dd")}-{request.EndDate.ToString("yyyy-MM-dd")}";
            _cache.Set(cacheKey, expectedData);

            // Act
            var response = await _service.GetCurrencyHistory(request);

            // Assert
            Assert.True(response.Success);
            Assert.Equal(expectedData, response.Data);
            _mockCurrencyProvider.Verify(p => p.GetCurrencyHistory(It.IsAny<CurrencyExchangeRateHistoryRequestDto>()), Times.Never);
            Assert.NotNull(_cache.Get<CurrencyExchangeRateHistoryResponseDto>(cacheKey));
        }

        [Fact]
        public async Task GetCurrencyHistory_CacheMiss_Success()
        {
            // Arrange
            var request = new CurrencyExchangeRateHistoryRequestDto
            {
                BaseCurrency = "USD",
                StartDate = DateTime.UtcNow.AddDays(-7),
                EndDate = DateTime.UtcNow,
                Page = 1,
                Take = 10
            };
            var cacheKey = $"currency-history-{request.BaseCurrency.ToLower()}" +
            $"-{request.StartDate.ToString("yyyy-MM-dd")}-{request.EndDate.ToString("yyyy-MM-dd")}";
            var expectedData = new CurrencyExchangeRateHistoryResponseDto { Rates = new() };
            _mockCurrencyProvider.Setup(p => p.GetCurrencyHistory(request))
                .Returns(Task.FromResult(expectedData));

            // Act
            var response = await _service.GetCurrencyHistory(request);

            // Assert
            Assert.True(response.Success);
            Assert.Equal(expectedData.Rates.Count, response.TotalRecords);
            Assert.Equal(expectedData.Rates.Take(request.Take).ToDictionary(), response.Data.Rates);
            _mockCurrencyProvider.Verify(p => p.GetCurrencyHistory(It.IsAny<CurrencyExchangeRateHistoryRequestDto>()), Times.Once);
            Assert.NotNull(_cache.Get<CurrencyExchangeRateHistoryResponseDto>(cacheKey));
        }
        #endregion

    }
}