using CurrencyConverter.Models;
using CurrencyConverter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CurrencyConverter.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CurrencyConverterApiController(ILogger<CurrencyConverterApiController> logger, ICurrencyService currencyService) : ControllerBase
    {
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestRates([FromQuery][Required] string baseCurrency)
        {
            if (string.IsNullOrEmpty(baseCurrency))
            {
                return BadRequest("Missing 'baseCurrency' parameter");
            }

            try
            {
                var rates = await currencyService.GetLatestRates(baseCurrency);
                return Ok(rates);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, ex.Message); // Internal Server Error
            }
        }

        [HttpGet("convert")]
        public async Task<IActionResult> ConvertCurrency([FromQuery] CurrencyExchangeRateRequestDto request)
        {
            try
            {
                var rates = await currencyService.ConvertCurrency(request);
                return Ok(rates);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, ex.Message); // Internal Server Error
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetGetCurrencyHistory([FromQuery] CurrencyExchangeRateHistoryRequestDto request)
        {
            try
            {
                var rates = await currencyService.GetCurrencyHistory(request);
                return Ok(rates);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, ex.Message); // Internal Server Error
            }
        }
    }
}
