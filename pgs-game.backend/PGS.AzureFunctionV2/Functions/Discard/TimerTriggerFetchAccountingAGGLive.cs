using System;
using example.invalid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;


namespace target.AzureFunctionV2.Functions
{
    public class TimerTriggerFetchAccountingAGGLive
    {
        private readonly ILogger _logger;

        public TimerTriggerFetchAccountingAGGLive(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TimerTriggerFetchAccountingAGGLive>();
        }

        [Function("TimerTriggerFetchAccountingAGGLive")]
        public void Run([TimerTrigger("0 30 14 * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            string apiDomain = AppHelper.GetJobAPIUrl();
            string targetUrl = apiDomain + "/GameVendor/Job/FetchAccountingReport?gameVendorID=330000&isAsync=true";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(targetUrl).Result;
                string responseBody = response.Content.ReadAsStringAsync().Result;
            }

        }
    }
}
