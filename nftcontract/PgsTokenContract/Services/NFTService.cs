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
using target.Models.ViewModels.Base;

namespace PgsTokenContract.Services
{
    public class NFTService : BaseService
    {

        private const string SALT_KEY = "e385027b060b";


        public NFTInfoViewModel GetNFTInfo(int tokenID)
        {
            var result = GetNFTInfoByContractConfig(tokenID , ContractConfig.NFTContract);
            return result;
        }

        public NFTInfoViewModel GetNFTInfoByContractConfig(int tokenID , ContractConfigModel config)
        {
            new { tokenID = tokenID }.RuleFor(x => x.tokenID).Rule_NotMinus();

            string methodName = "getNFTGameSettingsV2";
            var result = Eth_CallMethod<JsonRPCStringResultModel>(config, methodName, new List<object> { tokenID.ToString() });

            var parseModel = ParseQueryStringTextToModel<NFTGameSettingModel>(result.result);

            return new NFTInfoViewModel
            {
                Name = parseModel.Name,
                RTP = parseModel.RTP,
                OwnerAddress = parseModel.Owner,
                TokenID = tokenID
            };
        }

        static List<string> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize))
                .ToList();
        }

        public NFTMintResultViewModel MintNFT(NFTMintRequestModel model)
        {
            model.RuleFor(x => x.Name).Rule_NotEmptyText();
            model.RuleFor(x => x.OwnerAddress).Rule_NotEmptyText().Rule_IsValidHashAddress();
            model.RuleFor(x => x.RTP).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.TargetContractAddress).Rule_NotEmptyText();

            //model.RuleFor(x => x.MagicPassword).Rule_NotEmptyText();

            //if (model.MagicPassword != SystemConstant.CheckCode_MagicPassword)
            //    throw new TerproException(ApiErrorCode.ThirdPartyApiFail_SignCheckNotMatch);


            ContractConfigModel targetConfig = null;

            if (model.TargetContractAddress == ContractConfig.NFTContract.ContractAddress)
            {
                targetConfig = ContractConfig.NFTContract;         
            }
            else if(model.TargetContractAddress == ContractConfig.NFTContractDragonTiger.ContractAddress)
            {
                targetConfig = ContractConfig.NFTContractDragonTiger;
            }
            else
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, "查無對應的NFT合約");
            }

            string methodName = "mint";
            var sign = GenerateSignAndNonceByQueue(new GenerateSignBundleParametersModel(targetConfig, methodName, new List<object> { model.Name, model.RTP, model.OwnerAddress }));
            var result = Eth_SendRawTransaction(targetConfig.ProviderURL, sign);

            return new NFTMintResultViewModel
            {
                Result = result
            };
        }


    }
}
