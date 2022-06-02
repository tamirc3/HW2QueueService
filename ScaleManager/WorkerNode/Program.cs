using Model;
using WorkerNode;

var builder = WebApplication.CreateBuilder(args);

string requestQueueUrl_dequeue;
string completedQueueUrl_enqueue;
if (!string.IsNullOrEmpty(builder.Configuration["QueueHost"]))
{
    requestQueueUrl_dequeue = builder.Configuration["QueueHost"] + QueueUrlConsts.requestsQueue_dequeue_url;
    completedQueueUrl_enqueue = builder.Configuration["QueueHost"] + QueueUrlConsts.completedQueue_enqueue_url;
}
else
{
    requestQueueUrl_dequeue = QueueUrlConsts.LocalHost_Queue + QueueUrlConsts.requestsQueue_dequeue_url;
    completedQueueUrl_enqueue = QueueUrlConsts.LocalHost_Queue + QueueUrlConsts.completedQueue_enqueue_url;
}


IWorkerManager workersManager = new WorkerManager(requestQueueUrl_dequeue, completedQueueUrl_enqueue);
workersManager.StartWorking();
builder.Services.AddSingleton(_ => workersManager);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


