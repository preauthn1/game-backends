using System;
using example.invalid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;


namespace target.AzureFunctionV2
{
    public class TimerTriggerCommonRoutineJob_30Minutes
    {
        private readonly ILogger _logger;

        public TimerTriggerCommonRoutineJob_30Minutes(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TimerTriggerCommonRoutineJob_30Minutes>();
        }

        [Function("TimerTriggerCommonRoutineJob_30Minutes")]
        public void Run([TimerTrigger("0 */30 * * * *")]TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            string queryString = $"?v=202410&{AppHelper.GetJobAPIPasswordParameter()}";
            string apiUrl = "/common-jobs/30-minutes-routine" + queryString;

            EazyRequestHelper.SendHttpRequest(AppHelper.GetJobAPIUrl() + apiUrl);
            EazyRequestHelper.SendHttpRequest(AppHelper.GetJobAPIUrl_BWG() + apiUrl);
        }
    }
}
