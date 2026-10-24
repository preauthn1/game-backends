using System;
using example.invalid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace target.AzureFunctionV2
{
    public class TimerTriggerForceAGGBetRecordSettle
    {
        private readonly ILogger _logger;

        public TimerTriggerForceAGGBetRecordSettle(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TimerTriggerForceAGGBetRecordSettle>();
        }

        [Function("TimerTriggerForceAGGBetRecordSettle")]
        public void Run([TimerTrigger("0 20 * * * *")]TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            string queryString = $"?v=202410&{AppHelper.GetJobAPIPasswordParameter()}";
            string apiUrl = $"/bet-record/agg/trigger-settle" + queryString;

            EazyRequestHelper.SendHttpRequest(AppHelper.GetJobAPIUrl() + apiUrl);

        }
    }
}
