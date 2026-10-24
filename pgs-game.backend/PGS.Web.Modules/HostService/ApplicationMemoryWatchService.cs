using target.DA.Repository;
using target.Utility;
using target.Models.Table;
using System;
using target.BL.Interface;
using target.Utility.Helper;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using target.Utility.Extensions;
using target.Utility.IOC;
using target.Utility.Utility.ThirdParty;

namespace target.BL.Services
{
    public class ApplicationMemoryWatchService : BackgroundService
    {

        private readonly IHostApplicationLifetime _lifetime;

        private const long ThresholdBytes = 3000L * 1024 * 1024; // 3.5GB

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

                var used = Environment.WorkingSet;

                if (used > ThresholdBytes)
                {
                    string myMsg = $"{RuntimeHelper.GetK8sInstanceName()} 記憶體已超標 即將自動關機";

                    MyTelegramUtility.SendMessage(new TelegramMessageParameter
                    {
                        Message = myMsg,
                        TelegramChannelEnum = TelegramChannelEnum.pgs_important_error
                    });


                    _lifetime.StopApplication(); // 觸發 graceful shutdown
                }
            }
        }

    }



}
