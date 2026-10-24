using Nethereum.ABI;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.TransactionManagers;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using target.Utility.Helper;
using target.Utility.Utility;
using PgsTokenContract.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using target.Utility.Extensions;
using target.Models.Models;
using static PgsTokenContract.Services.TokenContractService;
using target.Utility;
using target.Models;
using target.Utility.Extensions;
using System.Text;
using System.Web;
using System.Collections.Specialized;
using target.Models.ViewModels.Base;
using Microsoft.Extensions.Hosting;
using System.Threading;
using target.Utility.Utility.ThirdParty;

namespace PgsTokenContract.Services
{
    public class HostRoutineService :IHostedService, IDisposable
    {

        private Timer timer;
        private int number;
        private static int triggerTimes = 0;


        public void Dispose()
        {
            timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(o =>
            {
                Interlocked.Increment(ref number);
                triggerTimes++;

                var service = new BoardGameBetService();
                service.CreateRoundAndAutoSettle(new CreateRoundParameterModel
                {
                    IsCheckEnableFlag = true,
                    RoundSettleSeconds = SystemConstant.DefaultRoundSettleSeconds,
                    IsSendNotifyToInternalChannel = triggerTimes <= 5 ? true : false
                });
            },

            null,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(SystemConstant.DefaultRoundSettleSeconds)); //排程間隔

            return Task.CompletedTask;
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }


    }
}
