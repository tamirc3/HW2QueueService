using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace QueueServiceApi.Controllers
{
    [ApiController]
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

        [HttpGet]
        [Route("queueServiceApi/pullCompleted")]
        public async Task<ActionResult> Get(int top)
        {

            if (top < 1)
            {
                return BadRequest("minimum items to dequeue is 1");
            }

            try
            {
                var queueResponse = await _workerQueueService.DequeueCompletedMessagesAsync(top);
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

        [HttpPost]
        [Route("queueServiceApi/enqueue")]
        public async Task<ActionResult> EnqueueWorkerMessage([FromBody] HashRequest hashRequest)
        {
            var queueResponse = await _workerQueueService.EnqueueMessageAsync(hashRequest);

            if (queueResponse.WorkerQueueResponse.IsSuccessStatusCode)
            {
                return Ok($"{queueResponse.TaskID}");
            }

            var message = queueResponse?.WorkerQueueResponse.Content != null ? await queueResponse.WorkerQueueResponse.Content.ReadAsStringAsync() : string.Empty;

            if (queueResponse != null)
                return new ObjectResult($"{message}") { StatusCode = (int)queueResponse.WorkerQueueResponse.StatusCode };
            return new ObjectResult($"failed to enqueue message") { StatusCode = (int)HttpStatusCode.InternalServerError };
        }
    }
}