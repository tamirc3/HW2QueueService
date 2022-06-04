using WorkerNode;

var builder = WebApplication.CreateBuilder(args);

string requestQueueUrl_dequeue;
string completedQueueUrl_enqueue;
if (!string.IsNullOrEmpty(builder.Configuration["QueueHost"]))
{
    requestQueueUrl_dequeue = builder.Configuration["QueueHost"] + "/Queue/workerQueue/dequeue";
    completedQueueUrl_enqueue = builder.Configuration["QueueHost"] + "/Queue/completedQueue/enqueue";
}
else
{
    requestQueueUrl_dequeue = "https://localhost:7108" + "/Queue/workerQueue/dequeue";
    completedQueueUrl_enqueue = "https://localhost:7108" + "/Queue/completedQueue/enqueue";
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


