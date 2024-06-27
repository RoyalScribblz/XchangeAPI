using XchangeAPI.Database;
using XchangeAPI.Database.Dtos;
using XchangeAPI.Endpoints;
using XchangeAPI.Extensions;
using XchangeAPI.Options;
using XchangeAPI.Services.AccountService;
using XchangeAPI.Services.CurrencyService;
using XchangeAPI.Services.EvidenceRequestService;
using XchangeAPI.Services.PendingExchangeService;
using XchangeAPI.Services.UserService;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddDbContext<XchangeDatabase>();

builder.Services.AddCors(
    options => options.AddPolicy(
        name: "LocalDev",
        policy =>
        {
            policy.SetIsOriginAllowed(origin => new Uri(origin).IsLoopback)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }));

List<Currency> currencies = [];
builder.Configuration.GetSection("Currencies").Bind(currencies);

builder.Services.Configure<OpenExchangeRatesOptions>(builder.Configuration.GetSection("OpenExchangeRatesOptions"));

builder.Services.AddHttpClient();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IEvidenceRequestService, EvidenceRequestService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IPendingExchangeService, PendingExchangeService>();

builder.Services.AddHttpClient<CurrencyService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger()
        .UseSwaggerUI()
        .UseCors("LocalDev");
}

app.UseHttpsRedirection();

app.MapAccountEndpoints()
    .MapCurrencyEndpoints()
    .MapEvidenceRequestEndpoints()
    .MapUserEndpoints();

using var scope = app.Services.CreateScope();
await scope.ServiceProvider
    .GetRequiredService<XchangeDatabase>()
    .Seed(currencies);

await app.RunAsync();
