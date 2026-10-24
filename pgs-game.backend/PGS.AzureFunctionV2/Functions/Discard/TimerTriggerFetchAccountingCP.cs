using System;
using example.invalid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace target.AzureFunctionV2
{
    public class TimerTriggerFetchAccountingCP
    {
        private readonly ILogger _logger;

        public TimerTriggerFetchAccountingCP(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TimerTriggerFetchAccountingCP>();
        }

        [Function("TimerTriggerFetchAccountingCP")]
        public void Run([TimerTrigger("0 30 0 * * *")]TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            string apiUrl = "/GameVendor/Job/FetchAccountingReport?gameVendorID=570000&isAsync=true";

            EazyRequestHelper.SendHttpRequest(AppHelper.GetJobAPIUrl_BWG() + apiUrl);
        }
    }
}
