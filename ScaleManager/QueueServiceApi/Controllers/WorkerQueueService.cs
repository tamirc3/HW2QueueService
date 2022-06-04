using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace QueueServiceApi.Controllers;

public interface IWorkerQueueService
{
    Task<EnqueueRequestResponse> EnqueueMessageAsync(HashRequest hashRequest);
    Task<List<string>> DequeueCompletedMessagesAsync(int numberOfCompletedRequests);
    Task<string> GetCompletedMessageFromQueue();
}

public class WorkerQueueService : IWorkerQueueService
{
    private readonly string requestsQueue_enqueue_url;
    private readonly string completedQueue_dequeue_url;
    public WorkerQueueService(string requestsQueueEnqueueUrl, string completedQueueDequeueUrl)
    {
        requestsQueue_enqueue_url = requestsQueueEnqueueUrl;
        completedQueue_dequeue_url = completedQueueDequeueUrl;
    }
    public async Task<EnqueueRequestResponse> EnqueueMessageAsync(HashRequest hashRequest)
    {
        WorkerRequestMessage workerRequestMessage = new WorkerRequestMessage(hashRequest);

        Uri relativeUri = new Uri(requestsQueue_enqueue_url);

        Console.WriteLine($"QueueServiceApi sending enqueue request to: {relativeUri}");
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = relativeUri,
            Content = new StringContent( JsonSerializer.Serialize(workerRequestMessage) , Encoding.UTF8, "application/json")
        };

        HttpClient httpClient = new HttpClient();
        var responseMessage = await httpClient.SendAsync(request);

        EnqueueRequestResponse enqueueRequestResponse = new EnqueueRequestResponse
        {
            WorkerQueueResponse = responseMessage,
            TaskID = workerRequestMessage.Id
        };
        return enqueueRequestResponse;

    }
    public async Task<List<string>> DequeueCompletedMessagesAsync(int numberOfCompletedRequests)
    {

        var myCollection = Enumerable.Range(0, numberOfCompletedRequests);
        var bag = new ConcurrentBag<string>();
        var tasks = myCollection.Select(async _ =>
        {
            var completedMessage = await GetCompletedMessageFromQueue();
            if (!string.IsNullOrEmpty(completedMessage))
            { 
                bag.Add(completedMessage);
            }

        });
        await Task.WhenAll(tasks);
        

        return bag.ToList();
    }

    public async Task<string> GetCompletedMessageFromQueue()
    {
        string completedMessage = "";
        Console.WriteLine($"QueueServiceApi completedQueue_dequeue_url: {completedQueue_dequeue_url}");
        Uri relativeUri = new Uri(completedQueue_dequeue_url);

        using var request = new HttpRequestMessage {Method = HttpMethod.Get, RequestUri = relativeUri,};
        using HttpClient httpClient = new HttpClient();
        var responseMessage = await httpClient.SendAsync(request);
        if (responseMessage.IsSuccessStatusCode)
        {
            completedMessage = await responseMessage.Content.ReadAsStringAsync();
        }

        return completedMessage;
    }
}