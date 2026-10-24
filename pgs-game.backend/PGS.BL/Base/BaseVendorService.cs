using target.Core.Repository;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using target.Utility.Extensions;
using target.DA.Repository;
using target.Models.Table;
using Microsoft.AspNetCore.Http;
using target.Models;
using target.Utility.Helper.HttpContext;
using target.Utility;
using Lck.Utility.Extensions.Helper;
using target.Utility.Helper;
using target.Utility.IOC;
using System.Threading;

namespace target.BL.Services
{
    public class BaseVendorService : BaseService
    {

        public MerchantRepository MerchantRepository { get; set; }

        public MerchantIPAddressLogRepository MerchantIPAddressLogRepository { get; set; }


        private static Dictionary<string, bool> _tempExistIPAddress = new Dictionary<string, bool>();
        private static DateTime _tempExpiredTime = new DateTime(2024, 12, 1);


        public void CheckIsVendorAPIMaintain()
        {
            var SystemConfigSettingRepository = new SystemConfigSettingRepository();

            var queryVendorAPIMaintain = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.SystemMaintain_IsVendorAPIMaintain);

            if (SettingHelper.IsLocalhostWebSiteName)
                return;

            if (queryVendorAPIMaintain != null &&
               queryVendorAPIMaintain.SettingValueOfInt == SystemMaintainStatusEnum.SystemUuderMaintenance.ToInteger())
            {
                throw new TerproException(ApiErrorCode.SystemMaintain);
            }
        }


        public MerchantEntity GetMerchantAndCheckIsValid(int merchantID, string secret)
        {
            if (!RegexRuleHelper.IsMerchantID(merchantID))
                throw new TerproException(ApiErrorCode.InvalidParameter_IsNotValidMerchantIDFormat);

            var validModel = new BaseVendorRequestParameter
            {
                MerchantID = merchantID,
                MerchantSecretKey = secret
            };

            validModel.RuleFor(x => x.MerchantID).Rule_NotZero().Rule_NotMinus();
            validModel.RuleFor(x => x.MerchantSecretKey).Rule_NotEmptyText();

            HeaderValueHelper.AppendDebugMerchantID(merchantID);
            
            //用id + secretKey 查出供應商資料
            var queryMerchant = GetMerchantByMemoryCache(merchantID);

            if(merchantID > 0)
            {
                AddMercahntInfoToHeader(merchantID);
            }

            if (queryMerchant == null)
            {
                throw new TerproException(ApiErrorCode.NotFoundMerchant);
            }

            //透過此方式 動態改變廠商端API要顯示的語系
            if (!MyHttpContext.Current.Request.Headers.ContainsKey(SystemConstant.HeaderKey_Lang))
            {
                MyHttpContext.Current.Request.Headers.Add(SystemConstant.HeaderKey_Lang, queryMerchant.DefaultLanguage);
            }

            //測試環境允許使用簡易通關KEY
            bool isMagicPassword = [REDACTED] && secret == SystemConstant.DevEnvOnly_MagicKey;

            if (!isMagicPassword && queryMerchant.ApiSecretKey != secret)
            {
                throw new TerproException(ApiErrorCode.NotFoundMerchantOrWrongApiKey);
            }

            if (queryMerchant.Status != (int)NormalActiveStatus.Enable)
                throw new TerproException(ApiErrorCode.InvalidStatus_MerchantStatus, new ErrorDetailModel(queryMerchant.MerchantID.ToString()));

            WriteIPAddressLogForDebug(queryMerchant.MerchantID);

            CheckMerchantIPAddressIsValid(queryMerchant.MerchantID, queryMerchant.Name);

            return queryMerchant;
        }




        /// <summary>
        /// 這個轉換比例是提供 報表/撈注單API的轉換率 (e.g. MYR100的商戶 希望數字是MYR 1:1)
        /// </summary>
        protected decimal GetExchangeToOriginalRate(bool isExchangeToOriginalRate, int merchantID)
        {
            decimal defaultRate = 1;

            if (!isExchangeToOriginalRate)
                return defaultRate;

            var queryMerchant = MerchantRepository.GetSingleByCache(merchantID);

            string myCurrency = queryMerchant.Currency;

            if (myCurrency == "MYR100")
                return 100;

            if (myCurrency == CurrencyEnum.KVND.ToString() || 
                myCurrency == CurrencyEnum.KIDR.ToString() ||
                myCurrency == CurrencyEnum.KCOP.ToString() ||
                myCurrency == CurrencyEnum.KMMK.ToString())
            {
                return 1000;
            }

            return defaultRate;
        }


