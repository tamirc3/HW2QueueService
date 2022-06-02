namespace Model
{
    public class QueueUrlConsts
    {
        //localHost
        public const string LocalHost_QueueServiceApi = "https://localhost:7145";
        public const string LocalHost_Queue = "https://localhost:7108";

        //api consts
        public const string QueueServiceApi_pullCompleted = "/queueServiceApi/pullCompleted";
        public const string QueueServiceApi_enqueue = "/queueServiceApi/enqueue";

        //request queue
        public const string requestsQueue_enqueue_url = "/Queue/workerQueue/enqueue";
        public const string requestsQueue_dequeue_url = "/Queue/workerQueue/dequeue";
        public const string requestsQueue_oldestMessageWaitingTime_url = "/Queue/workerQueue/oldestMessageWaitingTimeInSeconds";

        //completed queue
        public const string completedQueue_enqueue_url = "/Queue/completedQueue/enqueue";
        public const string completedQueue_dequeue_url = "/Queue/completedQueue/dequeue";

    }
}
