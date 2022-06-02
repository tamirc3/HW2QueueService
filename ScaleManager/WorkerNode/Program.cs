using Model;
using WorkerNode;

var builder = WebApplication.CreateBuilder(args);

string requestQueueUrl_dequeue;
string completedQueueUrl_enqueue;
if (QueueUrlConsts.ShouldTakeValuesFromConfig)
{
    requestQueueUrl_dequeue = builder.Configuration["QueueHost"] + QueueUrlConsts.requestsQueue_dequeue_url;
    completedQueueUrl_enqueue = builder.Configuration["QueueHost"] + QueueUrlConsts.completedQueue_enqueue_url;
}
else
{
    requestQueueUrl_dequeue = QueueUrlConsts.QueueHost + QueueUrlConsts.requestsQueue_dequeue_url;
    completedQueueUrl_enqueue = QueueUrlConsts.QueueHost + QueueUrlConsts.completedQueue_enqueue_url;
}


IWorkerManager workersManager = new WorkerManager(requestQueueUrl_dequeue, completedQueueUrl_enqueue);
workersManager.StartWorking();
builder.Services.AddSingleton(_ => workersManager);

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


