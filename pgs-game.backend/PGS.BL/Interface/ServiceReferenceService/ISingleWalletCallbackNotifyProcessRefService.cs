using System;
using System.Collections.Generic;
using game.Entity.Game;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;

namespace target.BL.Interface
{

    public interface ISingleWalletCallbackNotifyProcessRefService
    {

        SingleWalletModePlaceBetApiResponseModel PlaceBetProcessCallbackMerchantApi(int merchantID, SingleWalletModePlaceBetParameter model);

        SingleWalletModeSettleApiResponseModel SettlemenetProcessCallbackMerchantApi(SingleWalletSettleCallbackParameter model);

        SingleWalletModeSettleApiResponseModel BonusProcessCallbackMerchantApi(SingleWalletSettleCallbackParameter model);

        SingleWalletModeSettleApiResponseModel CancelProcessCallbackMerchantApi(SingleWalletModeCancelApiParameter model);

        SingleWalletTransferApiPlatformResponse SendEventPrizeCallbackMerchantApi(SingleWalletCallbackSendEventBonusParameter model);

        SingleWalletTransferApiPlatformResponse Transfer_TransferToGameVendorWalletCallbackMerchantApi(int merchantID, SingleWalletTransferApiParameter model);

        SingleWalletTransferApiPlatformResponse Transfer_TransferBalanceToPlatformWalletCallbackMerchantID(int merchantID, SingleWalletTransferApiParameter model);

        SingleWalletModeSettleApiResponseModel ChangeSettleInfoProcessPassToMerchantApi(SingleWalletChangeSettleInfoApiParameter model);

        /// <summary>
        /// Job API會呼叫
        /// </summary>
        SingleWalletModeSettleApiResponseModel PayoutProcessWithDifferentApiLogic(SingleWalletSettleCallbackParameter model);

        void CancelProcessWithDifferentApiLogic(SingleWalletModeCancelApiParameter model);
    }
}
