using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace TrafficManager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TrafficManagerController : ControllerBase
    {
        private readonly ILoadBalancerClass _loadBalancer;

        public TrafficManagerController(ILoadBalancerClass loadBalancer)
        {
            _loadBalancer = loadBalancer;
        }

        [HttpPut]
        [Route("/enqueue")]
        public async Task<ContentResult> TrafficManagerToEnqueue(int iterations, [FromBody] string dataToDigest)
        {
         
            //Object validation
            if (iterations <= 0 || dataToDigest is null || dataToDigest.Length == 0)
            {
                return new ContentResult() { Content = ErrorConstants.InvalidRequestEnqueue, StatusCode = 400 };
            }

            var client = new HttpClient();
            

            string host = _loadBalancer.GetNodeUrl();
            var requestUri = host + "/queueServiceApi/enqueue";
            Console.WriteLine($"TM routing to {requestUri}");
            HashRequest hashRequest = new HashRequest()
            {
                Buffer = dataToDigest,
                Iterations = iterations
            };
            var dataJson = JsonConvert.SerializeObject(hashRequest);
            var response = await client.PostAsync(requestUri,
                    new StringContent(dataJson, Encoding.UTF8, "application/json"));

            var workId = await response.Content.ReadAsStringAsync();

            return new ContentResult() { Content = workId, StatusCode = (int?)response.StatusCode };
        }

        [HttpPost]
        [Route("/pullCompleted")]
        public async Task<ContentResult> TrafficManagerToDequeue(int top)
        {
            //Object validation
            if (top <= 0)
            {
                return new ContentResult() { Content = ErrorConstants.InvalidRequestDequeue, StatusCode = 400 };
            }

            string host = _loadBalancer.GetNodeUrl();

            var client = new HttpClient();
            var url = host+"/queueServiceApi/pullCompleted" + "?top=" + top;
            Console.WriteLine($"TM routing to {url}");
            var response = await client.GetAsync(url);
            var preparedData = await response.Content.ReadAsStringAsync();
             preparedData = preparedData.Replace("-", string.Empty);

            return new ContentResult()
            { Content = JsonConvert.SerializeObject(preparedData), StatusCode = (int?)response.StatusCode };

        }


    }
}