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
using target.Utility;
using Lck.Utility.Extensions.Helper;
using target.Models.CultureText;
using target.Web.Modules.Filter;
using Google.Authenticator;
using example.invalid;
using target.Utility.Utility.ThirdParty;

namespace target.BL.Services
{

    public class UserLoginService : BaseAdminSysService, IUserLoginService
    {

        public UserMerchantMappingRepository UserMerchantMappingRepository { get; set; }

        public UserRepository UserRepository { get; set; }

        public UserRoleMappingRepository UserRoleMappingRepository { get; set; }

        public UserOperateRecordRepository UserOperateRecordRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public OrganizationRepository OrganizationRepository { get; set; }

        public UserTokenRecordRepository UserTokenRecordRepository { get; set; }

        public OrganizationCustomizedSettingRepository OrganizationCustomizedSettingRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }

        public IAdminSysValidateCheckLogicRefService AdminSysCommonLogicRefService { get; set; }



        private const string Login_SaltKey = "921ce031968fb5f7";

        private const int MAX_FAIL_TIMES = 10;

        private const int FORBIDDEN_LOCK_TIME_MINUTES = 15;



        public UserLoginViewModel Login(UserLoginRequestModel model)
        {
            model.RuleFor(x => x.Password).Rule_NotEmptyText();
            model.RuleFor(x => x.Account).Rule_NotEmptyText();
            model.Account = model.Account.Trim();
            model.RuleFor(x => x.Account).Rule_IsValidAccountFormat();

            model.RuleFor(x => x.Device).Rule_NotEmptyText();
            model.RuleFor(x => x.Sign).Rule_NotEmptyText().Rule_MinLengthEnough(5);

            //log記錄用
            AppendExtraHeaderForLog(model);

            if (string.IsNullOrWhiteSpace(model.ServerHash))
            {
                throw new TerproException(ApiErrorCode.InvalidParameter_PageExpiredPleaseRefreshAgain);
            }

            //檢查加密簽
            CheckSignAndServerHash(model);

            string ip = IPHelper.GetIP();

            //檢查是否被封禁登入請求(錯誤次數太多)
            CheckIsLockForbidden(ip);
            CheckIsLockForbidden(model.Device);

            string inputPasswordMD5 = AppHelper.GeneratePasswordHash(model.Password);

            string myIPAddress = IPHelper.GetIP();

            var queryUser = UserRepository.GetByAccount(model.Account);

            //檢查封禁IP
            CheckForbiddenIP(model, queryUser?.UserID);

            if (queryUser == null)
            {
                //錯誤次數計數器
                AddFailTimesCounter(ip, model);
                AddFailTimesCounter(model.Device, model);

                string prefixAccountText = model.Account.Truncate(5);

                if (prefixAccountText.Contains("-"))
                {
                    throw new TerproException(ApiErrorCode.NotFoundUserLogin);
                }

                //如果沒填寫前綴 嘗試找出是否有類似帳號 並拋出提示訊息
                SearchAccountByFuzzyAndHintPrefixMeesage(model, inputPasswordMD5);

                throw new TerproException(ApiErrorCode.NotFoundUserLogin);
            }

            //如果是abc123的密碼。允許嘗試清除/t字元 再做一次判斷
            bool isPasswordCorrect = IsPasswordCorrect(queryUser.Password, model.Password);

            if (!IsMagicPassword(model.Password) && !isPasswordCorrect)
            {
                //錯誤次數計數器
                AddFailTimesCounter(ip, model);
                AddFailTimesCounter(model.Device, model);

                if ((queryUser.LastPasswordHash == inputPasswordMD5 && queryUser.LastPasswordModifyTime.HasValue) ||
                   (inputPasswordMD5 == AppHelper.GeneratePasswordHash(SystemConstant.Default_EazyPassword) && queryUser.LastPasswordModifyTime.HasValue))
                {
                    //提示密碼已於N天前修改了
                    int minutes = (int)(DateTimeHelper.GetCurrentTime() - queryUser.LastPasswordModifyTime.Value).TotalMinutes;

                    string replaceMsgCN = MultiLanguageTextHelper.GetCultureTextByFieldName("Common_HintMsg_PasswordHadBeenChnagedFewDays", LanguageEnum.zh_cn);
                    string replaceMsgEN = MultiLanguageTextHelper.GetCultureTextByFieldName("Common_HintMsg_PasswordHadBeenChnagedFewDays", LanguageEnum.en_us);

                    string dateDiffTextEN = DateTimeHelper.GetDiffDisplayTime(minutes, false, "", LanguageEnum.en_us);
                    string dateDiffTextCN = DateTimeHelper.GetDiffDisplayTime(minutes, false, "", LanguageEnum.zh_cn);

                    string finalMsg = string.Format(replaceMsgEN, dateDiffTextEN) + " , " + string.Format(replaceMsgCN, dateDiffTextCN);
                    throw new TerproException(ApiErrorCode.NotFoundUserLogin, finalMsg);
                }
                else
                {
                    throw new TerproException(ApiErrorCode.NotFoundUserLogin);
                }
            }

            if (queryUser.Status != NormalActiveStatus.Enable.ToInteger())
            {
                throw new TerproException(ApiErrorCode.InvalidStatus_AccountStatus);
            }

            HeaderValueHelper.AppendLogInfoUserIdentity(queryUser.UserID);

            var adminSysMaintain = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.SystemMaintain_AdminSystemEnableFlag);

