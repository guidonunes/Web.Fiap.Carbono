using Microsoft.EntityFrameworkCore;
using Web.Fiap.Carbono.Data.Contexts;
using Web.Fiap.Carbono.Data.Repository;
using Web.Fiap.Carbono.Data.Repository.Implementations;
using Web.Fiap.Carbono.Data.Repository.Interfaces;
using Web.Fiap.Carbono.Mapping;
using Web.Fiap.Carbono.Middlewares;
using Web.Fiap.Carbono.Services.Implementations;
using Web.Fiap.Carbono.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#region DATA BASE INITIALIZATION

var connectionString = builder.Configuration.GetConnectionString("DatabaseConnection");
builder.Services.AddDbContext<DatabaseContext>(opt => opt.UseOracle(connectionString).EnableSensitiveDataLogging(true)
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