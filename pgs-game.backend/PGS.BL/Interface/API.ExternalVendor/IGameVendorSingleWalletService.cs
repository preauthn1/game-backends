using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.GameVendor;

namespace target.BL.Interface
{

    public interface IGameVendorSingleWalletService
    {
        SingleWalletGetWalletBalanceResultViewModel GetWalletBalance(GameVendorTypeEnum vendorID, SingleWalletGetWalletBalanceRequestParameter model);

        SingleWalletPlayerExtraInfoModel GetPlayerExtraInfo(GameVendorTypeEnum vendorID, int playerID);

        SingleWalletVendorCustomizedResponseConvertModel PlaceBet(GameVendorTypeEnum vendorID, GameVendorPlaceBetCallbackRequestParameter model);

        SingleWalletVendorCustomizedResponseConvertModel SettleBet(GameVendorTypeEnum vendorID, GameVendorSettleCallbackParameter model);

        SingleWalletVendorCustomizedResponseConvertModel CancelBet(GameVendorTypeEnum vendorID, SingleWalletCancelBetRequestParameter model);

        SingleWalletVendorCustomizedResponseConvertModel PlaceBetAndSettle(GameVendorTypeEnum vendorID, GameVendorPlaceBetCallbackRequestParameter model);

        SingleWalletVendorCustomizedResponseConvertModel WriteBetBetRecordWithoutUpdateBalance(GameVendorTypeEnum vendorID, GameVendorPlaceBetCallbackRequestParameter model);

        SingleWalletVendorCustomizedResponseConvertModel RecoverMissingBetRecord(GameVendorTypeEnum vendorID, GameVendorPlaceBetCallbackRequestParameter model);

        void CheckPlayerIsValidAndSystemMaintain(GameVendorTypeEnum vendorID, int playerID);

        GameVendorWalletTransactionResultModel GameVendorSendEventBonus(GameVendorTypeEnum vendorID, GameVendorWalletTransactionRequestParamter model);

        SingleWalletChangeSettleInfoResponseModel ChangeSettleAmount(GameVendorTypeEnum vendorID, SingleWalletChangeBetPayoutRequestParameter model);

        BetRecordGeneralColumnModel GetWagerBasicInfo(string transactionNo);
    }

}