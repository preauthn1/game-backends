using System;
using System.Collections.Generic;
using example.invalid;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;

namespace target.BL.Interface
{

    public interface IPerGameDetailService
    {

        PagedList<PerGameDetailViewModel> GetByPagingFilter(int currentUserID , QueryPerGameDetailLogApiRequestParameterModel filter);

        PerGameSlotHistoryDetailInfoViewModel GetSlotHistoryByTransactionNo(int currentUserID, string betRecordNoPGS);

        PerGameSlotHistoryDetailInfoViewModel GetSlotHistoryFromPerGameLog(int currentUserID, long perGameLogDetailID);

        PerGameLogFullInformationViewModel GetFullInformationInPerGame(int currentUserID , long perGameLogID);

        string ExportLargeCsvTextByVendorConfigID(int userID, LazyCommonRequestModel model);

        void AddTag(int currentUserID, PerGameTagAddRequestModel model);

        CRUDResultModel AddTagByTransactionNoList(int currentUserID, PerGameTagBatchAddRequestModel model);
    }
}
