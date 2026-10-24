using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.API.AdminSys;

namespace target.BL.Interface
{

    public interface IEventManagementService
    {
        PagedList<EventViewModel> GetByPagingFilter(int userID , QueryEventApiRequestParameterModel filter);

        EventDetailViewModel GetEventDetail(int userID, int eventID);

        void UpdateEventDetail(int userID, EventDetailViewModel model);

        void DeleteEvent(int userID, EventDeleteRequestModel model);

        EventQuickCreateResultViewModel QuickCreateEvent(int userID, EventQuickCreateRequestModel model);

        EventQuickCreateResultViewModel CopyEvent(int userID, EventCopyRequestModel model);

        void AppendEventCustomizedSetting(int userID, EventAppendSettingRequestModel model);

        PagedList<EventPrizeRecordViewModel> GetPrizeRecordsByPagingFilter(int userID, QueryEventPrizeRecordParameterModel filter);

        CRUDResultModel ResetPlayerRTPByEvent(int userID, ResetAllPlayerRTPByEventRequestModel model);

        EventContentBlockEditViewModel GetEventContentBlock(int userID, int eventID);

        void UpdateEventContentBlock(int userID, EventContentBlockEditViewModel model);

        void InitialEventContentBlock(int userID, EventContentBlockInitialRequestModel model);

        ExportEventPrizeCorrectValidateResultModel CheckExportPrizeRecordValid(int userID, ExportEventPrizeCorrectValidateRequest filter);
    }
}
