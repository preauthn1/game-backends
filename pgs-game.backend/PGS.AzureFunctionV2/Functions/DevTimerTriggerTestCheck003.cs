using System;
using example.invalid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace target.AzureFunctionV2
{
    public class DevTimerTriggerTestCheck003
    {
        private readonly ILogger _logger;

        public DevTimerTriggerTestCheck003(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DevTimerTriggerTestCheck003>();
        }

        [Function("DevTimerTriggerTestCheck003")]
        public void Run([TimerTrigger("0 30 1 * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            string apiDomain = "https://example.invalid";
            string targetUrl = apiDomain + "/task/CheckUnknowUserAndAutoSetTag";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(targetUrl).Result;
                string responseBody = response.Content.ReadAsStringAsync().Result;
            }
        }
    }
}
