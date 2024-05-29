using XchangeAPI.Database;
using XchangeAPI.Database.Dtos;
using XchangeAPI.Endpoints;
using XchangeAPI.Extensions;
using XchangeAPI.Options;
using XchangeAPI.Services.AccountService;
using XchangeAPI.Services.CurrencyService;
using XchangeAPI.Services.EvidenceRequestService;
using XchangeAPI.Services.UserService;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddDbContext<XchangeDatabase>();

List<Currency> currencies = [];
builder.Configuration.GetSection("Currencies").Bind(currencies);

builder.Services.Configure<OpenExchangeRatesOptions>(builder.Configuration.GetSection("OpenExchangeRatesOptions"));

builder.Services.AddHttpClient();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IEvidenceRequestService, EvidenceRequestService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddHttpClient<CurrencyService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapAccountEndpoints();

await app.Services.CreateScope().ServiceProvider
    .GetRequiredService<XchangeDatabase>()
    .Seed(currencies);

app.Run();