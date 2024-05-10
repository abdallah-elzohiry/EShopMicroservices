var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(Program).Assembly;
var ConnectionString = builder.Configuration.GetConnectionString("Database")!;
var CacheConnectionString = builder.Configuration.GetConnectionString("Redis")!;
// Add DI

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssemblies(assembly);
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddMarten(opts =>
{
    opts.Connection(ConnectionString);
    opts.Schema.For<ShoppingCart>().Identity(x => x.UserName);
}).UseLightweightSessions();
builder.Services.AddCarter();

builder.Services.AddExceptionHandler<CustomExceptionHandler>();

builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.Decorate<IBasketRepository, CachedBasketRepository>();
builder.Services.AddStackExchangeRedisCache(opt =>
{
    opt.Configuration = CacheConnectionString;
});

builder.Services.AddHealthChecks()
    .AddNpgSql(ConnectionString)
    .AddRedis(CacheConnectionString);

var app = builder.Build();


// Configure the HTTP Request Pipeline.
app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapCarter();
app.UseExceptionHandler(opt => { });
app.Run();
