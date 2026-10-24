using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.BL.Interface;
using target.Models.Enums;
using target.Models.Models;
using System.Linq;
using target.Utility.Helper;
using target.Models;
using target.DA.Repository;
using target.Models.ViewModels.Base;
using target.Utility;
using target.Utility.Utility.ThirdParty;

namespace target.BL.Services
{

    public class UserInfoService : BaseAdminSysService, IUserInfoService
    {
        public UserRepository UserRepository { get; set; }

        public UserTokenRecordRepository UserTokenRecordRepository { get; set; }

        public OrganizationRepository OrganizationRepository { get; set; }


        public UserInfoViewModel GetUserProfile(int userID)
        {
            var queryUser = UserRepository.GetById(userID);

            if (queryUser == null)
                throw new TerproException(ApiErrorCode.NotFoundUser);

            var queryOrgs = OrganizationRepository.GetById(queryUser.OrganizationID);

            TimeZoneAreaEnum timeZone = (TimeZoneAreaEnum)queryUser.TimeZoneAreaID;

            return new UserInfoViewModel
            {
                TimeZone = DataModelConvertHelper.GetTimeZoneByAreaID(queryUser.TimeZoneAreaID),
                TimeZoneAreaID = queryUser.TimeZoneAreaID,
                TimeZoneAreaName = MultiLanguageTextHelper.GetTimeZoneDisplayName(queryUser.TimeZoneAreaID),
                UserID = userID,
                UserAccount = queryUser.UserName,
                OrganizationID = queryUser.OrganizationID,
                OrganizationName = queryOrgs.OrganizationName,
                DefaultLang = queryUser.DefaultLang,
                IsSuperAdminOrganization = queryOrgs.OrganizationType == OrganizationTypeEnum.SuperAdmin.ToInteger()
            };
        }


        public AdminUserTokenJwtModel ParseAndCheckIsValidJwtToken(string jwtToken)
        {
            //jwt檢查包含 檢查偽造、過期、解析失敗
            if (string.IsNullOrWhiteSpace(jwtToken))
                throw new TerproException(ApiErrorCode.Authorize_AccessToken_NotValid, new ErrorDetailModel("#001"));

            AdminUserTokenJwtModel convertUserModel = MyJwtHelper.GetAdminUserModelFromJwtString(jwtToken);

            if (convertUserModel == null)
                throw new TerproException(ApiErrorCode.Authorize_AccessToken_NotValid, new ErrorDetailModel("#005"));

            var now = DateTimeHelper.GetCurrentTimeTimestamp();

            if (convertUserModel.TokenExpiredTime <= now)
                throw new TerproException(ApiErrorCode.Authorize_AccessToken_Expired, new ErrorDetailModel("#007"));

            string jwtHash = jwtToken.MD5Hash();

            var queryUser = UserRepository.GetSingleByCache(convertUserModel.LoginUserID);

            if (queryUser == null || queryUser.Status != NormalActiveStatus.Enable.ToInteger())
            {
                throw new TerproException(ApiErrorCode.InvalidStatus_AccountStatus);
            }

            var queryExistRecord = UserTokenRecordRepository.GetCacheByTokenHash(jwtHash);

            if (queryExistRecord == null)
            {
                throw new TerproException(ApiErrorCode.Authorize_AccessToken_Expired);
            }

            return convertUserModel;
        }


        public void UpdateUserLanguage(int userID, string lang)
        {
            bool isSupport = RegexRuleHelper.IsSupportLanguage(lang);

            if (!isSupport)
                throw new TerproException(ApiErrorCode.InvalidParameter_NotSupportLanguageCodet);

            var queryExist = UserRepository.GetById(userID);
            queryExist.DefaultLang = lang;

            UserRepository.Update(queryExist);
        }


        public void UpdateUserTimezone(int userID, UpdateCurrentUserTimezoneRequestModel model)
        {
            model.RuleFor(x => x.TimeZoneID).Rule_NotZeroNotMinus();
            UserRepository.Update(userID, new { TimeZoneAreaID = model.TimeZoneID });
        }


    }
}
