using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Web.Fiap.Carbono.Config.MongoDb;
using Web.Fiap.Carbono.Config.Security;
using Web.Fiap.Carbono.Config.Swagger;
using Web.Fiap.Carbono.Data.MongoDb;
using Web.Fiap.Carbono.Data.MongoDb.Repositories;
using Web.Fiap.Carbono.Data.MongoDb.Repositories.Interfaces;
using Web.Fiap.Carbono.Middlewares;
using Web.Fiap.Carbono.Services.Interfaces;
using Web.Fiap.Carbono.Services.Implementations;
using Web.Fiap.Carbono.Services.MongoDb;
using Web.Fiap.Carbono.Services.MongoDb.Interfaces;

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

builder.Services.AddScoped<
    IMongoEmpresaRepository,
    MongoEmpresaRepository
>();
builder.Services.AddScoped<
    IMongoProdutoRepository,
    MongoProdutoRepository
>();
builder.Services.AddScoped<
    IMongoFornecedorRepository,
    MongoFornecedorRepository
>();
builder.Services.AddScoped<
    IMongoFatorEmissaoRepository,
    MongoFatorEmissaoRepository
>();
builder.Services.AddScoped<
    IMongoEmissaoCarbonoRepository,
    MongoEmissaoCarbonoRepository
>();

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

#region Services
builder.Services.AddScoped<IAuthService, AuthService>();
#endregion

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlDocumentation = Path.Combine(
        AppContext.BaseDirectory,
        $"{typeof(Program).Assembly.GetName().Name}.xml"
    );

    options.IncludeXmlComments(xmlDocumentation);
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description =
                "Informe o JWT no formato: Bearer {token}.",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        }
    );
    options.OperationFilter<AuthorizationOperationFilter>();
});

builder.Services.AddScoped<
    IMongoEmpresaService,
    MongoEmpresaService>();

builder.Services.AddScoped<
    IMongoProdutoService,
    MongoProdutoService>();

builder.Services.AddScoped<
    IMongoFornecedorService,
    MongoFornecedorService>();

builder.Services.AddScoped<
    IMongoFatorEmissaoService,
    MongoFatorEmissaoService>();

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddScoped<
    IMongoEmissaoCarbonoService,
    MongoEmissaoCarbonoService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
