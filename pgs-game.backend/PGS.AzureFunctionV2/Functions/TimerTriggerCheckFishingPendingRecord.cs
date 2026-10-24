using System;
using example.invalid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;


namespace target.AzureFunctionV2
{
    public class TimerTriggerCheckFishingPendingRecord
    {
        private readonly ILogger _logger;

        public TimerTriggerCheckFishingPendingRecord(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TimerTriggerCheckFishingPendingRecord>();
        }

        [Function("TimerTriggerCheckFishingPendingRecord")]
        public void Run([TimerTrigger("0 0 */2 * * *")]TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            string queryString = $"?v=202410&isAsync=true&{AppHelper.GetJobAPIPasswordParameter()}";
            string apiUrl = "/bet-record/fishing/check-pending" + queryString;

            EazyRequestHelper.SendHttpRequest(AppHelper.GetJobAPIUrl() + apiUrl);
        }
    }
}
