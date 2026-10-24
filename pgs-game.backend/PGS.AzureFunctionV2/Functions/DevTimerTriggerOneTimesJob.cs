using System;
using example.invalid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace target.AzureFunctionV2
{
    public class DevTimerTriggerOneTimesJob
    {
        private readonly ILogger _logger;

        public DevTimerTriggerOneTimesJob(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DevTimerTriggerOneTimesJob>();
        }

        [Function("DevTimerTriggerOneTimesJob")]
        public void Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            string apiDomain = "https://example.invalid";
            string targetUrl = apiDomain + "/jobs/one-times?v=v20409";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(targetUrl).Result;
                string responseBody = response.Content.ReadAsStringAsync().Result;
            }
        }
    }
}
