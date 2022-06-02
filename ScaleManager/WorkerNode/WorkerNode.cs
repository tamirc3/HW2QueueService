using System.Net;
using System.Text;
using Model;
using Newtonsoft.Json;


namespace WorkerNode
{
    public class WorkerNode : IWorkerNode
    {
        private readonly string _requestsQueueDequeueUrl;
        private readonly string _completedQueueEnqueueUrl;
        public bool IsRunning { get; set; }
        public bool IsBusy { get; set; }

        public WorkerNode(string requestsQueueDequeueUrl, string completedQueueEnqueueUrl)
        {
            _requestsQueueDequeueUrl = requestsQueueDequeueUrl;
            _completedQueueEnqueueUrl = completedQueueEnqueueUrl;
            IsRunning = true;
        }
        private ComputedMessage CalculateHash(string jobId, string buffer, int iterations)
        {
            var bytes = Encoding.UTF8.GetBytes(buffer);
            using var hash = System.Security.Cryptography.SHA512.Create();
            var hashedInputBytes = hash.ComputeHash(bytes);
            for (int i = 1; i < iterations; i++)
            {
                hashedInputBytes = hash.ComputeHash(hashedInputBytes);
            }

            // Convert to text
            // StringBuilder Capacity is 128, because 512 bits / 8 bits in byte * 2 symbols for byte 
            var hashedInputStringBuilder = new StringBuilder(128);
            foreach (var b in hashedInputBytes)
                hashedInputStringBuilder.Append(b.ToString("X2"));

            string hashedString = hashedInputStringBuilder.ToString();
            ComputedMessage result = new ComputedMessage()
            {
                Id = jobId,
                buffer = hashedString
            };
            return result;
        }




        public async void GetItemsFromQueue(object? o)
        {
            while (IsRunning)
            {
                try
                {
                    var itemFromQueue = await GetItemFromQueue();
                    if (itemFromQueue != null)
                    {
                        IsBusy = true;
                        var computedMessage = CalculateHash(itemFromQueue.Id, itemFromQueue.HashRequest.Buffer,
                            itemFromQueue.HashRequest.Iterations);
                        await PostComputedMessage(computedMessage);
                    }
                    await Task.Delay(1000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
            
                }
                IsBusy = false;

            }
        }

        public void StopRunning()
        {
            IsRunning = false;
        }



        private  async Task<WorkerRequestMessage> GetItemFromQueue()
        {
            WorkerRequestMessage currentItem = null;
            var httpClient = new HttpClient();

            var response = await httpClient.GetAsync(_requestsQueueDequeueUrl);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                 currentItem = JsonConvert.DeserializeObject<WorkerRequestMessage>(responseContent);
            }

            return currentItem;
        }

        private async Task PostComputedMessage(ComputedMessage? computedMessage)
        {
            if (computedMessage != null)
            {
                var httpClient = new HttpClient();

                var content = JsonConvert.SerializeObject(computedMessage);

                
                var completedMessageResult = await httpClient.PostAsync(
                    string.Format(_completedQueueEnqueueUrl),
                    new StringContent(content, Encoding.UTF8, "application/json"));

                if (completedMessageResult.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception("failed to post result");
                }
            }
        }

        public void Start()
        {
            var cancellationTokenSource = new CancellationTokenSource();
                Task.Factory.StartNew(GetItemsFromQueue
                    , TaskCreationOptions.LongRunning
                    , cancellationTokenSource.Token);
            
        }
    }
}
