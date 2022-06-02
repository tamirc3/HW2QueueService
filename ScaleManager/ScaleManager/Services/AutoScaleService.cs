using Model;
using Newtonsoft.Json;

namespace AutoScaleService.Services
{
    public class AutoScaleService
    {
        private readonly AppServiceManager _appServiceManager;

        public AutoScaleService()
        {
            _appServiceManager = new AppServiceManager();
        }
        public async Task AutoScaleCheck()
        {
            while (true)
            {
                var queueStatics = await GetQueueStatics();

                if (queueStatics.TooManyLongWaitingMessages())
                {
                    _appServiceManager.CreateAppService();
                    await Task.Delay(60 * 1000); // let give the new app service some time to work
                }
                else if (queueStatics.MessagesAreProcessedFastEnough())
                {
                    _appServiceManager.DeleteAppService();
                }
            }
        }

        private static async Task<QueueStatics> GetQueueStatics()
        {
            QueueStatics queueStatics = new QueueStatics();

            int seconds = 0;
            while (seconds <= 60 || queueStatics.TooManyLongWaitingMessages())
            {
                seconds++;

                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(QueueUrlConsts.requestsQueue_oldestMessageWaitingTime_url);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var oldestMessageTime = int.Parse(responseContent);

                    if (oldestMessageTime >= QueueStatics.HighWaitingTime)
                    {
                        queueStatics.HighThresholdCount++;
                    }
                    else
                    {
                        queueStatics.LowThresholdCount++;
                    }

                }
            }
            return queueStatics;
        }



        public class QueueStatics
        {
            public int HighThresholdCount { get; set; }
            public int LowThresholdCount { get; set; }

            private const int HighThreshold = 20;
            private const int LowThreshold = 45;

            public static double HighWaitingTime = 3.0;
            public static double LowWaitingTime = 1.0;

            public bool TooManyLongWaitingMessages()
            {
                return HighThresholdCount >= HighThreshold;
            }
            public bool MessagesAreProcessedFastEnough()
            {
                return LowThresholdCount >= LowThreshold;
            }
        }
    }
}
