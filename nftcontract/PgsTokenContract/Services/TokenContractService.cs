using Nethereum.ABI;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.TransactionManagers;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using target.Utility;
using target.Utility.Utility;
using PgsTokenContract.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using target.Utility.Extensions;

namespace PgsTokenContract.Services
{
    public partial class TokenContractService : BaseService
    {

        public object GetDebugInfo()
        {
            var config = ContractConfig.PgsTokenContract;
            return new
            {
                ChainID = config.ChainID,
                ProviderURL = config.ProviderURL,
                ContractAddress = config.ContractAddress,
                ExploreUrls = new List<string>
                {
                    "https://example.invalid/"
                }
            };
        }


        public BalanceResultViewModel GetBalance(string address)
        {
            var config = ContractConfig.PgsTokenContract;

            var result = Eth_CallMethod<JsonRPCStringResultModel>(config, "balanceOf", new List<object>
            {
                address
            });

            return new BalanceResultViewModel
            {
                Address = address,
                Balance = ConvertHexToNumber(result.result)
            };
        }


        public CommonTransactionHashResultViewModel BindAddressAndSendFirstTimeBonus(string address)
        {
            new { address = address }.RuleFor(x => x.address).Rule_IsValidHashAddress();

            var config = ContractConfig.PgsTokenContract;

            var firstBonusBindCheck = Eth_CallMethod<JsonRPCStringResultModel>(config, "isAlreadySendFirstBonus", new List<object>
            {
                address
            });

            //已經綁定過首次獎勵的話 就不重複處理
            if (firstBonusBindCheck == null || firstBonusBindCheck.resultOfBoolean)
                return new CommonTransactionHashResultViewModel();

            long bonusTokenValue = 225000;
            string methodName = "firstBindSendBonus";
            List<object> parameters = new List<object>
            {
                address, 
                bonusTokenValue
            };

            var sign = GenerateSignAndNonceByDefaultAddress(new GenerateSignBundleParametersModel(config, methodName, parameters));
            var result = Eth_SendRawTransaction(config.ProviderURL, sign);

            return new CommonTransactionHashResultViewModel
            {
                TransactionHash = result.result
            };
        }


        public CommonTransactionHashResultViewModel MintBalanceForTargetAddress(string address , decimal amount = 100000)
        {
            new { address = address }.RuleFor(x => x.address).Rule_IsValidHashAddress();

            var config = ContractConfig.PgsTokenContract;

            string methodName = "mint";
            List<object> parameters = new List<object>
            {
                address,
                amount
            };

            var sign = GenerateSignAndNonceByDefaultAddress(new GenerateSignBundleParametersModel(config, methodName, parameters));
            var result = Eth_SendRawTransaction(config.ProviderURL, sign);
            return new CommonTransactionHashResultViewModel
            {
                TransactionHash = result.result
            };
        }



        public JsonRPCReceiptResultModel GetTransactionResultByTxHash(string txHash)
        {
            var config = ContractConfig.PgsTokenContract;
            var result = Eth_GetTransactionReceipt(config.ProviderURL, txHash);
            return result;
        }


      

    }
}
