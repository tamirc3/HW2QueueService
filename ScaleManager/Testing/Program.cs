//https://localhost:7113/queueServiceApi/enqueue
//body:
//
//{
//  "buffer": "this is binary content of a file",
//  "iterations": 42
//}
//example response:
//723aaa19 - 7141 - 4a8f - a5f2-9293b93436e5

//second get requst example:
//'https://localhost:7113/queueServiceApi/pullCompleted?numberOfCompletedRequests=<numberOfItems>
//second response example:
//["{\"buffer\":\"48BD4FB5935FB1EACA504AA81012955D3CEE8102572E115B69E48C3FDD9B65E051519131F091EB10E82E7BB433FB53CF4F0C9A1B74524AA3B6058FF308E3DEF8\",\"id\":\"723aaa19-7141-4a8f-a5f2-9293b93436e5\"}"]



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;

namespace ConsoleApplication1
{
    class Program
    {
        static List<string> queue = new List<string>();
        static string url = "https://localhost:7113/queueServiceApi/enqueue";
        static string url2 = "https://localhost:7113/queueServiceApi/pullCompleted?numberOfCompletedRequests=1";

        static void Main(string[] args)
        {
            Task producer = Task.Run(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    string response = sendRequest(url, createData()).Result;
                    Console.WriteLine("producer " + response);
                    string id = response.Substring(1, response.Length - 2);
                    queue.Add(id);
                }
            });
                

            Task consumer = Task.Run(async () =>
            {
                while (queue.Count > 0)
                {
                    var response = await sendRequest(url2, "");
                    Console.WriteLine("consumer " + response);
                    string id = response.Substring(1, response.Length - 2);
                    queue.Remove(id);
                }
            });
    

            Console.WriteLine("done");
            Console.ReadLine();
        }

        static async Task<string> sendRequest(string url, string json)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            var content = new StringContent(json);
            HttpResponseMessage response = await client.PostAsync(url,content);
            string responseString = response.Content.ReadAsStringAsync().Result;
            return responseString;
        }
        static string createData()
        {
            string filePath = @"C:\Users\pc\Documents\Arduino\libraries\readme.txt";
            var file = File.ReadAllText(filePath);
            //string fileString = Convert.ToBase64String(file);
            string data = "{\"buffer\": \"" + file + "\", \"iterations\": 42}";
            return data;
        }
    }
}

