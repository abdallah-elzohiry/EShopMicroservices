
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(Program).Assembly;
var ConnectionString = builder.Configuration.GetConnectionString("Database")!;
// Add DI

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssemblies(assembly);
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(assembly);
builder.Services.AddCarter();
builder.Services.AddMarten(opt =>
{
    opt.Connection(ConnectionString);
}).UseLightweightSessions();
if (builder.Environment.IsDevelopment())
{
    builder.Services.InitializeMartenWith<CatalogInitialData>();
}
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

builder.Services.AddHealthChecks().AddNpgSql(ConnectionString);

var app = builder.Build();

// Configure the HTTP Request Pipeline.
app.UseExceptionHandler(options => { });
app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapCarter();
app.Run();
