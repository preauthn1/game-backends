using target.Models.Table;
using System;
using System.Collections.Generic;

namespace target.BL.Interface
{
    public interface IApiRequestLogService
    {
        void CreateTryCatch(string tableName, ApiRequestLogEntity model);

        void CreateErrorLogTryCatch(string tableName, ApiRequestLogEntity model);

        void WriteToQueueBuffering(ApiRequestLogQueueTempModel model);

        void ReadQueueAndSyncApiRequestLogToRemote();

        void ReadQueueAndSyncSwCallbackLogToRemote();

        void ReadQueueAndSyncThirdPartyLogToRemote();

        void ReadQueueAndSyncErrorLogToRemote();

        void WriteSWLogToStorage(List<SingleWalletMerchantApiRequestLogEntity> myItems);
    }



}
