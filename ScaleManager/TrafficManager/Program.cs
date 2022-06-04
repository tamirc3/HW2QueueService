using TrafficManager.Controllers;

var builder = WebApplication.CreateBuilder(args);

string nodeApiA;
string nodeApiB;
if (!string.IsNullOrEmpty(builder.Configuration["QueueServiceApiNodeA"]) &&
    !string.IsNullOrEmpty(builder.Configuration["QueueServiceApiNodeB"]))
{
     nodeApiA = builder.Configuration["QueueServiceApiNodeA"];
     nodeApiB = builder.Configuration["QueueServiceApiNodeB"];
}
else
{
     nodeApiA = "https://localhost:7145";
     nodeApiB = "https://localhost:7145";
}

builder.Services.AddScoped<ILoadBalancerClass>(_ =>
    new LoadBalancerClass(nodeApiA, nodeApiB));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
