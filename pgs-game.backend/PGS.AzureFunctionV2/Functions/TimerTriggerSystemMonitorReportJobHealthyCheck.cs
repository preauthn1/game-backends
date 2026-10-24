using System;
using example.invalid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace target.AzureFunctionV2
{
    public class TimerTriggerSystemMonitorReportJobHealthyCheck
    {
        private readonly ILogger _logger;

        public TimerTriggerSystemMonitorReportJobHealthyCheck(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TimerTriggerSystemMonitorReportJobHealthyCheck>();
        }

        [Function("TimerTriggerSystemMonitorReportJobHealthyCheck")]
        public void Run([TimerTrigger("0 0 * * * *")]TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            string queryString = $"?v=202410&{AppHelper.GetJobAPIPasswordParameter()}";
            string apiUrl = $"/system-monitor/report-job/healthy" + queryString;

            EazyRequestHelper.SendHttpRequest(AppHelper.GetJobAPIUrl() + apiUrl);
            EazyRequestHelper.SendHttpRequest(AppHelper.GetJobAPIUrl_BWG() + apiUrl);
        }
    }
}