        private void CheckMerchantIPAddressIsValid(int merchantID, string merchantName = "")
        {
            if (!SettingHelper.IsVendorAPI)
                return;

            var merchantSettingRepository = IOCHelper.InstanceRepository<MerchantCustomizedSettingRepository>();

            var ipWhiteSetting = merchantSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.System_VendorAPI_IPAddressWhiteList_MappingTemplateSetting);

            if (ipWhiteSetting.SettingValueOfInt <= 0)
                return;

            string myIP = IPHelper.GetIP();

            if (IPHelper.IsCompanyIP())
                return;

            if (myIP == "::1")
                return;

            var ipAddressRepository = IOCHelper.InstanceRepository<IPAddressListTemplateRepository>();

            int templateID = ipWhiteSetting.SettingValueOfInt;

            var ipSetting = ipAddressRepository.GetSingleBriefByCache(templateID);

            if (ipSetting == null || string.IsNullOrWhiteSpace(ipSetting.IPAddressList))
            {
                return;
            }

            List<string> ipList = ipSetting.IPAddressList.SeperateToListString();

            var validateResult = IsValidateIPAddressByCheckOneAndTwo(ipList, myIP , merchantID);

            if (validateResult.IsValidateOK)
                return;

            bool isAlwaysNotify = true;

            //N小時內最多通知一次 不然很吵
            if (!SendIpForbiddenNotifyOrNotByCache(merchantID))
            {
                isAlwaysNotify = false;
            }
            else if(myIP == "0.0.0.0") //部分IP洗頻很久了、通知客戶又沒加白的意願 (問號)
            {
                isAlwaysNotify = false;
            }
            else if(IPHelper.IsIPv6(myIP))
            {
                //一些奇怪的IPv6
                isAlwaysNotify = false;
            }


            if(isAlwaysNotify)
            {
                var disableSetting = merchantSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.DataQuery_DisableErrorNotify_IPWhitelist);

