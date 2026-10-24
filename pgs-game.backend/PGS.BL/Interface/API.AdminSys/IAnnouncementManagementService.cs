using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.API.AdminSys;

namespace target.BL.Interface
{

    public interface IAnnouncementManagementService
    {
        PagedList<AnnouncementPagingViewModel> GetByPagingFilter(int userID, QueryAnnouncementApiParameterModel filter);

        AnnouncementDetailViewModel GetDetail(int userID, int announcementID);

        void UpdateDetail(int userID, AnnouncementDetailViewModel model);

        CRUDResultModel QuickCreateEmptyAnnoucement(int userID, AnnouncementCreateRequestModel filter);

        void Delete(int userID, int announcementID);

        void ClearAllRead(int userID, int announcementID);
    }
}
