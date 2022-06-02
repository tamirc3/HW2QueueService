using Microsoft.AspNetCore.Mvc;

namespace WorkerNode.Controllers
{
    [ApiController]
    public class WorkersAvailability : ControllerBase
    {
        readonly IWorkerManager _workerManager;

        public WorkersAvailability(IWorkerManager workerManager)
        {
            _workerManager = workerManager;
        }
        [HttpPut]
        [Route("/stopWorking")]
        public ContentResult StopWorking()
        {
            _workerManager.StopWorkers();
            return new ContentResult() { StatusCode = 200 };
        }

        [HttpGet]
        [Route("/workerIsBusy")]
        public ContentResult WorkerIsBusy()
        {

            return new ContentResult() { Content = _workerManager.IsBusy().ToString(),StatusCode = 200 };
        }
    }
}