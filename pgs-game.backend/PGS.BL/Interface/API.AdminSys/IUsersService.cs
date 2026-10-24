using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;

namespace target.BL.Interface
{

    public interface IUsersService
    {
        PagedList<UsersViewModel> GetUsersPagingByFilter(int userID, QueryUserApiRequestParameterModel filter);

        UsersUpdatePageViewModel GetUserByID(int userID);

        UserCreateModel ConvertToUserCreateModel(MerchantCopyCreateUserAccountExtraProperty model);

        void CreateUserByQuickSetting(int userID, MerchantCopyCreateUserAccountExtraProperty model);

        CRUDResultModel Create(int userID, UserCreateModel model);

        /// <summary>
        /// 有資安風險 先棄用此API
        /// </summary>
        CRUDResultModel CopyByUserID(int userID, UserCopyRequestModel model);

        void Update(int userID, UserUpdateModel model);

        RequestDefaultPasswordResponseModel RequestDefaultPassword(int userID, RequestDefaultPasswordRequestModel model);

        CRUDResultModel UpdateUserPassword(int userID, UpdateCurrentUserPasswordRequestModel model, int currentUserID);

        void ResetReferenceCreditMerchant(int userID , int targetUserID);

        void UpdateUserBriefInfo(int currentUserID, UpdateUserBriefInfoRequestModel model);

        void Delete(int userID);

        void ForceLogout(int currentUserID, int targetUserID);

        void ValidateBeforeCreateUser(int userID , UserCreateModel model , UserCreateValidateParameter config = null);

        bool IsAccountExistInMyOrganization(int userID, string account);

        bool IsAccountExist(int organizationID, string account);

        void IsAccountExistThenThrowException(int organizationID, string account);

        bool IsContainsOrganizationPrefix(int organizationID, string account);

        UserAccountPrefixCheckResult IsContainsOrganizationPrefixCheck(int organizationID, string account);
    }
}
