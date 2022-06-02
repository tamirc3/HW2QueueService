namespace Model
{
    public class QueueUrlConsts
    {

        public const string QueueHost = "https://localhost:7104";
        public const string QueueServiceApiHost = "https://localhost:7104";
        public const string QueueServiceApi_pullCompleted = "/pullCompleted";
        public const string QueueServiceApi_enqueue = "/enqueue";

        //request queue
        public const string requestsQueue_enqueue_url = "/Queue/workerQueue/enqueue";
        public const string requestsQueue_dequeue_url = "/Queue/workerQueue/dequeue";
        public const string requestsQueue_oldestMessageWaitingTime_url = "/Queue/workerQueue/oldestMessageWaitingTimeInSeconds";

        //completed queue
        public const string completedQueue_enqueue_url = "/Queue/completedQueue/enqueue";
        public const string completedQueue_dequeue_url = "/Queue/completedQueue/dequeue";


        public const bool ShouldTakeValuesFromConfig = false;
    }
}