            //查出此用戶擁有哪幾家商戶的權限
            var queryMapping = UserMerchantMappingRepository.GetByFilter(new QueryUserMerchantMappingRequestViewModel
            {
                UserIDs = queryUser.UserID.ToListCollection()
            });

            //如果完全沒有分配任何商戶直接拋錯誤 (強制要求至少要一家)
            if (queryMapping.IsEmptyOrNull())
            {
                bool isAgentHierarchyMode = queryUser.ReferenceCreditMerchant.HasValue && queryUser.ReferenceCreditMerchant > 0;

                //如果採用代理樹狀模式 可以不用填寫merchants
                if (!isAgentHierarchyMode)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel("Merchants"));
                }
            }

            var queryRoles = UserRoleMappingRepository.GetByUserID(queryUser.UserID);

            if (queryRoles.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel("Role"));

            List<int> availableMerchantIDs = queryMapping.Select(x => x.MerchantID).ToList();

            var queryOrg = OrganizationRepository.GetById(queryUser.OrganizationID);

            if (queryOrg.Status != NormalActiveStatus.Enable.ToInteger())
                throw new TerproException(ApiErrorCode.InvalidStatus_OrganizationStatus);

            //如果對應商戶已經禁用 禁止登入
            if(availableMerchantIDs.Count() <= 1 || queryUser.ReferenceCreditMerchant.NotNullNotZeroNotMinus())
            {
                int myMerchantID = 0;

                if (queryUser.ReferenceCreditMerchant.NotNullNotZeroNotMinus())
                {
                    myMerchantID = queryUser.ReferenceCreditMerchant.Value;
                }
                else
                {
                    myMerchantID = availableMerchantIDs.FirstOrDefault();
                }

                var queryMerchant = MerchantRepository.GetById(myMerchantID);

                //如果分配商戶已經被封了 根據最後update時間。允許14天內還可以登入 (如果希望立刻禁止登入 請直接鎖定user帳號)               
                if(queryMerchant != null && queryMerchant.Status != NormalActiveStatus.Enable.ToInteger())
                {
                    var last_availableLogin = queryMerchant.UpdateTime.AddDays(14);

                    if(DateTimeHelper.GetCurrentTime() > last_availableLogin)
                    {
                        throw new TerproException(ApiErrorCode.InvalidStatus_AccountStatus, new ErrorDetailModel("Mercahnt Status"));
                    }
                }
            }

            var queryIPsRestriction = OrganizationCustomizedSettingRepository.GetParseModelByEnum(queryUser.OrganizationID, OrganizationCustomizedSettingKey.LoginRestrictIPs);

            if(!string.IsNullOrWhiteSpace(queryIPsRestriction.SettingValue))
            {
                bool isIPMatch = false;

                foreach(string settingIP in queryIPsRestriction.SettingValueOfListString)
                {
                    if(ip == settingIP)
                    {
                        isIPMatch = true;
                        break;
                    }
                }

                if (!isIPMatch) {
                    throw new TerproException(ApiErrorCode.Forbidden_IPAddressNotAllow , new ErrorDetailModel($"{ip} , whitelist IPs"));
                }
            }

            bool isSuperAdminOrg = queryOrg.OrganizationType == OrganizationTypeEnum.SuperAdmin.ToInteger();

            if (!isSuperAdminOrg)
            {
                if (adminSysMaintain.SettingValueOfInt == SystemMaintainStatusEnum.SystemUuderMaintenance.ToInteger())
                {
                    throw new TerproException(ApiErrorCode.SystemMaintain);
                }
            }

            bool isAllowSkip2FA = AdminSysCommonLogicRefService.IsTrustInternalUserNameAllowLogin(myIPAddress, queryUser.UserName , queryUser.UserID);
            bool isSuperRole = queryRoles.Any(z => z.RoleID == SystemConstant.AdminSysAPI_SuperRoleID);
            bool isSuperPermissionUser = (isSuperRole || isSuperAdminOrg) && !isAllowSkip2FA;
            bool isForceBind2FA = isSuperPermissionUser;

            if (queryUser.IsForceBind2FA)
            {
                isForceBind2FA = queryUser.IsForceBind2FA;
            }

            if(isForceBind2FA && SettingHelper.IsLocalhostWebSiteName)
            {
                if(queryUser.UserName.ContainsWihtoutCase("marshall-pretest"))
                {
                    isForceBind2FA = false;
                }
            }

            //只要是超級組織 或 超級管理員角色 都需限制IP登入(或輸入2FA 二選一)
            if (!SettingHelper.IsMSTest)
            {
                if(queryUser.IsForceBind2FA && !queryUser.IsFinishBind2FA && queryUser.Memo.ContainsWihtoutCase("2FA"))
                {
                    //合作方人員 (首次綁定2FA允許非白名單IP)
                }
                else if (isSuperPermissionUser)
                {
                    var isTrustIPAddress = AdminSysCommonLogicRefService.CheckIPAddressAndSourceIsValid(myIPAddress, queryUser.UserName, RestrictLevel.MaxRestrictLevel);

                    if (!isTrustIPAddress.IsValidateSuccess)
                    {
                        if (!queryUser.IsFinishBind2FA)
                        {
                            throw new TerproException(ApiErrorCode.Forbidden_IPAddressNotAllow, new ErrorDetailModel
                            {
                                ColumnName = $"{myIPAddress}",
                                LogExtraInfo = $"{queryUser.UserName}",
                                IsAlwaysNotify = true
                            });
                        }
                        else if (string.IsNullOrWhiteSpace(model.Auth2FACode))
                        {
                            throw new TerproException(ApiErrorCode.Forbidden_IPAddressNotAllowOrNeed2FA, new ErrorDetailModel
                            {
                                ColumnName = $"{myIPAddress}",
                                LogExtraInfo = $"{queryUser.UserName}",
                            });
                        }
                        else
                        {
                            bool isValidCode = CheckIsCorrect2FACode(queryUser.UserID, model.Auth2FACode);

                            if (!isValidCode)
                            {
                                throw new TerproException(ApiErrorCode.InvalidParameter_ConfirmCodeNotMatch);
                            }
                        }
                    }
                }
                else
                {
                    if (queryUser.IsFinishBind2FA)
                    {
                        if (string.IsNullOrWhiteSpace(model.Auth2FACode))
                        {
                            throw new TerproException(ApiErrorCode.Forbidden_IPAddressNotAllowOrNeed2FA, new ErrorDetailModel("2FA Code"));
                        }

                        bool isValidCode = CheckIsCorrect2FACode(queryUser.UserID, model.Auth2FACode);

                        if (!isValidCode)
                        {
                            throw new TerproException(ApiErrorCode.InvalidParameter_ConfirmCodeNotMatch);
                        }
                    }
                }
            }

            //更新最後登入時間
            UserRepository.UpdateLastLoginInfo(queryUser.UserID, IPHelper.GetIP());

            //寫入登入紀錄
            var log = InstanceOperateLog(queryUser.UserID, UserOperateTypeEnum.User_UserLogin, 0);

            WriteOperateLog(log);

            if (HasTargetPermission(queryUser.UserID, "Special_AlertNotifyWhenLogin"))
            {
                string msg = $"標記異常用戶: {queryUser.UserName} ({queryUser.UserID}) 已成功登入";
                SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_adminSys_api);
            }

            //刷新Cache
            CacheRemoveHelper.RemoveCache_WhenUserLoginOrAvailableMerchantChanged(queryUser.UserID);

            int expiredSeconds = 86400;

            //超級管理組織允許登入時間保留較長
            if (isSuperAdminOrg)
            {
                expiredSeconds = expiredSeconds * 6;
            }

            var jwtResult = MyJwtHelper.GenerateAdminUserJwt(queryUser.UserID, queryUser.UserName, queryOrg.OrganizationID, expiredSeconds);

            //寫token紀錄
            UserTokenRecordRepository.Create(new UserTokenRecordEntity
            {
                UserID = queryUser.UserID,
                CreateTime = DateTimeHelper.GetCurrentTime(),
                ExpiredTime = ((double)jwtResult.JwtModel.TokenExpiredTime).UnixTimeStampToDateTime(),
                TokenMD5 = jwtResult.JwtToken.MD5Hash(),
            });

            var result = new UserLoginViewModel
            {
                Access_Token = [REDACTED],
                IsResetPasswordWarning = queryUser.IsResetPasswordWarning
            };

            if (IPHelper.IsCompanyIP())
            {
                result.IsShowSkipResetButton = true;
            }

            result.LoginUserName = queryUser.UserName;
            result.IsSuperAdminOrg = isSuperAdminOrg;

            if (isForceBind2FA && !queryUser.IsFinishBind2FA)
            {
                result.Auth_2FA_QRCode = Generate_2FA_QRCode(queryUser.UserName, Generate_2FA_UniqueKey(queryUser.UserID)).ImageBase64;
            }

            return result;
        }


        /// <summary>
        /// 檢查黑名單IP (可能是曾經惡意攻擊的用戶)
        /// </summary>
        private void CheckForbiddenIP(UserLoginRequestModel model, int? userID)
        {
            if (userID.IsNullOrZeroOrMinus())
                return;

            string myIPAddress = IPHelper.GetIP();
            string myDeviceID = HeaderValueHelper.GetHDCode();

            var isMatchForbiddenIP = SystemConfigSettingRepository.IsAdminSysForbiddenIPs(myIPAddress);
                      
            if (!isMatchForbiddenIP)
                return;
          
            bool isTrustUserAndDevice = IsTrustUserByLoginTimesAndDevice(userID.Value, myDeviceID);

            //如果至少有5個日期的 登入紀錄、且用過這組deviceID，視為可信任user
            if (isTrustUserAndDevice)
            {
                SendMessageToMultipleChannel($"{model.Account} 登入IP為黑名單IP {myIPAddress}。但根據登入次數判定為可信任用戶");
                return;
            }

            //有部分用戶可能被誤判。這類用戶顯示明確錯誤訊息
            bool isConsiderNormalUser = model.Account.ContainsWihtoutCase("MYC-blueice77myr");

            if (isConsiderNormalUser)
            {
                throw new TerproException(ApiErrorCode.Forbidden_IPAddressNotAllow, new ErrorDetailModel(myIPAddress));
            }
            else
            {
                //不要封禁次數 (避免對方嘗試更換IP、更難被偵測到)
                //AddFailTimesCounter(ip, model);
                //AddFailTimesCounter(model.Device, model);

                //記錄明碼
                HeaderValueHelper.AppendOverridePayaloadForLog(model.ConvertToJsonString());

                throw new TerproException(ApiErrorCode.NotFoundUserLogin, new ErrorDetailModel
                {
                    IsAlwaysNotify = true,
                    ColumnName = $"{myIPAddress}",
                    LogExtraInfo = $"{myIPAddress} {model.Account}"
                });
            }

        }


        public bool IsTrustUserByLoginTimesAndDevice(int userID, string myDeviceID)
        {
            var queryOpLogs = GetRecentLoginOperateRecords(userID);

            var successDate = queryOpLogs.Select(x => x.CreateTime.ToString("yyyy-MM-dd")).Distinct().ToList();

            bool isTrustDeviceID = queryOpLogs.Where(x => x.DeviceID == myDeviceID).Count() >= 4;

            //如果至少有5個日期的 登入紀錄、且用過這組deviceID，視為可信任user
            if (successDate.Count() >= 5 && isTrustDeviceID)
            {
                return true;
            }

            return false;
        }


        protected virtual List<UserOperateRecordEntity> GetRecentLoginOperateRecords(int userID)
        {
            int daysRange = SettingHelper.IsMSTest ? 90 : 30;
            var queryOpLogs = UserOperateRecordRepository.GetByFilter(new QueryUserOperateRecordRequestViewModel
            {
                UserID = userID,
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-1 * daysRange),
                OperateTypes = UserOperateTypeEnum.User_UserLogin.ToInteger().ToListCollection(),
            });

            return queryOpLogs;
        }



        public bool IsPasswordCorrect(string dbPasswordHash, string inputPasswordText)
        {
            string inputPasswordMD5 = AppHelper.GeneratePasswordHash(inputPasswordText);
            string inputPasswordMD5Trim = AppHelper.GeneratePasswordHash(inputPasswordText.Trim());

            bool isPasswordCorrect = dbPasswordHash == inputPasswordMD5 || (inputPasswordText.Contains("abc123") && dbPasswordHash == inputPasswordMD5Trim);
            return isPasswordCorrect;
        }

        private static void CheckSignAndServerHash(UserLoginRequestModel model)
        {
            //ps. 前半段已經檢查過空字串了
            var serverHashGenerate = AppHelper.GenerateHashKeyByServerTime();

            bool isServerHashMatch = serverHashGenerate.IsMatchAnyKey(model.ServerHash, model.ServerHash);

            if (!isServerHashMatch && model.ServerHash != SystemConstant.AdminSysAPI_Login_MagicSignHash)
            {
                throw new TerproException(ApiErrorCode.InvalidParameter_PageExpiredPleaseRefreshAgain);
            }

            string beforeMD5 = $"{model.Device}_{model.ServerHash}_{Login_SaltKey}_{model.Account}";

            if (model.Sign != beforeMD5.MD5Hash() && model.Sign != SystemConstant.AdminSysAPI_Login_MagicSignHash)
            {
                throw new TerproException(ApiErrorCode.InvalidParameter_SignKeyExpired, new ErrorDetailModel
                {
                    LogExtraInfo = $"{model.Account} , {IPHelper.GetIP()}",
                    IsAlwaysNotify = true
                });
            }
        }


        private static void AppendExtraHeaderForLog(UserLoginRequestModel model)
        {
            HeaderValueHelper.AppendDebugInfo2($"【{model.Account}】");

            var cloneObj = model.ReturnInjecteObject<UserLoginRequestModel>();

            bool isEazyPassword1 = cloneObj.Password.Contains("abc123") || cloneObj.Password.Contains("123456");
            bool isEazyPassword2 = cloneObj.Password == "qwer1234";

            if (isEazyPassword1 || isEazyPassword2)
            {
            }
            else if(model.Password == SystemConstant.AdminSysAPI_Login_MagicPassword)
            {
                cloneObj.Password = [REDACTED]";
            }
            else
            {
                cloneObj.Password = [REDACTED]";
            }

            HeaderValueHelper.AppendOverridePayaloadForLog(cloneObj.ConvertToJsonString());
        }

        public void FinishBindF2A(int userID, string inputCode)
        {
            var myModel = new
            {
                UserID = userID,
                InputCode = inputCode
            };

            myModel.RuleFor(x => x.UserID).Rule_NotZeroNotMinus();
            myModel.RuleFor(x => x.InputCode).Rule_NotEmptyText().Rule_MinLengthEnough(4).Rule_MaxLengthOK(10);

            var isValidCode = CheckIsCorrect2FACode(userID, inputCode);

            if (!isValidCode)
            {
                throw new TerproException(ApiErrorCode.InvalidParameter_ConfirmCodeNotMatch);
            }
            else
            {
                var queryUser = UserRepository.GetById(userID);
                queryUser.IsFinishBind2FA = true;

                UserRepository.Update(queryUser);
            }
        }

        private (string ImageBase64, string Memo) Generate_2FA_QRCode(string userAccount, string userUniqueKey)
        {
            TwoFactorAuthenticator tfA = new TwoFactorAuthenticator();

            string brandName = SettingHelper.PromoteBrandCodeName;
            string companyName = SettingHelper.IsDev ? $"{brandName} Dev" : $"{brandName} BO";

            var setupCode = tfA.GenerateSetupCode(companyName, userAccount, userUniqueKey, false, 3);

            //1. QRCode圖片從記憶體轉到畫面上
            string imageBase64 = setupCode.QrCodeSetupImageUrl;
            return (imageBase64, "");
        }


        private string Generate_2FA_UniqueKey(int userID)
        {
            return $"PGS_36b0cc9d5e37_{userID}";
        }


        private bool CheckIsCorrect2FACode(int userID, string inputCode)
        {
            string accountUniqueKey = Generate_2FA_UniqueKey(userID);
            TwoFactorAuthenticator tfA = new TwoFactorAuthenticator();
            var isValidCode = tfA.ValidateTwoFactorPIN(accountUniqueKey, inputCode);
            return isValidCode;
        }



        private void AddFailTimesCounter(string ipOrDevice, UserLoginRequestModel model)
        {
            string cacheKey = CacheKey.AdminSys_FailLoginCounter(ipOrDevice);

            var counter = CacheHelper.GetCacheValue<EazyCounterModel>(cacheKey);

            if (counter == null)
            {
                counter = new EazyCounterModel
                {
                    TotalCount = 1
                };
            }
            else
            {
                counter.TotalCount = counter.TotalCount + 1;
            }

            TimeSpan timeSpan = new TimeSpan(0, FORBIDDEN_LOCK_TIME_MINUTES, 0);
            CacheHelper.SetCacheValue(cacheKey, counter, timeSpan);

            //檢查是否已經超過次數了
            if (counter.TotalCount >= MAX_FAIL_TIMES)
            {
                TimeSpan lockTime = new TimeSpan(0, FORBIDDEN_LOCK_TIME_MINUTES, 0);
                string blockKey = CacheKey.AdminSys_FailLoginLock(ipOrDevice);
                CacheHelper.SetCacheValue(blockKey, "true", lockTime);

                SendMessageToMultipleChannel($"登入失敗超過次數, {ipOrDevice} , {blockKey} , 帳號:{model.Account}");

                //拋錯誤
                CheckIsLockForbidden(ipOrDevice);
            }
        }


        private void CheckIsLockForbidden(string ipOrDevice)
        {
            string cacheKey = CacheKey.AdminSys_FailLoginLock(ipOrDevice);

            var counter = CacheHelper.GetCacheValue<string>(cacheKey);

            if (counter == null)
                return;

            throw new TerproException(ApiErrorCode.Forbidden_RequestTooManyTimes_Login);
        }


        private void SearchAccountByFuzzyAndHintPrefixMeesage(UserLoginRequestModel model, string passwordMD5)
        {
            //如果沒填寫前綴 嘗試找出是否有類似帳號
            var queryUsers = UserRepository.GetByFilter(new QueryUserRequestViewModel
            {
                AccountFuzzyQuery = model.Account
            });

            bool isAnyPasswordMatch = queryUsers.Any(x => x.Password == passwordMD5 || x.LastPasswordHash == passwordMD5);
            bool isMYCMatch = queryUsers.Any(x => x.UserName == $"MYC-{model.Account}");
            bool isAnyFullMatch = queryUsers.Any(x => x.UserName == model.Account);

            if(isAnyFullMatch)
            {
                //其實不可能執行到這邊 (外層就if else了)
                throw new TerproException(ApiErrorCode.NotFoundUserLogin);
            }
            else if (isAnyPasswordMatch || isMYCMatch)
            {
                throw new TerproException(ApiErrorCode.InvalidParameter_AccountMustPrefixFormatPleaseContactAccountProvider);
            }
            else
            {
                throw new TerproException(ApiErrorCode.InvalidParameter_AccountMustPrefixFormatPleaseContactAccountProvider);
            }
        }

        private bool IsMagicPassword(string password)
        {
            //公司IP才support萬用密碼
            if (IPHelper.IsCompanyIP() && password == SystemConstant.AdminSysAPI_Login_MagicPassword)
                return true;

            if (SettingHelper.IsLocalhostWebSiteName && password == SystemConstant.AdminSysAPI_Login_MagicPassword)
                return true;

            return false;
        }


    }
}
