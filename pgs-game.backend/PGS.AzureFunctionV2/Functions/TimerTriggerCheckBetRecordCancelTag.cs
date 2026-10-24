using System;
using example.invalid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace target.AzureFunctionV2
{
    public class TimerTriggerCheckBetRecordCancelTag
    {
        private readonly ILogger _logger;

        public TimerTriggerCheckBetRecordCancelTag(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TimerTriggerCheckBetRecordCancelTag>();
        }

        [Function("TimerTriggerCheckBetRecordCancelTag")]
        public void Run([TimerTrigger("0 0 */8 * * *")]TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            string queryString = $"?v=202410&{AppHelper.GetJobAPIPasswordParameter()}";
            string apiUrl = "/GameVendor/Job/CheckCancelTags" + queryString;

            EazyRequestHelper.SendHttpRequest(AppHelper.GetJobAPIUrl() + apiUrl);
            EazyRequestHelper.SendHttpRequest(AppHelper.GetJobAPIUrl_BWG() + apiUrl);
        }
    }
}
