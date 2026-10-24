using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.API.AdminSys;

namespace target.BL.Interface
{

    public interface IEventMultiplePrizeService
    {
        PagedList<EventDrawPrizeViewModel> GetByPagingFilter(int userID, QueryEventDrawPrizeApiRequestParameterModel filter);

        EventDrawPrizeDetailViewModel GetDrawPrizeDetail(int userID, int eventDrawPrizeID);

        void UpdateDrawPrizeDetail(int userID, EventDrawPrizeDetailViewModel model);

        CRUDResultModel CloneDrawPrize(int userID, EventDrawPrizeCloneRequestModel model);

        void Delete(int userID, int eventDrawPrizeID);
    }
}
