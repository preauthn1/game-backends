using System;
using example.invalid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace target.AzureFunctionV2
{
    public class DevTimerTriggerTestCheck002
    {
        private readonly ILogger _logger;

        public DevTimerTriggerTestCheck002(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DevTimerTriggerTestCheck002>();
        }

        /// <summary>
        /// 這時間是UTC+0 為基準。因此+8台灣時間 可能會
        /// </summary>
        [Function("DevTimerTriggerTestCheck002")]
        public void Run([TimerTrigger("0 */30 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            string apiDomain = "https://example.invalid";
            string targetUrl1 = apiDomain + "/CheckPVP/all-players/latest";
            string targetUrl2 = apiDomain + "/CheckPVP/all-players/latest/Shuffle";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(targetUrl1).Result;
                string responseBody = response.Content.ReadAsStringAsync().Result;
            }

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(targetUrl2).Result;
                string responseBody = response.Content.ReadAsStringAsync().Result;
            }
        }
    }
}
