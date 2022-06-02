using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace QueueApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QueueController : ControllerBase
    {

        private static readonly ConcurrentQueue<WorkerRequestMessage> WorkerRequests = new ConcurrentQueue<WorkerRequestMessage>();
        private static readonly ConcurrentQueue<ComputedMessage> CompletedRequests = new ConcurrentQueue<ComputedMessage>();
        private readonly ILogger<QueueController> _logger;

        public QueueController(ILogger<QueueController> logger)
        {
            _logger = logger;
        }

        
        [HttpPost]
        [Route("workerQueue/enqueue")]
        public async Task<ActionResult> EnqueueWorkerMessage([FromBody] WorkerRequestMessage workerRequestMessage)
        {
            WorkerRequests.Enqueue(workerRequestMessage);
            return Ok();
        }


        [HttpGet]
        [Route("workerQueue/dequeue")]
        public async Task<ActionResult> DequeueWorkerMessage()
        {
            var res=   WorkerRequests.TryDequeue(out var result);
            return Ok(result);
        }
        
        [HttpGet]
        [Route("workerQueue/count")]
        public async Task<ActionResult> WorkerMessageCount()
        {
           
            return Ok(WorkerRequests.Count);
        }

        [HttpGet]
        [Route("workerQueue/oldestMessageWaitingTimeInSeconds")]
        public async Task<ActionResult> OldestMessageWaitingTimeInSeconds()
        {
            var gotMessage = WorkerRequests.TryPeek(out WorkerRequestMessage message);
            if (gotMessage)
            {
                var timeWaiting = (DateTime.UtcNow - message.StartTime).TotalSeconds;
                return Ok(timeWaiting);
            }
            return Ok(0.0);

        }

        [HttpPost]
        [Route("completedQueue/enqueue")]
        public async Task<ActionResult> EnqueueCompletedMessage([FromBody] ComputedMessage computedMessage)
        {
            CompletedRequests.Enqueue(computedMessage);
            return Ok();
        }


        [HttpGet]
        [Route("completedQueue/dequeue")]
        public async Task<ActionResult> DequeueCompletedMessage()
        {
            var res = CompletedRequests.TryDequeue(out var result);
            return Ok(result);
        }

        [HttpGet]
        [Route("completedQueue/count")]
        public async Task<ActionResult> CompletedCount()
        {

            return Ok(CompletedRequests.Count);
        }
    }
}