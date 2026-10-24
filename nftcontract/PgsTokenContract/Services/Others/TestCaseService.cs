using Microsoft.AspNetCore.Http;
using Nethereum.ABI;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.TransactionManagers;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using target.Models;
using target.Models.Models;
using target.Models.ViewModels.Base;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using PgsTokenContract.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PgsTokenContract.Services
{
    public class TestCaseService : BaseService
    {
        public object Testtttt()
        {
            var config = ContractConfig.DragonTigerLogicContract;

            string methodName = "getReferenceContractAddress";
            List<object> parameters = new List<object>
            {

            }; 

            var result = Eth_CallMethod<JsonRPCStringResultModel>(config, methodName, parameters);

            var qqq = RemovePrefix0x_SplitByChunksize(result.result, 64);

            List<string> checkList = new List<string>
            {
                ContractConfig.SlotsLogicContract.ContractAddress,
                ContractConfig.DragonTigerLogicContract.ContractAddress
            };

            foreach(var address in checkList)
            {
                var myAddress = RemoveUselessPrefix000000(address);

                if(checkList.Any(x=> x.Contains(myAddress)))
                {
                    string qqq666 = "";
                }
            }

            return "XXX"; 
        }
    }
}
