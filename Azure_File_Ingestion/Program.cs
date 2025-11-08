using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "File API",
        Version = "v1"
    });
});

var funcBaseUrl = builder.Configuration["FUNC_BASE_URL"];
if (string.IsNullOrWhiteSpace(funcBaseUrl))
    throw new InvalidOperationException("FUNC_BASE_URL not configured");

builder.Services.AddHttpClient("blobUploadFunction", c =>
{
    c.BaseAddress = new Uri(funcBaseUrl);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint("/swagger/v1/swagger.json", "File API v1");
        o.RoutePrefix = "swagger";
    });

    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}

app.MapControllers();
await app.RunAsync();