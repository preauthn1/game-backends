using System;
using example.invalid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;


namespace target.AzureFunctionV2
{
    public class TimerTriggerFetchAccountingJili
    {
        private readonly ILogger _logger;

        public TimerTriggerFetchAccountingJili(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TimerTriggerFetchAccountingJili>();
        }

        [Function("TimerTriggerFetchAccountingJili")]
        public void Run([TimerTrigger("0 30 0 * * *")]TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            string queryString = $"?gameVendorID=390000&isAsync=true&{AppHelper.GetJobAPIPasswordParameter()}";
            string apiUrl = $"/GameVendor/Job/FetchAccountingReport" + queryString;

            EazyRequestHelper.SendHttpRequest(AppHelper.GetJobAPIUrl() + apiUrl);
        }
    }
}
