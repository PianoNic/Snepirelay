using Snepirelay.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);
builder.Services.AddSnepirelay(builder.Configuration);

var app = builder.Build();

app.MapSnepirelay();
app.Run();
