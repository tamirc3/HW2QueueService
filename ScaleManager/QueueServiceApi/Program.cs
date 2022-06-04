using QueueServiceApi.Controllers;

var builder = WebApplication.CreateBuilder(args);

string requestsQueue_enqueue_url;
string completedQueue_dequeue_url;

if (!string.IsNullOrEmpty(builder.Configuration["QueueHost"]))
{
    requestsQueue_enqueue_url = builder.Configuration["QueueHost"] + "/Queue/workerQueue/enqueue";
    completedQueue_dequeue_url = builder.Configuration["QueueHost"] + "/Queue/completedQueue/dequeue";
}
else
{
    requestsQueue_enqueue_url = "https://localhost:7108" + "/Queue/workerQueue/enqueue";
    completedQueue_dequeue_url = "https://localhost:7108" + "/Queue/completedQueue/dequeue";
}

builder.Services.AddScoped<IWorkerQueueService>(_ =>
    new WorkerQueueService(requestsQueue_enqueue_url, completedQueue_dequeue_url));



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
