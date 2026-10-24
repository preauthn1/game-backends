using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;

namespace target.BL.Interface
{

    public interface ISingleWalletCallRetryJobQueueService
    {
        void BetReocrdNotifyQueueRetryProcess();

        void BetReocrdNotifyQueueRetryProcess_SpecificOrderNo(string specificBetRecordNoPGS);

        CRUDResultModel ConvertEazyTempValueToNotifyQueue(EazyTempValueConvertNotifyQueueRequestModel model);

        void TriggerRetryByID(long id);
    }
}
