using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Web.Fiap.Carbono.Config.MongoDb;
using Web.Fiap.Carbono.Config.Security;
using Web.Fiap.Carbono.Data.Contexts;
using Web.Fiap.Carbono.Data.MongoDb;
using Web.Fiap.Carbono.Data.Repository;
using Web.Fiap.Carbono.Data.Repository.Implementations;
using Web.Fiap.Carbono.Data.Repository.Interfaces;
using Web.Fiap.Carbono.Mapping;
using Web.Fiap.Carbono.Middlewares;
using Web.Fiap.Carbono.Services.Implementations;
using Web.Fiap.Carbono.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

#region MongoDB configuration

builder.Services
    .AddOptions<MongoDbSettings>()
    .Bind(
        builder.Configuration.GetSection(
            MongoDbSettings.SectionName
        )
    )
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(
                settings.ConnectionString
            ),
        "MongoDb:ConnectionString is required."
    )
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(
                settings.DatabaseName
            ),
        "MongoDb:DatabaseName is required."
    )
    .ValidateOnStart();

builder.Services.AddSingleton<IMongoClient>(
    serviceProvider =>
    {
        var settings = serviceProvider
            .GetRequiredService<
                IOptions<MongoDbSettings>
            >()
            .Value;

        return new MongoClient(
            settings.ConnectionString
        );
    }
);

builder.Services.AddSingleton<IMongoDatabase>(
    serviceProvider =>
    {
        var client = serviceProvider
            .GetRequiredService<IMongoClient>();

        var settings = serviceProvider
            .GetRequiredService<
                IOptions<MongoDbSettings>
            >()
            .Value;

        return client.GetDatabase(
            settings.DatabaseName
        );
    }
);

builder.Services.AddSingleton<MongoDbContext>();

#endregion

// Add services to the container.

#region JwtAuthentication

var jwtSettingsSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtSettings>(jwtSettingsSection);

var jwtSettings = jwtSettingsSection.Get<JwtSettings>();

if (jwtSettings is null || string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
{
    throw new InvalidOperationException("As configurações de JWT não foram encontradas.");
}

var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key),

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

#endregion

#region DATA BASE INITIALIZATION

var connectionString = builder.Configuration.GetConnectionString("OracleConnection");
builder.Services.AddDbContext<DatabaseContext>(opt => opt.UseOracle(connectionString)
);

#endregion

#region AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});

builder.Services.AddScoped<IEmissaoCarbonoRepository, EmissaoCarbonoRepository>();
builder.Services.AddScoped<IProdutoCarbonoRepository, ProdutoCarbonoRepository>();
builder.Services.AddScoped<IFornecedorCarbonoRepository, FornecedorCarbonoRepository>();
builder.Services.AddScoped<IDashboardCarbonoRepository, DashboardCarbonoRepository>();

builder.Services.AddScoped<IEmissaoCarbonoService, EmissaoCarbonoService>();
builder.Services.AddScoped<IProdutoCarbonoService, ProdutoCarbonoService>();
builder.Services.AddScoped<IFornecedorCarbonoService, FornecedorCarbonoService>();
builder.Services.AddScoped<IDashboardCarbonoService, DashboardCarbonoService>();

builder.Services.AddScoped<IAuthService, AuthService>();
#endregion

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
