using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;

namespace target.BL.Interface
{
    public interface IOneToManyCustomizedSettingService
    {

        PagedList<OneToManyCustomizedSettingViewModel> GetByPagingFilter(int userID, QueryOneToManyCustomizedSettingApiRequestParameterModel filter);

        void Create(int userID, OneToManyCustomizedSettingCreateRequestModel model);

        SystemConfigSettingPageViewModel GetSettingItems(int userID, int oneToManyCustomizedSettingID);

        void UpdateSettingItems(int userID, SystemConfigSettingUpdateRequestModel model);

        void UpdateReportDateMemoSettings(int userID, LazyCommonRequestModel model);

        void InsertNewEmptyItem(int userID, OneToManySettingEazyRequestModel model);

    }
}
