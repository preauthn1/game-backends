using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;

namespace target.BL.Interface
{

    public interface IW88ExtraProcessService
    {
        AGGFishW88BalanceApiDetailDataModel GetW88Balance(int playerID);

        AGGFishTransferApiDetailDataModel GameWalletTransfer(WalletTransferTypeEnum type, int playerID, AGGFishTransferApiRequestaModel model);

        AGGFishTxtIDResultDetailModel WriteBonusRecord(int playerID, AGGFishBonusRecordApiRequestaModel model);

        AGGFishPaymentLinkResultDetailModel GetPaymentLink(int playerID, AGGFishGetPaymentLinkApiRequestaModel model);

    }

}