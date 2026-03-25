using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "docs";
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "PingApi v1");
});

app.MapGet("/ping", () => Results.Ok(new { 
    message = "Hola, este mensaje vive gracias a APS.NET Core 9" 
}));

app.Run();
