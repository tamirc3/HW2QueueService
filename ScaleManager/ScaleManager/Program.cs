using AutoScaleService.Services;
using ScaleManager.Services;

var builder = WebApplication.CreateBuilder(args);

var tenantID = builder.Configuration["tenantID"];
var clientID = builder.Configuration["clientID"];
var clientSecret = builder.Configuration["clientSecret"];

var queueHost = builder.Configuration["QueueHost"];
AzureCredentials azureCredentials = new AzureCredentials(clientSecret, clientID, tenantID);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IAppServiceManager, AppServiceManager>();


var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
