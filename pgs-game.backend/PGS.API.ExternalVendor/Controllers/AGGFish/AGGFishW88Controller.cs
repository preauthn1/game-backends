using Microsoft.AspNetCore.Mvc;
using target.Models.Enums;
using target.Models.ViewModels;
using target.Models.ViewModels.GameVendor;
using target.Utility.Extensions;
using target.Web.Modules.Filter;

namespace target.API.ExternalVendor.Controllers
{

    public partial class AGGFishController : BaseGameVendorController
    {

        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.SingleWalletApi_AGGFish)]
        [Route("aggFish/wallet/balance")]
        [HttpPost]
        public AGGApiCommonResponse<AGGFishW88BalanceApiDetailDataModel> GetW88WalletBalance([FromBody] AGGCommonHashRequestModel<AGGFishW88BalanceApiRequestaModel> model)
        {
            var data = model.data;

            data.RuleFor(x => x.Token).Rule_NotEmptyText();

            //step 1.先檢查session是否合法
            var checkResult = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                SessionOrToken = [REDACTED],
                EncryptSign = model.hash
            });

            //step 2. 轉帳邏輯
            var result = W88ExtraProcessService.GetW88Balance(checkResult.PlayerID);

            return InstanceAGGCommonResponse(result);
        }


        /// <summary>
        /// 將平台幣轉至捕魚錢包
        /// </summary>
        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.SingleWalletApi_AGGFish)]
        [Route("aggFish/wallet/deposit")]
        [HttpPost]
        public AGGApiCommonResponse<AGGFishTransferApiDetailDataModel> DepositToFishWallet([FromBody] AGGCommonHashRequestModel<AGGFishTransferApiRequestaModel> model)
        {    
            //模擬錯誤發生 (Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_PlaceBet);

            var data = model.data;

            data.RuleFor(x => x.Token).Rule_NotEmptyText();

            //step 1.先檢查session是否合法
            var checkResult = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                SessionOrToken = [REDACTED],
                EncryptSign = model.hash
            });

            //step 2. 轉帳邏輯
            var result = W88ExtraProcessService.GameWalletTransfer(WalletTransferTypeEnum.Transfer_In, checkResult.PlayerID, model.data);

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_PlaceBet_SuccessUpdateBut_DelayTimeout);

            return InstanceAGGCommonResponse(result);
        }

        /// <summary>
        /// 將捕魚錢包扣除 轉回平台
        /// </summary>
        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.SingleWalletApi_AGGFish)]
        [Route("aggFish/wallet/withdrawal")]
        [HttpPost]
        public AGGApiCommonResponse<AGGFishTransferApiDetailDataModel> WithdrawalFromFishWallet([FromBody] AGGCommonHashRequestModel<AGGFishTransferApiRequestaModel> model)
        {  
            //模擬錯誤發生 (Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_PlaceBet);

            var data = model.data;

            data.RuleFor(x => x.Token).Rule_NotEmptyText();

            //step 1.先檢查session是否合法
            var checkResult = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                SessionOrToken = [REDACTED],
                EncryptSign = model.hash
            });

            //step 2. 轉帳邏輯
            var result = W88ExtraProcessService.GameWalletTransfer(WalletTransferTypeEnum.Transfer_Out, checkResult.PlayerID, model.data);

            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_PlaceBet_SuccessUpdateBut_DelayTimeout);
            return InstanceAGGCommonResponse(result);
        }



        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.SingleWalletApi_AGGFish)]
        [Route("aggFish/users/bonus-record")]
        [HttpPost]
        public AGGApiCommonResponse<AGGFishTxtIDResultDetailModel> WriteBonusRecord([FromBody] AGGCommonHashRequestModel<AGGFishBonusRecordApiRequestaModel> model)
        { 
            //模擬錯誤發生 (Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_Settle);

            var data = model.data;

            data.RuleFor(x => x.Token).Rule_NotEmptyText();

            //step 1.先檢查session是否合法
            var checkResult = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                SessionOrToken = [REDACTED],
                EncryptSign = model.hash
            });

            //step 2. 轉帳邏輯
            var result = W88ExtraProcessService.WriteBonusRecord(checkResult.PlayerID, model.data);

            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_Settle_SuccessUpdateBut_DelayTimeout);
            return InstanceAGGCommonResponse(result);
        }


        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.SingleWalletApi_AGGFish)]
        [Route("aggFish/users/payment-link")]
        [HttpPost]
        public AGGApiCommonResponse<AGGFishPaymentLinkResultDetailModel> GetPaymentLink([FromBody] AGGCommonHashRequestModel<AGGFishGetPaymentLinkApiRequestaModel> model)
        {
            var data = model.data;

            data.RuleFor(x => x.Token).Rule_NotEmptyText();

            //step 1.先檢查session是否合法
            var checkResult = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                SessionOrToken = [REDACTED],
                EncryptSign = model.hash
            });

            //step 2. 轉帳邏輯
            var result = W88ExtraProcessService.GetPaymentLink(checkResult.PlayerID, model.data);
            return InstanceAGGCommonResponse(result);
        }


        protected AGGApiCommonResponse<T> InstanceAGGCommonResponse<T>(T model) where T: class
        {
            return new AGGApiCommonResponse<T>
            {
                result = model
            };
        }


    }
}
