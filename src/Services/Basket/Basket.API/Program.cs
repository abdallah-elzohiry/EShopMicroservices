var builder = WebApplication.CreateBuilder(args);

// Add DI


var app = builder.Build();


// Configure the HTTP Request Pipeline.


app.Run();
