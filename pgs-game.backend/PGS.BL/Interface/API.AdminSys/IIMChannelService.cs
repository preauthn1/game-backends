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

    public interface IIMChannelService
    {

        PagedList<IMChannelViewModel> GetByPagingFilter(int userID, IMChannelRequestParameterModel filter);

        IMChannelEntity GetByChatID(string chatID, IMChannelTypeEnum type);

        CRUDResultModel Create(int userID, IMChannelCreateRequestModel model);

        CRUDResultModel Update(int userID, IMChannelUpdateRequestModel model);

        CommonCustomizedSettingBatchEditModel GetIMChannelCutomziedSettings(int userID, int imChannelID);

        void CreateOrUpdateSetting(int userID, CustomizedSettingItemEditModel model);

    }
}
