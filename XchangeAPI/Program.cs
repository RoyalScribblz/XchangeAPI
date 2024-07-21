using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Minio;
using XchangeAPI.Database;
using XchangeAPI.Database.Dtos;
using XchangeAPI.Endpoints;
using XchangeAPI.Endpoints.Validation;
using XchangeAPI.Extensions;
using XchangeAPI.Options;
using XchangeAPI.Services.AccountService;
using XchangeAPI.Services.CurrencyService;
using XchangeAPI.Services.EvidenceRequestService;
using XchangeAPI.Services.PendingExchangeService;
using XchangeAPI.Services.StorageBucketService;
using XchangeAPI.Services.StorageBucketSetupService;
using XchangeAPI.Services.UserService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["Auth0:Domain"];
    options.Audience = builder.Configuration["Auth0:Audience"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = ClaimTypes.NameIdentifier
    };
    options.RequireHttpsMetadata = false;
});

builder.Services.AddAuthorization(options => options.AddPolicy("RequireAdmin",
    policy => policy.RequireRole("Admin")));

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddMinio(configureClient => configureClient
    .WithEndpoint(builder.Configuration["minio:endpoint"])
    .WithCredentials(builder.Configuration["minio:accessKey"], builder.Configuration["minio:secretKey"])
    .WithSSL(false)
    .Build());

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddDbContext<XchangeDatabase>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("XchangeDb")));

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

builder.Services.AddHttpClient()
    .AddScoped<IAccountService, AccountService>()
    .AddScoped<ICurrencyService, CurrencyService>()
    .AddScoped<IEvidenceRequestService, EvidenceRequestService>()
    .AddScoped<IUserService, UserService>()
    .AddScoped<IStorageBucketService, StorageBucketService>()
    .AddSingleton<IPendingExchangeService, PendingExchangeService>()
    .AddTransient<IValidator<CreateAccountRequest>, CreateAccountValidator>();

builder.Services.AddHostedService<StorageBucketSetupService>();

builder.Services.AddHttpClient<CurrencyService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger()
        .UseSwaggerUI()
        .UseCors("LocalDev");
}

app.UseAuthentication()
    .UseAuthorization();

app.UseHttpsRedirection();

app.MapAccountEndpoints()
    .MapCurrencyEndpoints()
    .MapEvidenceRequestEndpoints()
    .MapUserEndpoints();

using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider
    .GetRequiredService<XchangeDatabase>();

var created = await context.Database.EnsureCreatedAsync();

if (created)
{
    await context.Seed(currencies);
}

await app.RunAsync();
