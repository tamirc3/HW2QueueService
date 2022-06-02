namespace WorkerNode
{
    public interface IWorkerManager
    {
        void StartWorking();
        bool IsBusy();
        void StopWorkers();
    }

    public class WorkerManager : IWorkerManager
    {
        private readonly List<WorkerNode> _workers;
        private readonly string requestQueueUrl_dequeue;
        private readonly string completedQueueUrl_enqueue;
        public WorkerManager(string requestQueueUrlDequeue, string completedQueueUrlEnqueue)
        {
            requestQueueUrl_dequeue = requestQueueUrlDequeue;
            completedQueueUrl_enqueue = completedQueueUrlEnqueue;
            _workers = new List<WorkerNode>();
            CreateWorkerTasks();
        }

        private void CreateWorkerTasks()
        {
            var numberOfWorkersTasks = Environment.ProcessorCount;
            for (int i = 0; i < numberOfWorkersTasks; i++)
            {
                _workers.Add(new WorkerNode(requestQueueUrl_dequeue, completedQueueUrl_enqueue));
            }
        }

        public void StartWorking()
        {
            _workers.ForEach(node => node.Start());
        }
        public bool IsBusy()
        {
            return _workers.Any(node => node.IsBusy);
        }

        public void StopWorkers()
        {
            _workers.ForEach(node => node.StopRunning());
        }

    }
}
