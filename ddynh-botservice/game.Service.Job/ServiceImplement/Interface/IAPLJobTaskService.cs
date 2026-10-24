using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.Users;
using game.Entity.Game;
using game.Entity.ViewModels;

namespace game.Service
{

    public interface IAPLJobTaskService
    {

        /// <summary>
        /// 以台灣時區(+8) 當基準，重抓昨日的數據
        /// </summary>
        void FetchByTaiwanYesterday(int platformID);

        /// <summary>
        /// 以台灣時區(+8) 當基準，重抓某一天的數據
        /// </summary>
        void FetchByTaiwanTargetDate(DateTime date, int platformID);


        void FetchByTaiwanTimeFromDateToDate(DateTime begin, DateTime end, int platformID);



        void FetchBetRecordsSinglePlatform(JobTriggerFetchBetRecordParameterModel model);


        /// <summary>
        /// 自動對帳昨日的數據
        /// </summary>
        void CheckYesterdayDataIsCorrect(int platformID);


        void CheckTargetDataIsCorrect(int platformID, DateTime targetDate_8TimeZone);


        /// <summary>
        /// 檢查尚未結算的注單 是否已結算
        /// </summary>
        /// <param name="platformID"></param>
        void ConfirmNotSettleBetRecords(int platformID);


        object GetBetRecordApiResult(int platformID, DateTime targetDate_8TimeZone);

    }
}
