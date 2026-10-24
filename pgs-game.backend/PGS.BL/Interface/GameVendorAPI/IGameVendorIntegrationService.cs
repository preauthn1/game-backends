using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.Base;
using target.Models.ViewModels.GameVendor;
using target.Utility.Utility;

namespace target.BL.Interface
{

    public interface IGameVendorIntegrationService
    {

        /// <summary>
        /// 檢查是否傳進來的token是否合法
        /// </summary>
        /// <param name="model"></param>
        ExternalVendorSessionTokenCheckResultModel CheckIsValidRequestInfo(ExternalVendorInputValidateRequestParameter model);


        /// <summary>
        /// (已自動繼承、不需自行實作) 取得遊戲商的時區
        /// </summary>
        public double GetVendorTimeZone();


        /// <summary>
        /// 將pgs通用的exception回應 轉成 每家遊戲商自訂的response格式
        /// </summary>
        SingleWalletExceptionHandleModel FormatParsing_ParseExceptionModelToVendorApiResponse(GameVendorExceptionParsingParameter model);



        /// <summary>
        /// 將這家遊戲商的 betRecord資料 轉成perGameLog
        /// </summary>
        ConvertPerGameLogResultModel FormatParsing_ConvertBetRecordToPerGameDetailLog(string betRecordNoPGS);


        /// <summary>
        /// 新建遊戲商的用戶帳號
        /// </summary>
        void CreateVendorPlayerAccountOrNot(ExternalVendorCreateAccountRequestParameter model);


        /// <summary>
        /// 當singleWallet API 通知玩家投注成功時，寫入一筆raw投注紀錄 (但是perGameLog 先不會寫)
        /// </summary>
        void WriteRawBetRecordWhenPlaceBet(InsertRawBetRecordRequiredInfoModel model, EachGameVendorRawRequestInfoModel rawInfo);


        /// <summary>
        /// 當singleWallet API 通知玩家投注成功時，更新raw投注紀錄
        /// </summary>
        SettlementBetRecordInfoModel UpdateRawBetRecordAfterSettlement(SettlementBetRecordRequiredBriefInfoModel model, EachGameVendorRawRequestInfoModel rawInfo);


        SettlementBetRecordInfoModel UpdateRawBetRecord(SettlementBetRecordRequiredInfoModel model);


        /// <summary>
        /// 取消注單
        /// </summary>
        ExternalVendorCancelBetResultModel CancelBet(string betRecordNoPGS, string memo);


        /// <summary>
        /// 取得 投注詳細資訊(外部連結/json)
        /// </summary>
        ThirdPartyGameVendorBetRecordDetailUrlViewModel GetExternalBetDetailAndScreenshot(ExternalVendorBetRecordDetailInfoRequestModel model);



        BetRecordBriefInfoModel GetByBetRecordNo(BetRecordInfoQueryParameter model);


        /// <summary>
        /// 根據 PGS單號 取得投注單
        /// </summary>
        BetRecordGeneralColumnModel GetByPGSTransactionNo(string betRecordNoPGS);


        /// <summary>
        /// API對接: 取得遊戲啟動網址
        /// </summary>
        ExternalVendorLaunchUrlViewModel GetGameLaunchUrl(ExternalVendorLaunchUrlRequestModel model);


        List<ThirdPartyVendorFetchGameListConvertModel> FetchGameList(int merchantID);


        ExternalVendorFetchWagersResult FetchWagers(ExternalVendorFetchWagersRequestParameter model);



        GameListIdInfoBrief GetGameByThirdGameCode(string gameCode);


        /// <summary>
        /// API對接: 取得投注統計數據
        /// </summary>
        ExternalVendorGetReportResultModel FetchEachGameAccountingReport(ExternalVendorGetReportRequestParameter model);

    }
}
