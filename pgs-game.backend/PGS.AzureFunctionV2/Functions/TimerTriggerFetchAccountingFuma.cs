using System;
using example.invalid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace target.AzureFunctionV2
{
    public class TimerTriggerFetchAccountingFuma
    {
        private readonly ILogger _logger;

        public TimerTriggerFetchAccountingFuma(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TimerTriggerFetchAccountingFuma>();
        }

        [Function("TimerTriggerFetchAccountingFuma")]
        public void Run([TimerTrigger("0 30 0 * * *")]TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            string queryString = $"?gameVendorID=630000&isAsync=true&{AppHelper.GetJobAPIPasswordParameter()}";
            string apiUrl = $"/GameVendor/Job/FetchAccountingReport" + queryString;

            EazyRequestHelper.SendHttpRequest(AppHelper.GetJobAPIUrl_BWG() + apiUrl);
        }
    }
}
