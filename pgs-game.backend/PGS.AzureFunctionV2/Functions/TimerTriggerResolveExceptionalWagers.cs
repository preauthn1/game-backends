using System;
using example.invalid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace target.AzureFunctionV2
{
    public class TimerTriggerResolveExceptionalWagers
    {
        private readonly ILogger _logger;

        public TimerTriggerResolveExceptionalWagers(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TimerTriggerResolveExceptionalWagers>();
        }

        [Function("TimerTriggerResolveExceptionalWagers")]
        public void Run([TimerTrigger("0 0 */8 * * *")]TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            string queryString = $"?v=202410&isAsync=true&{AppHelper.GetJobAPIPasswordParameter()}";
            string apiUrl = $"/bet-record/agg/exceptional-wagers" + queryString;

            EazyRequestHelper.SendHttpRequest(AppHelper.GetJobAPIUrl() + apiUrl);
        }
    }
}
