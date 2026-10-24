using System;
using example.invalid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace target.AzureFunctionV2
{
    public class TimerTriggerTGRtpWarningNotify
    {
        private readonly ILogger _logger;

        public TimerTriggerTGRtpWarningNotify(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TimerTriggerTGRtpWarningNotify>();
        }

        [Function("TimerTriggerTGRtpWarningNotify")]
        public void Run([TimerTrigger("0 0 1 * * *")]TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            string queryString = $"?v=202410&{AppHelper.GetJobAPIPasswordParameter()}";
            string apiUrl = $"/TG/statistic-notify/game-vendor-rtp-unexpected" + queryString;

            EazyRequestHelper.SendHttpRequest(AppHelper.GetJobAPIUrl() + apiUrl);
            EazyRequestHelper.SendHttpRequest(AppHelper.GetJobAPIUrl_BWG() + apiUrl);
        }
    }
}
