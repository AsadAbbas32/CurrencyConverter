using CurrencyConverter.Data.Interface;
using CurrencyConverter.Data;
using CurrencyConverter.WebAPI.Services;
using Microsoft.Extensions.Caching.Memory;
using FluentValidation.AspNetCore;
using CurrencyConverter.Models.Dto.Validators;
using CurrencyConverter.Models.Dto.MapperProfile;

var builder = WebApplication.CreateBuilder(args);

// Configure cache options
builder.Services.AddMemoryCache(opts => new MemoryCacheEntryOptions()
    .SetSlidingExpiration(TimeSpan.FromMinutes(1))
    .SetAbsoluteExpiration(TimeSpan.FromSeconds(40)));

// Configure HttpClient
builder.Services.AddHttpClient();

// Register services
builder.Services.AddTransient<ICurrencyProvider, FrankfurterProvider>();
builder.Services.AddTransient<ICurrencyService, CurrencyService>();
builder.Services.AddControllers();

// Register Validaters for Request Models
builder.Services.AddFluentValidation(fv => 
    fv.RegisterValidatorsFromAssemblyContaining<CurrencyExchangeRateRequestDtoValidator>());

builder.Services.AddAutoMapper(config =>
{
    config.AddProfile<CurrencyExchangeRateProfile>();
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
