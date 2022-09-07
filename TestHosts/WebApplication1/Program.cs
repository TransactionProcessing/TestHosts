using System.Security.Cryptography.Xml;
using WebApplication1;
using SoapCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSoapCore();
builder.Services.AddSingleton<IPingService, PingService>();
builder.Services.AddMvc();

var app = builder.Build();

app.UseRouting();
app.UseEndpoints(endpoints => {
 //endpoints.UseSoapEndpoint<IPingService>("/PingService.svc", new SoapEncoderOptions(), SoapSerializer.DataContractSerializer);
 endpoints.UseSoapEndpoint<IPingService>("/PingService.asmx", new SoapEncoderOptions(), SoapSerializer.XmlSerializer);
 });

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}