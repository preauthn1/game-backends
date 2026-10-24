using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.Base;
using target.Utility.Utility;
using target.Utility.Extensions;
using System.Linq;
using target.Models.Models;
using target.Utility;
using target.DA.Repository;
using target.BL.Interface;
using target.Models.ViewModels.GameVendor;
using target.Utility.Helper;
using target.Utility.Helper.HttpContext;

namespace target.BL.Services
{

    /// <summary>
    /// 之前會拆另一個檔案是因為這邊有分成 v2023 / v2025 兩種寫法
    /// </summary>
    public partial class AGGIntegrationService : BaseGameVendorIntegrationService, IGameVendorIntegrationService, IAGGInternalAPIService
    {

        public ExternalVendorLaunchUrlViewModel GetGameLaunchUrlByAPI(ExternalVendorLaunchUrlRequestModel model)
        {
            return GetGameLaunchUrlByAPI_V202502(model);
        }


        public string GetRecordReplayHistoryByApi(ExternalVendorGetSingleBetRecordRequestParameter model)
        {
            return GetRecordReplayHistoryByApi_V202502(model);
        }


        public ExternalVendorGetReportResultModel FetchEachGameAccountingReport(ExternalVendorGetReportRequestParameter filter)
        {
            return FetchEachGameAccountingReport_V202502(filter);
        }


        public ApiResultBriefObjectModel GetSingleBetRecordRawResponseByApi(string betRecordNoPGS)
        {           
            return GetSingleBetRecordRawResponseByApi_V202502(betRecordNoPGS);
        }


        /// <summary>
        /// AGG撈注單目前遇過的問題
        /// 2025/01 有特定注單撈不到
        /// 2025/01 撈到所有注單後，篩選 endTime特定日期。實際筆數與他們後台筆數不符
        /// 2025/02 因網路異常，導致以下注單遺失/重複寫單 (2025/02/03 的TG訊息)
        /// </summary>
        /// <exception cref="TerproException"></exception>
        public AGGAccountingRecordsResponseModel GetAGGRecordsByInternalApi_New(ExternalVendorFetchWagersRequestParameter model, int pageSize)
        {
            return GetAGGRecordsByInternalApi_V202502(model, pageSize);
        }


        /// <summary>
        /// 取得異常注單
        /// </summary>
        public ApiResultBriefModel<AGGGetExceptionalWagersResponseModel> GetExceptionalRecords(AGGGetExceptionalWagersReqeustParameter model)
        {
            return GetExceptionalRecords_V202502(model);
        }


        /// <summary>
        /// 觸發異常注單處理
        /// </summary>
        public string TriggerExceptionalWagerRetry(AGGGetExceptionalWagersReqeustParameter model)
        {
            return TriggerExceptionalWagerRetry_V202502(model);
        }


        public AGGNamespaceCreateResponseModel CreateAGGNamespaceByAPI(int merchantID)
        {
            return CreateAGGHierarchyNodes_EachLayer(merchantID);
        }



        //private bool IsEnableV2API(int merchantIdOrVendorConfigID = 0)
        //{
        //    if (merchantIdOrVendorConfigID == 0)
        //        return false;

        //    if (RegexRuleHelper.IsMerchantID(merchantIdOrVendorConfigID))
        //    {
        //        SetVendorConfigSettingsToTempVariableByMerchantID(merchantIdOrVendorConfigID);
        //    }
        //    else
        //    {
        //        SetVendorConfigSettingsToTempVariableByVendorConfigID(merchantIdOrVendorConfigID);
        //    }

        //    AGGExtraJsonSettingModel jsonSetting = GetAGGExtraSetting(isThrowException: false);

        //    if (jsonSetting == null)
        //        return false;

        //    return jsonSetting.IsV2API;
        //}


    }
}
