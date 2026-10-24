using System;
using example.invalid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace target.AzureFunctionV2
{
    public class TimerTriggerFetchAccountingFunky
    {
        private readonly ILogger _logger;

        public TimerTriggerFetchAccountingFunky(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TimerTriggerFetchAccountingFunky>();
        }

        [Function("TimerTriggerFetchAccountingFunky")]
        public void Run([TimerTrigger("0 30 0 * * *")]TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            string queryString = $"?gameVendorID=250000&isAsync=true&{AppHelper.GetJobAPIPasswordParameter()}";
            string apiUrl = $"/GameVendor/Job/FetchAccountingReport" + queryString;

            EazyRequestHelper.SendHttpRequest(AppHelper.GetJobAPIUrl() + apiUrl);
            EazyRequestHelper.SendHttpRequest(AppHelper.GetJobAPIUrl_BWG() + apiUrl);
        }
    }
}
