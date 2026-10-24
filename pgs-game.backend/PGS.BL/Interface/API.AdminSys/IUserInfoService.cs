using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.Base;

namespace target.BL.Interface
{

    public interface IUserInfoService
    {        
        UserInfoViewModel GetUserProfile(int userID);

        AdminUserTokenJwtModel ParseAndCheckIsValidJwtToken(string jwtToken);

        void UpdateUserLanguage(int userID, string lang);

        void UpdateUserTimezone(int userID, UpdateCurrentUserTimezoneRequestModel model);

    }
}
