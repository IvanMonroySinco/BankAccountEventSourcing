using BankAccount.API.Handlers;
using JasperFx;
using JasperFx.Events;
using Marten;
using Weasel.Core;

var builder = WebApplication.CreateBuilder(args);

// Configurar Marten (Event Store)
var connectionString = builder.Configuration.GetConnectionString("PostgreSQL")
    ?? "Host=localhost;Database=bankaccount_db;Username=postgres;Password=postgres";

builder.Services.AddMarten(options =>
{
    options.Connection(connectionString);
    options.AutoCreateSchemaObjects = AutoCreate.All;
    options.Events.StreamIdentity = StreamIdentity.AsGuid;
});

// Registrar Handlers
builder.Services.AddScoped<IBankAccountCommandHandler, BankAccountCommandHandler>();
builder.Services.AddScoped<IBankAccountQueryHandler, BankAccountQueryHandler>();

// Agregar controladores
builder.Services.AddControllers();

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// OpenAPI en desarrollo
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Mapear controladores
app.MapControllers();

app.Run();
