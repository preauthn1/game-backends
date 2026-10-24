using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;

namespace target.BL.Interface
{
    public interface IBOAnnouncementService
    {

        PagedList<BOAnnouncementViewModel> GetByPagingFilter(int userID, QueryBOAnnouncementApiRequestParameterModel filter);


        BOAnnouncementViewModel GetById(int userID, int id);


        CRUDResultModel Create(int userID, BOAnnouncementCreateRequestModel model);


        CRUDResultModel Update(int userID, BOAnnouncementUpdateRequestModel model);


        CRUDResultModel Delete(int userID, int targetID);

        BOAnnouncementHomePageViewModel GetVisibleAnnouncements(int userID);

        void SetAnnouncementToRead(int userID, int announcementID);
    }
}
