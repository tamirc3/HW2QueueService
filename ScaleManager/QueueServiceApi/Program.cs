using Model;
using QueueServiceApi.Controllers;

var builder = WebApplication.CreateBuilder(args);

string requestsQueue_enqueue_url;
string completedQueue_dequeue_url;

if (QueueUrlConsts.ShouldTakeValuesFromConfig)
{
    requestsQueue_enqueue_url = builder.Configuration["QueueHost"] + QueueUrlConsts.requestsQueue_enqueue_url;
    completedQueue_dequeue_url = builder.Configuration["QueueHost"] + QueueUrlConsts.completedQueue_dequeue_url;
}
else
{
    requestsQueue_enqueue_url = QueueUrlConsts.QueueHost + QueueUrlConsts.requestsQueue_enqueue_url;
    completedQueue_dequeue_url = QueueUrlConsts.QueueHost + QueueUrlConsts.completedQueue_dequeue_url;
}

builder.Services.AddScoped<IWorkerQueueService>(_ =>
    new WorkerQueueService(requestsQueue_enqueue_url, completedQueue_dequeue_url));



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
