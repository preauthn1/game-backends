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
using System.Threading;

namespace PgsTokenContract.Services
{
    public partial class SuperAdminService : BaseService
    {
        public void CheckContractAddressMatchAndNotify(ContractConfigModel config)
        {
            string methodName = "getReferenceContractAddress";
            List<object> parameters = new List<object>
            {

            };

            var result = Eth_CallMethod<JsonRPCStringResultModel>(config, methodName, parameters);

            var apiResponseList = RemovePrefix0x_SplitByChunksize(result.result, 64);

            List<string> checkList = new List<string>
            {
                ContractConfig.PgsTokenContract.ContractAddress.ToLower()
            };

            if(config.ContractAddress == ContractConfig.SlotsLogicContract.ContractAddress)
            {
                checkList.Add(ContractConfig.NFTContract.ContractAddress.ToLower());
            }
            else
            {
                checkList.Add(ContractConfig.NFTContractDragonTiger.ContractAddress.ToLower());
            }

            foreach (var address in apiResponseList)
            {
                var myAddress = RemoveUselessPrefix000000(address).ToLower();

                if (!checkList.Any(x => x.Contains(myAddress)))
                {
                    string contractName = config.ABI_FILE_Name.Replace("ABI.json", "").Replace(".json", "");
                    string msg = $"{contractName} 合約內部設置檢查: {myAddress} 並非config定義好的合約地址";
                    SendNotifyToChannel(msg);
                }
            }
        }


        /// <summary>
        /// 修改spin合約中所參考的 tokenERC20合約位址 
        /// </summary>
        public CommonTransactionHashResultViewModel ChangeTokenContractAddressInSlotsContract(LazyCommonRequestModel model)
        {
            model.RuleFor(x => x.Address).Rule_IsValidHashAddress();

            string contractAddress = model.Address;

            var config = model.TargetContractConfig;
            string pgsWagerID = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo();

            string methodName = "superAdminSetTokenContractAddress";
            List<object> parameters = new List<object>
            {
                contractAddress
            };

            var sign = GenerateSignAndNonceByDefaultAddress(new GenerateSignBundleParametersModel(config, methodName, parameters));
            var result = Eth_SendRawTransaction(config.ProviderURL, sign);

            return new CommonTransactionHashResultViewModel
            {
                TransactionHash = result.result
            };

        }


        /// <summary>
        /// 修改spin合約中所參考的 tokenERC20合約位址 
        /// </summary>
        public CommonTransactionHashResultViewModel ChangeNFTContractAddressInSlotsContract(LazyCommonRequestModel model)
        {
            model.RuleFor(x => x.Address).Rule_IsValidHashAddress();

            string contractAddress = model.Address;

            var config = model.TargetContractConfig;
            string pgsWagerID = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo();

            string methodName = "superAdminSetNFTContractAddress";
            List<object> parameters = new List<object>
            {
                contractAddress
            };

            var sign = GenerateSignAndNonceByDefaultAddress(new GenerateSignBundleParametersModel(config, methodName, parameters));
            var result = Eth_SendRawTransaction(config.ProviderURL, sign);

            return new CommonTransactionHashResultViewModel
            {
                TransactionHash = result.result
            };

        }



        /// <summary>
        /// 幾個主要合約中 增加信任白名單錢包位址
        /// </summary>
        public CommonTransactionHashResultViewModel AddAddressToTrustList(LazyCommonRequestModel model)
        {
            model.RuleFor(x => x.Address).Rule_IsValidHashAddress();

            string trustSenderAddress = model.Address;

            List<ContractConfigModel> targetContracts = new List<ContractConfigModel>
            {
                ContractConfig.PgsTokenContract,
                ContractConfig.SlotsLogicContract,
                ContractConfig.DragonTigerLogicContract,
            };

            string resultText = "";
            foreach (var config in targetContracts)
            {
                string pgsWagerID = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo();

                string methodName = "superAdminSetTrustSenderAddress";

                List<object> parameters = new List<object>
                {
                    trustSenderAddress
                };

                var sign = GenerateSignAndNonceByDefaultAddress(new GenerateSignBundleParametersModel(config, methodName, parameters));
                var result = Eth_SendRawTransaction(config.ProviderURL, sign);
                resultText += "、" + result.result + $"{sign.SenderInfo.Nonce}" +  result.error?.message;

                //最後一筆不需要delay
                if(targetContracts.LastOrDefault().ContractAddress != config.ContractAddress)
                {
                    Thread.Sleep(1000 * 30);
                }
            }

            return new CommonTransactionHashResultViewModel
            {
                TransactionHash = resultText
            };

        }

    }
}
