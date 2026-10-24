using Microsoft.AspNetCore.Mvc;
using Nethereum.Web3;
using target.Models.Models;
using target.Utility;
using target.Utility.Utility.ThirdParty;
using target.Web.Modules.Filter;
using PgsTokenContract.Models;
using PgsTokenContract.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using example.invalid;
using System.Linq;
using example.invalid.Http;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace target.API.AdminSys.Controllers
{

    public class TestCaseController : BaseController
    {
        public TokenContractService ContractService = new TokenContractService();

        public TestCaseService TestCaseService = new TestCaseService();

        public BoardGameBetService BoardGameBetService = new BoardGameBetService();


        [Route("debug/001")]
        [HttpGet , HttpPost]
        public object GetDebugInfo()
        {
            //var qqqq = TestCaseService.Testtttt();

            MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
            {
                Message = "qqqqqqq",
                TelegramChannelEnum = TelegramChannelEnum.pgs_contract_notify
            });

            throw new TerproException(ApiErrorCode.NotImplement);

            //var config = ContractConfig.DragonTigerLogicContract;
            //var accountInfo = ContractConfig.AuthorizeSender["0x6212a8a54F7Da4F2CdAF3416bAa1934BB306123C"];
            //var account = new Nethereum.Web3.Accounts.Account(accountInfo.SenderPrivateKey, config.ChainID);
            //var web3 = new Web3(account, config.ProviderURL);

            //var contract = web3.Eth.GetBalance.SendRequestAsync("0x6212a8a54F7Da4F2CdAF3416bAa1934BB306123C", null).Result;
            //.SendRequestAsync<string>("0x6212a8a54F7Da4F2CdAF3416bAa1934BB306123C", null).Result;

            return new
            {
                code = 0,
                msg = "",
                result = ""
            };
        }


        [Route("debug/dragonTiger")]
        [HttpGet]
        public object GetDragonTigerDebugInfo()
        {
            var cloneRoundList = BoardGameBetService.RoundList.ToList();
            cloneRoundList.Reverse();

            return new
            {
                code = 0,
                msg = "",
                result = cloneRoundList.Take(20).ToList(),
            };
        }


        [Route("debug/dev-debug-info")]
        [HttpGet]
        public object GetDevDebugInfo()
        {
            List<object> listOfRecords = new List<object>();

            foreach(var item in BaseService.AddressTransactionRecordsMappings)
            {
                var records = new
                {
                    addresss = item.Key,
                    topRecords = item.Value.OrderByDescending(x => x.Time).Take(10).ToList()
                };

                listOfRecords.Add(records);
            }


            //BaseService

            return new
            {
                code = 0,
                msg = "",
                info1 = listOfRecords,
                info2 = BaseService.lastTransactionResult,
                info3 = BaseService.DebugHistoryQueue.Select(x=> x).ToList(),
            };
        }

    }
}
