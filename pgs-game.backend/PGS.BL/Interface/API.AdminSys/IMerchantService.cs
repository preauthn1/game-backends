using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;

namespace target.BL.Interface
{

    public interface IMerchantService
    {

        PagedList<MerchantViewModel> GetByPagingFilter(QueryMerchantApiRequestParameterModel filter);

        MerchantViewModel GetByID(int merchantID);

        MerchantCopyInfoPreviewViewModel GetCopyPreviewInfo(int userID, int merchantID);

        void Create(int userID, MerchantCreateRequestModel model);

        void UpdateStatusByAgent(int userID, LazyCommonRequestModel model);

        CRUDResultModel CopyAndBatchCreateNewMerchant(int userID, MerchantBatchCopyAndCreateRequestModel model);

        void Update(int userID, MerchantUpdateRequestModel model);

        void Delete(int userID , int targetID, string memo = "");

        MerchantNameExistCheckResult IsMerchantNameExist(string merchantName, bool isIncludeDeleteData = false);

        MerchantCompareResultViewModel CompareTwoMerchants(int userID, MerchantCompareRequestModel model);

        MerchantAdvanceListAllViewModel ListAllMerchantCustomizedSettings(int userID, MerchantAdvanceListRequestModel filter);

        MerchantStatusAndHierarchyPreviewInfoModel GetMerchantStatusAndHierarchyPreviewInfo(int userID, int merchantID);

        void DoExtraOperateForMerchant(int userID, LazyCommonRequestModel model);
    }
}
