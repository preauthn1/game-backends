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

    public interface IAdminSysInternalUtilityService
    {
        GenerateSQLResultViewModel GenerateSqlInsertCommand(LazyCommonRequestModel filter);

        FavoritSettingViewModel GetFavoritSetting(int userID , LazyCommonRequestModel filter);

        /// <summary>
        /// 外部人士使用 (請勿包含敏感資訊)
        /// </summary>
        CommonFavoritSettingViewModel GetCommonFavoritSetting(LazyCommonRequestModel filter);


        RemoteFileDownloadModel DownalodApiDocumentFile(LazyCommonRequestModel model);
    }
}