                if(disableSetting.SettingValueOfBoolean)
                {
                    isAlwaysNotify = false;
                }
            }


            throw new TerproException(ApiErrorCode.Forbidden_IPAddressNotAllow, new ErrorDetailModel
            {
                IsAlwaysNotifySlackOnly = isAlwaysNotify,
                LogExtraInfo = $" `Hi teams, We noticed that your merchant has this new IP { myIP } who request our API (currently not in ip whitelist). Do you need add this ip to whitelist ? 您好。我们发现有新的IP { myIP } 来自您的商户调用我方API(非合法白名单已被阻挡)。请问这组新IP需要加入白名单中吗`",
                ColumnName = $"{myIP} , {merchantID} , {merchantName}"
            });
        }


        public (bool IsValidateOK, string Memo) IsValidateIPAddressByCheckOneAndTwo(List<string> whiteIPList , string myIP , int merchantID)
        {
            //Step 1. 基本ipTemplate檢查
            var valid_1 = IsValidateIPAddressOK(whiteIPList, myIP);

            if (valid_1.IsValidateOK)
                return valid_1;

            var rep = IOCHelper.InstanceRepository<MerchantCustomizedSettingRepository>();

            //Step 2. 如果沒有匹配的白名單IP 嘗試從customized檢查
            var extraIPs = rep.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.System_VendorAPI_IPAddressWhiteList_CustomizedIPs);

            if(extraIPs.SettingValue == "*")
            {
                return (true, "*");
            }

            if(!string.IsNullOrWhiteSpace(extraIPs?.SettingValue))
            {
                List<string> extraIPList = extraIPs.SettingValue.SeperateToListString();

                var valid_2 = IsValidateIPAddressOK(extraIPList, myIP);

                return valid_2;
            }

            return (false, "");
        }


        public (bool IsValidateOK, string Memo) IsValidateIPAddressOK(List<string> whiteIPList, string myIP)
        {
            bool isAllowAll = whiteIPList.Any(x => x == "*");

            if (isAllowAll)
                return (true, "");

            whiteIPList = whiteIPList.Select(x => x.Trim()).ToList();

            foreach (var eachIP in whiteIPList)
            {
                if (eachIP == myIP)
                {
                    return (true, "");
                }
                else if(eachIP.Contains("*"))
                {
                    string likeQuery = eachIP.Replace("*", "");

                    if(myIP.StartsWith(likeQuery))
                    {
                        return (true, "");
                    }
                }
            }

            return (false, "");
        }

        /// <summary>
        /// 因為早期沒有設白名單IP。但考量資安因素 還是把這項機制補上
        /// 但有些商戶這邊沒有文件紀錄IP白名單、改成用程式碼紀錄
        /// </summary>
        private void WriteIPAddressLogForDebug(int merchantID)
        {
            if (!SettingHelper.IsVendorAPI)
                return;

            var now = DateTimeHelper.GetCurrentTime();

            //超過指定的日期後 就不要再運行此邏輯了
            if (now > _tempExpiredTime)
            {
                return;
            }

            try
            {
                string ipAddress = IPHelper.GetIP();

                string tempKey = $"{merchantID}_{ipAddress}";

                //記憶體記錄過此組合了 不再處理新增
                if (_tempExistIPAddress.ContainsKey(tempKey))
                {
                    return;
                }

                _tempExistIPAddress.Add(tempKey, true);

                MerchantIPAddressLogRepository.CreateTryCatch(new MerchantIPAddressLogEntity
                {
                    IPAddress = ipAddress,
                    MerchantID = merchantID,
                    CreateTime = now
                });
            }
            catch
            {

            }
        }

        public VendorAPIRequestTimeLockerCache CheckRequestTooManyTimes(int merchantID, string parameterHash, string apiName , int maxTimesHourlyProduction = 60 , string msgHint = "")
        {
            //系統限制。同樣的請求參數 一小時內禁止請求超過10次。(避免廠商端誤用無窮迴圈)
            //每個小時的cacheKey分開。(意思就是下一個小時就會解禁)
            string dateText = DateTimeHelper.GetCurrentTime().ToString("yyyyMMddHH");

            string cacheKey = CacheKey.VendorAPI_CheckRequestTooManyTimes(merchantID, dateText, parameterHash, apiName);

            var cacheValueCounter = RedisCache_3.GetCacheValue<VendorAPIRequestTimeLockerCache>(cacheKey);

            //測試環境次數會更少
            int devMaxTimes = maxTimesHourlyProduction / 6;

            //最少有10次 避免客響客戶測試
            if(devMaxTimes <= 10)
            {
                devMaxTimes = 10;
            }

            if (cacheValueCounter != null && cacheValueCounter.Counter > devMaxTimes)
            {
                var queryMerchant = MerchantRepository.GetSingleByCache(merchantID);

                //正式商戶才發通知
                bool isProduction = queryMerchant.Type == (int)MerchantTypeEnum.Plaform_ProductionEnvironment;

                string logExtraInfo = $"{merchantID} {queryMerchant.Name} , {apiName}";

                if (isProduction)
                {
                    //如果超過允許次數了，(若是正式環境可更多次)
                    if (cacheValueCounter.Counter > maxTimesHourlyProduction)
                    {
                        bool isAlwaysNotify = true;

                        //部分商戶屢勸不聽、一直觸發上限機制
                        if (merchantID == 176324 || merchantID == 176882) {
                            isAlwaysNotify = false;
                        }

                        cacheValueCounter.Counter++;
                        RedisCache_3.SetCacheValue(cacheKey, cacheValueCounter, new TimeSpan(0, 30, 0));

                        //超過次數就不要再notify了 簡直洗頻
                        var isStopNotify = IsStopRequestTimesErrorNotifyOrAddCache(merchantID, apiName);

                        if(isStopNotify)
                        {
                            isAlwaysNotify = false;
                        }

                        if (cacheValueCounter.Counter > (maxTimesHourlyProduction + 10))
                        {
                            isAlwaysNotify = false;
                        }

                        if (apiName == "GetMerchantEnableAllGames")
                        {
                            isAlwaysNotify = false;
                        }

                        if(isAlwaysNotify)
                        {
                            var merchantCustomizedSettingRepository = IOCHelper.InstanceRepository<MerchantCustomizedSettingRepository>();

                            var disableNotify = merchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.DataQuery_DisableErrorNotify_RequestTooMany);

                            bool isDisable = disableNotify.SettingValueOfBoolean;

                            if (isDisable)
                            {
                                isAlwaysNotify = false;
                            }                        
                        }

                        Thread.Sleep(1000 * 2);

                        //vendorAPI維護就不要通知了 很吵
                        if(isAlwaysNotify)
                        {
                            var sysRep = IOCHelper.InstanceRepository<SystemConfigSettingRepository>();
                            var vendorMaintain = sysRep.GetParseModelByCache(SystemConfigSettingEnum.SystemMaintain_IsVendorAPIMaintain);

                            if(vendorMaintain.SettingValueOfInt == (int)SystemMaintainStatusEnum.SystemUuderMaintenance)
                            {
                                isAlwaysNotify = false;
                            }
                        }

                        throw new TerproException(ApiErrorCode.Forbidden_RequestTooManyTimes, new ErrorDetailModel
                        {
                            ColumnName = msgHint,
                            LogExtraInfo = logExtraInfo,
                            IsAlwaysNotify = isAlwaysNotify,
                        });
                    }
                }
                else
                {
                    throw new TerproException(ApiErrorCode.Forbidden_RequestTooManyTimes, new ErrorDetailModel
                    {
                        ColumnName = msgHint,
                        LogExtraInfo = logExtraInfo,
                        IsAlwaysNotify = false,
                    });
                }
            }

            if (cacheValueCounter == null)
            {
                cacheValueCounter = new VendorAPIRequestTimeLockerCache
                {
                    Counter = 0,
                    CreateTime = DateTimeHelper.GetCurrentTime()
                };
            }

            cacheValueCounter.Counter++;
            RedisCache_3.SetCacheValue(cacheKey, cacheValueCounter, new TimeSpan(0, 30, 0));
            return cacheValueCounter;
        }


        /// <summary>
        /// 如果首次觸發 新增cahce
        /// 下一次觸發。不再重複發通知 (N分鐘內只發通知一次）
        /// </summary>
        protected bool IsStopRequestTimesErrorNotifyOrAddCache(int merchantID , string apiName)
        {
            string cacheKey = CacheKey.VendorAPI_RequestTimesErrorTempCache(merchantID, apiName);

            var queryExist = CacheHelper.GetCacheValue<string>(cacheKey);

            if(!string.IsNullOrWhiteSpace(queryExist))
            {
                return true;
            }

            CacheHelper.SetCacheValue(cacheKey, DateTimeHelper.GetCurrentTimeTimestamp(), new TimeSpan(0, 15, 0));
            return false;
        }



        protected bool IsForbiddenToCreateAccountByVendorAPI(int merchantID)
        {
            var rep = IOCHelper.InstanceRepository<MerchantCustomizedSettingRepository>();

            var forbiddenSetting = rep.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.System_GameVendorForbiddenUseApiToCreateAccount);

            if (forbiddenSetting.SettingValueOfInt == NormalActiveStatus.Enable.ToInteger())
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// 如果機器多台 可能會不合適。但是merchant資訊修改頻率很低。還是有點幫助
        /// </summary>
        private MerchantEntity GetMerchantByMemoryCache(int merchantID)
        {
            string cacheKey = CacheKey.AdminSys_MerchantInfo(merchantID);

            TimeSpan time = CacheHelper.InstanceMemoryCacheTime();
            var cacheValue = CacheHelper.GetCacheOrQueryByMemoryCache(cacheKey, () =>
               {
                   var queryMerchant = MerchantRepository.GetSingleByCache(merchantID);
                   return queryMerchant;
               }, time);

            return cacheValue;
        }


        /// <summary>
        /// 簡單來說就是 6小時內 最多發送一次通知 (不然太吵了)
        /// </summary>
        private bool SendIpForbiddenNotifyOrNotByCache(int merchantID)
        {
            string cacheKey = CacheKey.VendorAPI_IpForbidden(merchantID);

            bool isAlreadyExist = CacheHelper.IsCacheKeyExist(cacheKey);

            if (isAlreadyExist)
                return false;

            CacheHelper.SetCacheValue(cacheKey, "XXXXXX", new TimeSpan(6, 0, 0));
            return true;
        }


        /// <summary>
        /// 透過此方式，ApiLog會獨立某個欄位記錄是哪個merchantID
        /// </summary>
        /// <param name="merchantID"></param>
        private void AddMercahntInfoToHeader(int merchantID)
        {
            if (MyHttpContext.Current.Request.Headers.ContainsKey(SystemConstant.HeaderKey_IdentityMemo))
                return;

            MyHttpContext.Current.Request.Headers.Add(SystemConstant.HeaderKey_IdentityMemo, merchantID.ToString());
        }


        protected List<int> GetMerchantPlayerIDByPlayerName(int merchantID, string playerName)
        {
            var playerRepository = new PlayerRepository();

            var queryPlayer = playerRepository.GetByPlayerNameAndMerchantID(playerName, merchantID);

            if (queryPlayer == null)
                return new List<int> { -1 };

            return queryPlayer.PlayerID.ToListCollection();
        }


    }
}
