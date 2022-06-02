using System.Net;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace QueueServiceApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QueueServiceApiController : ControllerBase
    {

        private readonly ILogger<QueueServiceApiController> _logger;
        private readonly IWorkerQueueService _workerQueueService;

        public QueueServiceApiController(ILogger<QueueServiceApiController> logger,
            IWorkerQueueService workerQueueService)
        {
            _logger = logger;
            _workerQueueService = workerQueueService;
        }

        [HttpGet(Name = "pullCompleted")]
        public async Task<ActionResult> Get(int numberOfCompletedRequests)
        {

            if (numberOfCompletedRequests < 1)
            {
                return BadRequest("minimum items to dequeue is 1");
            }

            try
            {
                var queueResponse = await _workerQueueService.DequeueCompletedMessagesAsync(numberOfCompletedRequests);
                if (queueResponse.Count == 0)
                {
                    return NoContent();
                }
                return Ok(queueResponse);
            }
            catch (Exception e)
            {
                var message = "failed to get pullCompleted, response: " + e.Message;
                return new ObjectResult($"{message}") { StatusCode = (int)HttpStatusCode.InternalServerError };
            }

        }

        [HttpPost(Name = "enqueue")]
        public async Task<ActionResult> EnqueueWorkerMessage([FromBody] HashRequest hashRequest)
        {
            var queueResponse = await _workerQueueService.EnqueueMessageAsync(hashRequest);

            if (queueResponse.WorkerQueueResponse.IsSuccessStatusCode)
            {
                return Ok($"enqueue request completed,taskID = {queueResponse.TaskID}");
            }
            else
            {
                var message = queueResponse?.WorkerQueueResponse.Content != null ? await queueResponse.WorkerQueueResponse.Content.ReadAsStringAsync() : string.Empty;


                if (queueResponse != null)
                    return new ObjectResult($"{message}") { StatusCode = (int)queueResponse.WorkerQueueResponse.StatusCode };
            }
            return new ObjectResult($"failed to enqueue message") { StatusCode = (int)HttpStatusCode.InternalServerError };
        }
    }
}