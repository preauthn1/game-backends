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

    public interface IGameVendorSingleWalletCommonLogicProcessService
    {
        UpdateBalanceSPResult UpdateWalletAmountSettleProcess(GameVendorTypeEnum vendorID, SettleBalanceUpdateProcessConfig config, ConvertPerGameLogResultModel convertPerGameResult);

        GameVendorWalletTransactionResultModel GameVendorSendEventBonus(GameVendorTypeEnum vendorID, GameVendorWalletTransactionRequestParamter model);

    }

}