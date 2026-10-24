using System;
using example.invalid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;


namespace target.AzureFunctionV2
{
    public class TimerTriggerCheckPendingRecordV3
    {
        private readonly ILogger _logger;

        public TimerTriggerCheckPendingRecordV3(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TimerTriggerCheckPendingRecordV3>();
        }

        [Function("TimerTriggerCheckPendingRecordV3")]
        public void Run([TimerTrigger("0 0 * * * *")]TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            string queryString = $"?v=202410&isAsync=true&{AppHelper.GetJobAPIPasswordParameter()}";
            string apiUrl = "/GameVendor/Job/CheckPendingRecordsAndAutoRefund_V3" + queryString;

            EazyRequestHelper.SendHttpRequest(AppHelper.GetJobAPIUrl() + apiUrl);
        }
    }
}
