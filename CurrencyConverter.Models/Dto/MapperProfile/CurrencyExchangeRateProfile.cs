using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CurrencyConverter.Models.Dto.MapperProfile;

public class CurrencyExchangeRateProfile: Profile
{
    public CurrencyExchangeRateProfile()
    {
        CreateMap<CurrencyExchangeRateResponseDto, CurrencyExchangeRateCacheDto>()
            .ForMember(dest => dest.FromCurrency, opt => opt.MapFrom(src => src.Base))
            .ForMember(dest => dest.ToCurrency, opt => opt.MapFrom(src => src.Rates.First().Key))
            .ForMember(dest => dest.UnitRate, opt => opt.MapFrom(src => src.Rates.First().Value/src.Amount));
    }
}
