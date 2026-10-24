using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;
using target.Models;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.Base;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Utility;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace target.Utility.Helper
{
    public class AppHelper
    {

        /// <summary>
        /// IDG AM只允許查看特定遊戲
        /// </summary>
        public static bool IsIDGVisibleGames(int gameID)
        {
            var gameVendorID = GetGameVendorByGameID(gameID);

            if (gameVendorID == (int)GameVendorTypeEnum.AGG)
                return true;

            if (gameVendorID == (int)GameVendorTypeEnum.target)
                return true;

            return false;
        }


        public static bool IsEnableSwLogMigrateToES()
        {
            if (SettingHelper.IsDev)
                return true;

            var now = DateTimeHelper.GetCurrentTime();

            bool isReleaseES = now > new DateTime(2026, 5, 26);

            return isReleaseES && SettingHelper.IsProduction;
        }


        public static HashKeyWithServerTimeModel GenerateHashKeyByServerTime(DateTime? targetTime = null)
        {
            var time_0 = DateTimeHelper.GetCurrentTime();

            if (targetTime.HasValue)
            {
                time_0 = targetTime.Value;
            }

            //先用比較寬鬆的條件 以day為單位
            var time_1 = time_0.AddDays(-1);

            string timeText_0 = time_0.ToString("yyyyMMdd");
            string timeText_1 = time_1.ToString("yyyyMMdd");

            string hashKey0 = $"{timeText_0}_Qr3V5sK9X".MD5Hash();
            string hashKey1 = $"{timeText_1}_Qr3V5sK9X".MD5Hash();

            return new HashKeyWithServerTimeModel
            {
                Time0 = time_0,
                Time1 = time_1,
                Hash0 = hashKey0,
                Hash1 = hashKey1,
            };
        }



        public static string StaticResourceDownloadDomain()
        {
            if (SettingHelper.IsMSTest)
            {
                return "http://0.0.0.0"; //內部機器一直拋530錯誤 很擾人
            }
            else
            {
                return SystemConstant.StaticResourceDownloadDomain;
            }
        }


        /// <summary>
        /// 如果花費超過指定時間 發通知
        /// </summary>
        public static void SendNotifyWhenSpendTooMuchTime(DateTime begin, DateTime end, string tagName, int seconds = 3)
        {
            double spendTime = (end - begin).TotalMilliseconds;

            if (spendTime < (seconds * 1000))
                return;

            string msg = $"{tagName} 花費太多時間, {spendTime} 毫秒";

            MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
            {
                Message = msg,
                Channel = SlackChannelEnum.n_error_pgs_common
            });
        }

        public static void SendNotifyWhenSpendTooMuchTime(DateTime begin, DateTime midTime, DateTime end, string tagName, int seconds = 3)
        {
            double spendTime = (end - begin).TotalMilliseconds;

            if (spendTime < (seconds * 1000))
                return;

            string timeText = $"{begin.ToString("HH:mm:ss")},{midTime.ToString("HH:mm:ss")},{end.ToString("HH:mm:ss")},";

            string msg = $"{tagName} 花費太多時間, {spendTime} 毫秒, {timeText}";

            MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
            {
                Message = msg,
                Channel = SlackChannelEnum.n_error_pgs_common
            });
        }



        /// <summary>
        /// 純粹用來標記程式碼
        /// </summary>
        public static string ToDoList(string memo = "")
        {
            return "";
        }

        public static bool IsSingleWalletMode(MerchantWalletModeEnum walletMode)
        {
            return walletMode == MerchantWalletModeEnum.SingleWalletMode;
        }

        public static bool IsSingleWalletMode(int walletMode)
        {
            return IsSingleWalletMode((MerchantWalletModeEnum)walletMode);
        }

        public static ChartCommonDataStatReportViewModel ConvertToChartDataModel(ChartDataConvertRequestParameterModel model, bool isExcludeZero)
        {
            string targetName = model.TargetName;

            //圓餅圖的話需要排除0
            if (isExcludeZero)
            {
                model.Items = model.Items.Where(x => x.Y_Value != 0).ToList();
            }

            if (model.IsDescOrderByValue)
            {
                model.Items = model.Items
                    .OrderByDescending(x => x.Y_Value)
                    .ToList();
            }

            return new ChartCommonDataStatReportViewModel
            {
                ChartName = model.ChartDisplayName,
                Labels = model.Items.Select(x => x.X_Label).ToList(),
                Groups = new List<string> { targetName },
                Datasets = new List<ChartCommonColorDataStatReportItemViewModel>
                {
                    new ChartCommonColorDataStatReportItemViewModel
                    {
                        GroupName = targetName,
                        DataOfValues = model.Items
                            .Select(x=> x.Y_Value)
                            .ToList(),

                        Data = model.Items
                        .Select(x=> new ChartLabelMappingValueViewModel
                        {
                            LabelName = x.X_Label,
                            Value = x.Y_Value
                        })
                        .ToList()
                    }
                }
            };
        }

        /// <summary>
        /// 換行、空格、逗號
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="isPositiveIntOnly">只會解析 正整數 而且是 int32範疇的 </param>
        /// <returns></returns>
        public static List<int> AnalyzeIDsFromRawText(string inputText, bool isPositiveIntOnly = true)
        {
            if (string.IsNullOrWhiteSpace(inputText))
                return new List<int>();

            List<string> lines = inputText.Split(new[] { Environment.NewLine, "\t\n", "\n", " ", "," }, StringSplitOptions.None)
                  .Where(x => !string.IsNullOrWhiteSpace(x))
                  .ToList();

            List<int> resultIDs = new List<int>();

            foreach (var eachRow in lines)
            {
                List<string> eachColumnText = new List<string>();

                //excel格式的話是tab符號解析
                if (eachRow.Contains("\t"))
                {
                    eachColumnText = eachRow
                        .Split('\t')
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x.Trim())
                        .Where(x => isPositiveIntOnly ? x.IsNumbericOfPositiveInt32() : x.IsNumeric())
                        .ToList();

                    var firstMatchNumber = eachColumnText.FirstOrDefault();

                    if (string.IsNullOrWhiteSpace(firstMatchNumber))
                        continue;

                    resultIDs.Add(Int32.Parse(firstMatchNumber));
                }
                else
                {
                    //逗號格式的話是 ,符號解析
                    eachColumnText = eachRow
                       .Split(',')
                       .Where(x => !string.IsNullOrWhiteSpace(x))
                       .Select(x => x.Trim())
                       .Where(x => isPositiveIntOnly ? x.IsNumbericOfPositiveInt32() : x.IsNumeric())
                       .ToList();

                    resultIDs.AddRange(eachColumnText.Select(x => Int32.Parse(x)).Distinct().ToList());
                }

            }

            return resultIDs.Distinct().ToList();
        }


        /// <summary>
        /// 傳入一段文字 (可以不用換行或逗號隔開) 擷取裡面的merchantID
        /// </summary>
        public static List<int> AnalyzeMerchantIDsByRegex(string input)
        {
            List<string> numbersList = new List<string>();

            // 使用正規表達式擷取以17開頭的六位數字
            Regex regex = new Regex(@"\b(?:17|18)\d{4}\b");
            MatchCollection matches = regex.Matches(input);

            // 將匹配的結果存入 List
            foreach (Match match in matches)
            {
                numbersList.Add(match.Value);
            }

            return numbersList
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x => x.Length == 6)
                .Select(x => Int32.Parse(x))
                .Where(x => x >= 175001 && x <= 189999)
                .Distinct()
                .ToList();
        }



        /// <summary>
        /// 默認包含 換行、逗號、空格
        /// </summary>
        public static List<string> AnalyzeTextFromRawTextBySpaceChar(string inputText, List<string> splitToken = [REDACTED]
        {
            if (string.IsNullOrWhiteSpace(inputText))
                return new List<string>();

            if (splitToken.IsEmptyOrNull())
            {
                splitToken = [REDACTED] List<string>
                {
                    " " , "," , "\t\n", "\n"
                };
            }

            List<string> eachSplit = inputText
                  .Split(splitToken.ToArray(), StringSplitOptions.None)
                  .Where(x => !string.IsNullOrWhiteSpace(x))
                  .ToList();

            List<string> results = eachSplit
                .Select(x => x.Trim())
                .Distinct()
                .ToList();

            return results;
        }

        public static List<int> InstanceLoopTimes(int maxTimes)
        {
            List<int> list = new List<int>();

            for (int i = 0; i < maxTimes; i++)
            {
                list.Add(i + 1); //ps. 放進array的數字是從1開始
            }

            return list;
        }


        public static void ExecuteMethodAndRetryMaxTimes(int retryTimes, Func<RetryLoopMethodParameter, bool> ExecuteMethod, bool isAsync = false)
        {
            ExecuteMethodAndRetryMaxTimes(new RetryLoopParameter
            {
                MaxTimes = retryTimes,
                DelayMilliseconds = 1000,
                IsAsync = isAsync,

            }, ExecuteMethod);
        }


        public static void ExecuteMethodAndRetryMaxTimes(RetryLoopParameter param, Func<RetryLoopMethodParameter, bool> ExecuteMethod)
        {
            //根據 IsAsync 決定是否非同步處理
            if (param.IsAsync && !SettingHelper.IsMSTest)
            {
                Task.Factory.StartNew(() =>
                {
                    MyExecuteMethodAndRetryMaxTimes(param, ExecuteMethod);
                });
            }
            else
            {
                MyExecuteMethodAndRetryMaxTimes(param, ExecuteMethod);
            }

            return;
        }


        /// <summary>
        /// 執行失敗N次後。也不會拋錯誤Exception
        /// 可能需要自行判斷是否拿到物件是null之類來中斷後續流程
        /// </summary>
        private static void MyExecuteMethodAndRetryMaxTimes(RetryLoopParameter param, Func<RetryLoopMethodParameter, bool> ExecuteMethod)
        {
            var loopTimes = InstanceLoopTimes(param.MaxTimes);

            foreach (int times in loopTimes)
            {
                var myParam = new RetryLoopMethodParameter
                {
                    LoopTimes = times
                };

                try
                {
                    bool isSuccess = ExecuteMethod(myParam);

                    if (isSuccess)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    param.ExtraSetting = param.ExtraSetting ?? new RetryLoopExtraSetting();

                    string msg = $"{(param.Tag ?? "")} {param.ExtraSetting.LogExtraInfo} 迴圈執行錯誤。{ex.GetMessageAndErrorLine()} ";

                    bool isNotify = false;

                    if (param.ExceptionNotifyLevel == LoopExceptionNotifyLevelEnum.AlwaysWhenException)
                    {
                        isNotify = true;
                    }
                    else if (param.ExceptionNotifyLevel == LoopExceptionNotifyLevelEnum.OnlyWhenLastTimeExceptioin && times == loopTimes.Max())
                    {
                        isNotify = true;
                    }

                    //如果有指定特定關鍵字不發通知
                    if (isNotify && param.ExtraSetting.IgnoreNotifyKeyword.IsNotEmptyNull())
                    {
                        bool isAnyKeywordMatch = param.ExtraSetting.IgnoreNotifyKeyword.Any(text => msg.Contains(text));

                        if (isAnyKeywordMatch)
                        {
                            isNotify = false;
                        }
                    }

                    if (isNotify)
                    {
                        MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
                        {
                            Message = msg,
                            TelegramChannelEnum = TelegramChannelEnum.pgs_common_error
                        });
                    }

                }

                Thread.Sleep(param.DelayMilliseconds);
            }

            return;
        }


        /// <summary>
        /// 計算特殊分組的partition邏輯
        /// 每20個1組。但中間如果有VIP編號 VIP獨立自己一組
        /// </summary>
        public static HashSet<int> GeneratePartitionNumberGroup(int startMerchantID, int endMerchantID, List<int> vipIDs)
        {
            HashSet<int> partitionIDs = new HashSet<int>();

            int plusValue = 20;
            int nextMerchantID_begin = startMerchantID;
            int nextMerchantID_end = startMerchantID + plusValue;

            while (true)
            {
                var matchVipNumbers = vipIDs.Where(vipNumber => vipNumber > nextMerchantID_begin && vipNumber <= nextMerchantID_end).ToList();

                if (matchVipNumbers.Count() >= 1)
                {
                    int firstMatchNumber = matchVipNumbers.Min();

                    nextMerchantID_begin = startMerchantID;
                    nextMerchantID_end = firstMatchNumber - 1;

                    partitionIDs.Add(nextMerchantID_end);
                    partitionIDs.Add(nextMerchantID_end + 1);

                    nextMerchantID_begin = nextMerchantID_end + 1;
                    nextMerchantID_end = nextMerchantID_begin + plusValue;
                }
                else
                {
                    partitionIDs.Add(nextMerchantID_end);

                    nextMerchantID_begin = nextMerchantID_end;
                    nextMerchantID_end = nextMerchantID_begin + plusValue;
                }

                if (nextMerchantID_end > endMerchantID)
                {
                    partitionIDs.Add(endMerchantID);
                    break;
                }
            }

            return partitionIDs;
        }

        public static string GeneratePartitionNumberSQLCommand(int startMerchantID, int endMerchantID, List<int> vipIDs)
        {
            StringBuilder sb = new StringBuilder();

            var partitionNumbers = GeneratePartitionNumberGroup(startMerchantID, endMerchantID, vipIDs);

            /*
                初始創建Partition SQL           
                CREATE PARTITION FUNCTION [PF_MerchantID_V2](int) AS RANGE LEFT FOR VALUES (175001, 375001) 
                CREATE PARTITION SCHEME [PS_MerchantID_V2] AS PARTITION [PF_MerchantID_V2] TO ([PRIMARY], [PRIMARY], [PRIMARY])
             */

            foreach (var id in partitionNumbers)
            {
                sb.AppendLine($"ALTER PARTITION SCHEME PS_MerchantID_V2 NEXT USED [Primary]");
                sb.AppendLine($"ALTER PARTITION FUNCTION PF_MerchantID_V2() SPLIT RANGE( {id} )");
            }

            return sb.ToString();
        }


        /// <summary>
        /// 有自動urlEncode、避免特殊符號造成httpGet異常 (若不需要請自行處理)
        /// </summary>
        public static string GenerateEazyExpiredToken(DateTime expiredTime)
        {
            string myEncrypt = EncryptAES.Encrypt(expiredTime.ConvertToUnixTimestamp().ToString(), SystemConstant.EazyExpiredToken_AES_KEY);
            return myEncrypt.UrlEncode();
        }


        public static void CheckEazyTokenAlreadyExpired(string encryptText)
        {
            string decrypt = "";

            try
            {
                decrypt = EncryptAES.Decrypt(encryptText, SystemConstant.EazyExpiredToken_AES_KEY);
            }
            catch
            {
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("not validate encrypt text"));
            }

            if (!decrypt.IsNumeric())
            {
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("decrypt fail."));
            }

            var expiredTS = long.Parse(decrypt);

            var currentTS = DateTimeHelper.GetCurrentTimeTimestamp();

            if (currentTS > expiredTS)
            {
                throw new TerproException(ApiErrorCode.Authorize_LinkHasExpired);
            }
        }



        public static string GenerateSimpleTokenModelSignKey(SimpleExternalTokenModel model)
        {
            string beforeMD5 = $"{model.RequestID}_{SystemConstant.JwtToken_SaltKey}_{model.ExpiredTime.ToString("yyyy-MM-dd HH:mm")}_{model.TransactionNo}";
            return beforeMD5.MD5Hash();
        }



        /// <summary>
        /// 部分報表頁匯出的excel檔案後綴字
        /// </summary>
        public static string GenerateExportFileSuffixText(DateTime? beginUTC, DateTime? endUTC, double timezone)
        {
            string begin = beginUTC.HasValue ? beginUTC.Value.AddHours(timezone).ToString("MMdd") : "";
            string end = endUTC.HasValue ? endUTC.Value.AddHours(timezone).ToString("MMdd") : "";
            //e.g. _0601_0701
            return $"_{begin}_{end}";
        }


        public static string GenerateGameIdCheckCode(int gameID, int playerID)
        {
            string beforeMD5 = $"{gameID}_{SystemConstant.JwtToken_SaltKey}_{playerID}";
            return beforeMD5.MD5Hash();
        }


        public static bool IsChineseLang(LanguageEnum lang)
        {
            return lang == LanguageEnum.zh_cn || lang == LanguageEnum.zh_tw;
        }

        public static bool IsChineseLang(string lang)
        {
            return lang == "zh_cn" || lang == "zh_tw" || lang == "zh-cn" || lang == "zh-tw";
        }

        public static LanguageEnum ConvertLangToEnum(string lang, LanguageEnum defaultLang = LanguageEnum.en_us)
        {
            lang = lang.Replace("-", "_");

            return ExtensionOfEnum.GetEnumByTextValue<LanguageEnum>(lang, defaultLang);
        }

        public static string GenerateGameLaunchRequestUUID(long id)
        {
            return $"GL_" + id.ToString();
        }


        public static string GetGameListLanguageCode(string gameCode)
        {
            return $"GameList_{gameCode}";
        }

        public static int GetLanguageOrder(string contentLang, LanguageEnum userLang)
        {
            //語系完全符合 => 最優先
            if (contentLang == userLang.ToDashLangCode())
                return 0;

            if (IsChineseLang(userLang))
            {
                //若為中文語系 => 嘗試找出 zh-cn、zh-tw
                return IsChineseLang(contentLang) ? 1 : 100;
            }
            else
            {
                if (contentLang == LanguageEnum.en_us.ToDashLangCode())
                {
                    //英文會稍微高一點
                    return 50;
                }
                else
                {
                    //都不符合 隨意
                    return 100;
                }
            }
        }

        public static string GenerateEventDescriptionPageUrl(int eventID, bool isPreview = false, string langCode = "zh-cn", string eventPageUrl = "eventDescription")
        {
            string checkCode = GenerateIntegerCheckCode(eventID);
            string preivewParam = isPreview ? "&isPreview=true" : "";
            string langParam = langCode.IsEmptyOrNull() ? "" : $"&lang={langCode}";

            return $"?page={eventPageUrl}&eventID={eventID}&checkCode={checkCode}{langParam}";
        }


        public static string GenerateIntegerCheckCode(int id)
        {
            return $"{SystemConstant.Password_SaltKey}_{id}".MD5Hash();
        }

        public static string GeneratePasswordHash(string password)
        {
            return $"{SystemConstant.Password_SaltKey}_{password}".MD5Hash();
        }

        public static string GenerateDefaultPasswordByUserName(string userName)
        {
            return $"{SystemConstant.Password_SaltKey}_Pgs_v2026_{userName}".MD5Hash().Truncate(8);
        }

        public static string GenerateDefaultPasswordByMerchantID(int merchantID)
        {
            return $"{SystemConstant.Password_SaltKey}_{merchantID}".MD5Hash().Truncate(8);
        }


        /// <summary>
        /// 部分預設密碼是有規則的。可以反推回去。若發現是這種預設密碼 顯示出來
        /// </summary>
        public static string ShwoAdminSysVisiblePassowrd(string userName , string dbPasswordHash , int merchantID)
        {
            if (IsDefaultPasswordHash(dbPasswordHash))
            {
                return SystemConstant.Default_EazyPassword;
            }
            else if (IsMerchantIDPasswordHash(merchantID, dbPasswordHash))
            {
                return GenerateDefaultPasswordByMerchantID(merchantID);
            }
            else if (IsUserNameDefaultPasswordHash(userName, dbPasswordHash))
            {
                return GenerateDefaultPasswordByUserName(userName);
            }
            else
            {
                var AMpassword = [REDACTED], dbPasswordHash);

                if (AMpassword.IsPasswordMatch)
                {
                    return AMpassword.Password;
                }
                else
                {
                    return string.Empty;
                }
            }
        }


        public static bool IsDefaultPasswordHash(string dbPasswordHash)
        {
            return GeneratePasswordHash(SystemConstant.Default_EazyPassword) == dbPasswordHash ||
                   GeneratePasswordHash("aaa111") == dbPasswordHash;
        }

        public static (bool IsPasswordMatch, string Password) IsAMDefaultPassword(string userName, string dbPasswordHash)
        {
            string AMDefaultPassword = $"{userName}".Replace("MYC-", "") + "a";
            bool isMatch = GeneratePasswordHash(AMDefaultPassword) == dbPasswordHash;

            if (isMatch)
            {
                return (true, AMDefaultPassword);
            }

            return (false, "");
        }

        public static bool IsMerchantIDPasswordHash(int merchantID, string passwordHash)
        {
            return GeneratePasswordHash(GenerateDefaultPasswordByMerchantID(merchantID)) == passwordHash;
        }

        public static bool IsUserNameDefaultPasswordHash(string userName, string passwordHash)
        {
            return GeneratePasswordHash(GenerateDefaultPasswordByUserName(userName)) == passwordHash;
        }


        public static TransactionScope InstanceTransactionScopeByLevel(IsolationLevel level = IsolationLevel.RepeatableRead)
        {
            // 若需要複習 請google 「複習資料庫的 Isolation Level 與圖解五個常見的 Race Conditions」
            // levle 1. ReadUncommitted (最寬鬆)
            // levle 2. ReadCommitted
            // levle 3. RepeatableRead 
            // levle 4. Serializable (最嚴格)
            var transactionOptions = new TransactionOptions();
            transactionOptions.IsolationLevel = level;

            return new TransactionScope(TransactionScopeOption.Required, transactionOptions);
        }


        /// <summary>
        /// 計算分子除以分母 (比例計算很常用到)
        /// </summary>
        /// <param name="numerator">分子</param>
        /// <param name="denominator">分母</param>
        /// <returns></returns>
        public static decimal CalculateRate(int numerator, int denominator)
        {
            return CalculateRate((decimal)numerator, (decimal)denominator);
        }


        public static decimal CalculateRTP_Percentage(decimal betAmount, decimal payoutAmount)
        {
            return CommonLogicHelper.CalculateRTP_Percentage(betAmount, payoutAmount);
        }

        public static decimal CalculateProfieRate_Percentage(decimal betAmount, decimal merchantProfitWithCost)
        {
            if (betAmount == 0)
                return 0;

            return Math.Round(merchantProfitWithCost / betAmount * 100, 2);
        }


        /// <summary>
        /// 計算輸贏流水
        /// </summary>
        public static decimal CalculateWinLossFlowAmount(decimal bet, decimal payout)
        {
            return (payout - bet).ToAbs();
        }

        /// <summary>
        /// 玩家視角-輸贏金額
        /// </summary>
        public static decimal CalculateWinLoss(decimal bet, decimal payout)
        {
            return (payout - bet);
        }

        /// <summary>
        /// 計算分子除以分母 (比例計算很常用到)
        /// </summary>
        /// <param name="numerator">分子</param>
        /// <param name="denominator">分母</param>
        /// <returns></returns>
        public static decimal CalculateRate(decimal numerator, decimal denominator)
        {
            if (denominator == 0)
                return 0;

            return (numerator) / (denominator);
        }


        public static EventProcessModel PerGameConvertToEventProcessModel(PerGameDetailLogEntity perGame, DateTime betTime)
        {
            return new EventProcessModel
            {
                PlayerID = perGame.PlayerID,
                GameID = perGame.GameID,
                ExtraParameter = new ExtraEventProcessParameter
                {
                    BetRecordNoPGS = perGame.TransactionNo,
                    BetAmount = perGame.BetAmount,
                    ValidAmount = perGame.ValidAmount,
                    PayoutAmount = perGame.PayoutAmount,
                    BetTime = betTime,
                    SettleTime = perGame.GameTime,
                }
            };
        }

        public static string ReplaceApiDocumentFileName(string myFileName)
        {
            if (SettingHelper.PromoteBrandTypeEnum != PromoteBrandTypeEnum.target)
            {
                var replaceName = SettingHelper.PromoteBrandCodeName;
                return myFileName.Replace("PGS_", $"{replaceName}_");
            }

            return myFileName;
        }

        public static string GenerateGameListDownloadUrl()
        {
            string relativeUrl = "/cs-download/GameList_Slots+Live.xlsx";

            if (SettingHelper.PromoteBrandTypeEnum == PromoteBrandTypeEnum.BWG)
            {
                relativeUrl = "/cs-download/GameList_BWG.xlsx";
            }

            string myUrl = AppHelper.StaticResourceDownloadDomain() + relativeUrl + "?v=" + DateTimeHelper.GetCurrentTimeTimestamp();
            return myUrl.ReplaceDuplicateUrlChar();
        }


        public static string GenerateGameImageUrl(string imageDomain, string pgs_game_code, GameImageStyleTypeEnum iconStyle = GameImageStyleTypeEnum.Normal)
        {
            pgs_game_code = pgs_game_code ?? "";

            //imageDomain 是透過 SystemConfigSettingEnum.LobbyPageGameImageUrl 配置拿到
            if (iconStyle == GameImageStyleTypeEnum.WithPGSIcon)
            {
                imageDomain = ReplaceGameImageFolderToIconVersion(imageDomain, "/target-game-icon");
            }
            else if (iconStyle == GameImageStyleTypeEnum.WithPGSIconHorizontalMode)
            {
                imageDomain = ReplaceGameImageFolderToIconVersion(imageDomain, "/target-game-icon-h");
            }
            else if (iconStyle == GameImageStyleTypeEnum.WithBWGIcon)
            {
                imageDomain = ReplaceGameImageFolderToIconVersion(imageDomain, "/bwg-game-icon");
            }

            return ($"{imageDomain}/{pgs_game_code.ToLower()}.png").ReplaceDuplicateUrlChar();
        }


        /// <summary>
        /// 根據順序取得第一個非空字串的文字
        /// </summary>
        public static string GetFirstNoneEmptyValue(params string[] listOfText)
        {
            if (listOfText.IsEmptyOrNull())
                return string.Empty;

            foreach (var text in listOfText)
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }

            return string.Empty;
        }


        /// <summary>
        /// 將路徑取代成 有pgs-logo的圖片路徑
        /// </summary>
        public static string ReplaceGameImageFolderToIconVersion(string imageDomain, string replaceTo)
        {
            return imageDomain.Replace("/game-icon", replaceTo);
        }

        public static string GenerateLobbyRedirectPortalUrl(string lobbyPageUrl, string backUrlBase64)
        {
            return $"{lobbyPageUrl}?page=redirectportal&backurlBase64={backUrlBase64}".UrlEncode();
        }


        /// <summary>
        /// 判斷是否為 外部對接的遊戲商遊戲
        /// </summary>
        public static bool IsExternalVendorGame(int gameID)
        {
            return gameID > 99999;
        }


        /// <summary>
        /// 判斷是否為 PGS自製的 [Slot] 遊戲
        /// </summary>
        public static bool IsPgsSelfGameButNotSlotsType(int gameID, int gameType)
        {
            return IsPgsSelfGame(gameID) && gameType != GameTypeEnum.Slot.ToInteger();
        }


        /// <summary>
        /// 判斷是否為 PGS自製的 [Slot] 遊戲
        /// </summary>
        public static bool IsPgsSelfSlotGame(int gameID, int gameType)
        {
            return IsPgsSelfGame(gameID) && gameType == GameTypeEnum.Slot.ToInteger();
        }


        /// <summary>
        /// 判斷是否為 PGS自製的 [棋牌] 遊戲
        /// </summary>
        public static bool IsPgsSelfBoardGame(int gameID, int gameType)
        {
            return IsPgsSelfGame(gameID) && gameType == GameTypeEnum.BoardGame.ToInteger();
        }

        /// <summary>
        /// 判斷是否為 PGS自製的遊戲 
        /// 包含slot88系列的也算在內
        /// </summary>
        public static bool IsPgsSelfGame(int gameID)
        {
            return gameID > 0 && !IsExternalVendorGame(gameID);
        }

        public static string ConvertCNY_To_RMB(string currency)
        {
            if (currency == "CNY")
                return "RMB";

            return currency;
        }

        /// <summary>
        /// 判斷是否為 外部對接的遊戲商
        /// </summary>
        public static bool IsExternalVendor(int vendorID)
        {
            if (vendorID == GameVendorTypeEnum.target.ToInteger() ||
                vendorID == GameVendorTypeEnum.Slot88.ToInteger())
            {
                return false;
            }

            return true;
        }


        public static bool IsExternalVendorApiLogTableName(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return false;

            return tableName.ContainsWihtoutCase("ExternalVendorAPI") && !tableName.ContainsWihtoutCase("error");
        }


        public static string GetOutboundIPAddress()
        {
            var apiResult = APIRequestHelper.RequestApiResult<string>(new RequestParameters
            {
                BaseUrl = "https://example.invalid?format=json",
                ApiUrl = APIRequestHelper.No_Url_Keyword,
                Method = HttpMethodType.HttpGet
            });

            return apiResult.ResponseText;
        }


        /// <summary>
        /// 取得仍正常支援(未下架)的遊戲商
        /// </summary>
        public static List<GameVendorTypeEnum> GetStillAvailableGameVendors()
        {
            var allEnums = ExtensionOfEnum.GetEnumAllValue<GameVendorTypeEnum>();

            //BWG 僅顯示很少部分遊戲商
            if (SettingHelper.PromoteBrandTypeEnum == PromoteBrandTypeEnum.BWG)
            {
                return allEnums
                    .Where(z => z != GameVendorTypeEnum.CrazyFishing)
                    .Where(z => z != GameVendorTypeEnum.YesGame)
                    .Where(z => z != GameVendorTypeEnum.GPI)
                    .Where(z => z != GameVendorTypeEnum.AGGFish)
                    .Where(z => z != GameVendorTypeEnum.ATGames)
                    .Where(z => z != GameVendorTypeEnum.AGG)
                    .Where(z => z != GameVendorTypeEnum.AGGLive)
                    .Where(z => z != GameVendorTypeEnum.Jili)
                    .Where(z => z != GameVendorTypeEnum.Mario)
                    .Where(z => z != GameVendorTypeEnum.target)
                    .Where(z => z != GameVendorTypeEnum.AT)
                    .Where(z => z != GameVendorTypeEnum.AGin)
                    .Where(z => z != GameVendorTypeEnum.Galaxy)
                    .ToList();
            }
            else
            {
                //target
                allEnums = allEnums
                    .Where(z => z != GameVendorTypeEnum.CrazyFishing) //已下架
                    .Where(z => z != GameVendorTypeEnum.YesGame)        //已下架
                    .Where(z => z != GameVendorTypeEnum.GPI)          //已下架
                    .Where(z => z != GameVendorTypeEnum.AGGFish)      //已下架    
                    .Where(z => z != GameVendorTypeEnum.ATGames)      //已下架   
                    .Where(z => z != GameVendorTypeEnum.ATGames)      //已下架      
                    .Where(z => z != GameVendorTypeEnum.ATGames)      //已下架                                                                        
                                                                      //.Where(z => z != GameVendorTypeEnum.AGGLive)      //已下架
                                                                      //.Where(z => z != GameVendorTypeEnum.Galaxy)       //
                    .ToList();

                if (SettingHelper.IsProduction)
                {
                    //這些遊戲不再出現在PGS正式環境
                    allEnums = allEnums
                        .Where(z => z != GameVendorTypeEnum.ClotPlay)
                        .Where(z => z != GameVendorTypeEnum.RG)
                        .Where(z => z != GameVendorTypeEnum.SlotMania)
                        .Where(z => z != GameVendorTypeEnum.AGin)      //停止合作
                        .Where(z => z != GameVendorTypeEnum.AT)        //停止合作
                        .ToList();
                }

                return allEnums;
            }

        }

        public static string GenerateCopyInfoAndForwardToGameVendor(GameVendorTypeEnum gameVendor,
            bool isCS_role,
            string assignOperatorID,
            string currency,
            int playerID,
            GameVendorCopyInfoExtraTextModel extraInfo)
        {
            string extraText = "";

            if (gameVendor == GameVendorTypeEnum.SlotMania)
            {
                extraText = " (AppID)";
            }

            var gameVendorDef = GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(gameVendor);

            string myLastTokenText = "";

            if (extraInfo.LastSessionTokenQuery != null && AppHelper.GetGameVendorByGameID(extraInfo.LastSessionTokenQuery.GameID) == (int)gameVendor)
            {
                myLastTokenText = extraInfo.LastTokenText + Environment.NewLine;
            }

            var IMChannleHint = "";
            var vendorConfigExtraInfo = "";

            if (!isCS_role)
            {
                vendorConfigExtraInfo = $"{extraInfo.VendorConfigInfo}{Environment.NewLine}";
            }

            if (isCS_role && !string.IsNullOrWhiteSpace(gameVendorDef.IMChannelName))
            {
                IMChannleHint = "遊戲問題請轉至 ⭐" + gameVendorDef.IMChannelName + Environment.NewLine +
                                "遊戲問題請轉至 ⭐" + gameVendorDef.IMChannelName + Environment.NewLine + Environment.NewLine;
            }

            string resultText = IMChannleHint + $"服务器代号：{assignOperatorID}{extraText}{Environment.NewLine}";

            if (!string.IsNullOrWhiteSpace(vendorConfigExtraInfo))
            {
                resultText += $"{vendorConfigExtraInfo}";
            }

            resultText += $"貨幣：{currency} {extraInfo.WalletModeInfo}{Environment.NewLine}" +
                $"玩家ID：{playerID} (可能对应贵系统的玩家帐号、玩家昵称){Environment.NewLine}" + myLastTokenText;

            if (!string.IsNullOrWhiteSpace(extraInfo.ThirdPartyBetRecordNo))
            {
                resultText += $"投注单号：{extraInfo.ThirdPartyBetRecordNo} {AddBracketsSymbolIfNotEmptyText(gameVendorDef.WagerIdHint)}{Environment.NewLine}";
                resultText += $"{SettingHelper.PromoteBrandCodeName}单号：(游戏商可无视此栏位) {extraInfo.BetRecordNoPGS}{Environment.NewLine}";
            }

            resultText += "-------------------------------" + Environment.NewLine;
            return resultText;
        }

        public static string GenerateWagerCopyExtraInfo(BetRecordGeneralColumnViewModel generalInfo)
        {
            string finalText = string.Empty;
            finalText += $"游戏名称：" + generalInfo.GameName + " (GameCode: " + generalInfo.ThirdPartyGameCode + ")" + Environment.NewLine;
            finalText += $"游玩时间：" + generalInfo.CreateTime.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss") + " (GMT+8)" + Environment.NewLine;
            finalText += $"投注金额：" + generalInfo.BetAmount + Environment.NewLine;

            if(generalInfo.Status == BetRecordStatusEnum.SettleComplete.ToInteger())
            {
                finalText += $"派奖金额：" + generalInfo.PayoutAmount + Environment.NewLine;
            }
            else
            {
                finalText += $"注單狀態：" + MultiLanguageTextHelper.GetEnumsCultureText((BetRecordStatusEnum)generalInfo.Status) + Environment.NewLine;
            }

            finalText += $"-------------------------------" + Environment.NewLine;
            return finalText;
        }


        public static string AddBracketsSymbolIfNotEmptyText(string myText)
        {
            if (string.IsNullOrWhiteSpace(myText))
                return "";

            return $"({myText})";
        }


        public static void SetSeamlessCallbackHeaderInfo(RequestParameters para, int merchantID, string merchantXToken, string requestUUID)
        {
            para.AddHeaderData("X-target-MerchantID", merchantID.ToString());
            para.AddHeaderData("X-target-IDENTITY", merchantXToken);
            para.AddHeaderData("X-PROVIDER-MerchantID", merchantID.ToString());
            para.AddHeaderData("X-PROVIDER-IDENTITY", merchantXToken);

            para.AddHeaderData("X-PROVIDER-REQUEST-ID", requestUUID);
            para.AddHeaderData("X-PROVIDER-REQUEST-BEGIN", DateTimeHelper.GetCurrentTime(8).ToString("MM-dd HH:mm:ss.fff"));
            para.AddHeaderData("X-PROVIDER-SOURCE", SettingHelper.WebSiteName);
        }


        public static void SetSeamlessCallbackHeaderInfo_QTech(RequestParameters para, int merchantID, string merchantXToken, string requestUUID)
        {
            //para.AddHeaderData("Session-Token", merchantID.ToString());
            para.AddHeaderData("Pass-Key", merchantXToken);

            para.AddHeaderData("X-PROVIDER-MerchantID", merchantID.ToString());
            para.AddHeaderData("X-PROVIDER-REQUEST-BEGIN", DateTimeHelper.GetCurrentTime(8).ToString("MM-dd HH:mm:ss.fff"));
            para.AddHeaderData("X-PROVIDER-SOURCE", SettingHelper.WebSiteName);
        }


        public static List<GameVendorTypeEnum> GetReportAvailableGameVendors()
        {
            var allEnums = GetStillAvailableGameVendors();

            return allEnums
                .Where(z => z != GameVendorTypeEnum.Slot88) //排除掉報表上不需要顯示的
                .ToList();
        }


        public static int ConvertSlot88ToPgsVendorID(int vendorID)
        {
            //如果是slot88 歸類在pgs中檢查
            if (vendorID == GameVendorTypeEnum.Slot88.ToInteger())
            {
                return GameVendorTypeEnum.target.ToInteger();
            }

            return vendorID;
        }


        public static bool IsLobbyGameID(int gameID)
        {
            var modValue = gameID % 1000;

            if (modValue == 999 || modValue == 998)
                return true;

            return false;
        }

        public static int GetGameIdByServerID(int serverID)
        {
            if (serverID <= 100000000)
                return 0;

            int gameID = ((int)serverID / 100000);
            return gameID;
        }

        public static bool IsLiveCasinoGameVendors(int gameID)
        {
            var gameVendor = GetGameVendorByGameID(gameID);

            if (gameVendor == (int)GameVendorTypeEnum.AGGLive ||
               gameVendor == (int)GameVendorTypeEnum.Galaxy)
            {
                return true;
            }

            return false;
        }


        public static int GetGameVendorByGameID(int gameID)
        {
            if (gameID <= 9999)  //請不要改掉此邏輯 slot88無視
                return GameVendorTypeEnum.target.ToInteger();

            var gameVendorID = (GameVendorTypeEnum)(gameID / 10000 * 10000);

            if (gameVendorID.IsDefinedValue())
                return gameVendorID.ToInteger();

            var maxID = ExtensionOfEnum.GetEnumAllValue<GameVendorTypeEnum>().OrderByDescending(x => x.ToInteger()).FirstOrDefault();

            //新加遊戲商的時候這邊常常錯誤 (因為code還沒合回去)
            if (gameVendorID <= (maxID + 100000))
                return gameVendorID.ToInteger();

            throw new TerproException(ApiErrorCode.NotFoundGameVendor);
        }


        public static (bool IsDefine, MerchantCustomizedSettingKeyEnum SettingKey) GetConvertRateCustomizedSettingKey(int vendorID)
        {
            var result = GetConvertRateCustomizedSettingKey((GameVendorTypeEnum)vendorID);
            return result;
        }

        /// <summary>
        /// 取得轉換比例的 配置enum編號
        /// </summary>
        public static (bool IsDefine, MerchantCustomizedSettingKeyEnum SettingKey) GetConvertRateCustomizedSettingKey(GameVendorTypeEnum vendorID)
        {
            if (vendorID == GameVendorTypeEnum.target || vendorID == GameVendorTypeEnum.Slot88)
                return (true, MerchantCustomizedSettingKeyEnum.GameVendor_ConvertRate_PGS);

            //編號的規則是 9000000 + 遊戲商ID
            int expectSettingID = 9000000 + vendorID.ToInteger();
            var convertToEnum = (MerchantCustomizedSettingKeyEnum)expectSettingID;

            if (!convertToEnum.IsDefinedValue())
                return (false, MerchantCustomizedSettingKeyEnum.GameVendor_ConvertRate_PGS);

            return (true, convertToEnum);
        }

        /// <summary>
        /// 簡體字轉繁體
        /// </summary>
        public static string SimplifiedToTraditional(string text)
        {
            if (text.IsEmptyOrNull())
                return text;

            return ChineseConverter.Convert(text, ChineseConversionDirection.SimplifiedToTraditional);//转繁体
        }

        /// <summary>
        /// 繁體轉簡體
        /// </summary>
        public static string TraditionalToSimplified(string text)
        {
            return ChineseConverter.Convert(text, ChineseConversionDirection.TraditionalToSimplified);//转繁体
        }

        public static string ReplaceUrlToNewDomainHost(string originalUrl, string keywordForReplace)
        {
            Uri originalURI = new Uri(originalUrl);
            Uri keywordForReplaceURI = new Uri(keywordForReplace);

            originalUrl = originalUrl.Replace(originalURI.Host, keywordForReplaceURI.Host);
            return originalUrl;
        }

        /// <summary>
        /// 取得代理樹狀層級 (從1開始=總代)
        /// </summary>
        public static int GetAgentLayerLevelByHierarchy(string hierarchy)
        {
            var eachLayer = hierarchy
                .Split('/')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            return eachLayer.Count();
        }

        public static List<int> GetHierarchyMerchantIDs(string hierarchy)
        {
            var eachLayer = hierarchy
                .Split('/')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            return eachLayer.Select(x => Int32.Parse(x)).ToList();
        }


        public static bool IsTelegramIMChannelID(string imChannelID)
        {
            if (!imChannelID.IsNumeric())
                return false;

            int imChannelIDValue = Int32.Parse(imChannelID);

            if (imChannelIDValue >= 10000 && imChannelIDValue <= 19999)
                return true;

            return false;
        }

        public static bool IsSkypeIMChannelID(string imChannelID)
        {
            if (!imChannelID.IsNumeric())
                return false;

            int imChannelIDValue = Int32.Parse(imChannelID);

            if (imChannelIDValue >= 20000 && imChannelIDValue <= 29999)
                return true;

            return false;
        }


        public static bool IsHierarchyChildOfTargetMerchant(string hierarchy, List<int> merchantIDs)
        {
            if (merchantIDs.IsEmptyOrNull())
                return false;

            foreach (var merchantID in merchantIDs)
            {
                string keyword = GenerateHierarchyLikeKeyword(merchantID);
                if (hierarchy.Contains(keyword))
                {
                    return true;
                }
            }

            return false;
        }
        public static int CalculateTotalPage(int totalCount, int pageSize)
        {
            int totalPage = ((int)totalCount / pageSize);

            if (totalCount % pageSize == 0)
                return totalPage;

            return totalPage + 1;
        }


        public static string GenerateMultipleHierarchyMerchantFilterWhereSQL(string filterColumnName = "MerchantID", string prefixText = " AND ")
        {
            return @$" {prefixText} {filterColumnName} in 
                        (
                            SELECT ChildMerchantID 
                            FROM [PGS_Master].[dbo].[MerchantHierarchyChildMapping] WITH (NOLOCK)
                            WHERE MerchantID in @FilterMerchantIDsByMultipleHierarchyMerchant
                        ) ";
        }

        public static string GenerateMultipleHierarchyMerchantFilterWhereSQL_V2026(string filterColumnName = "MerchantID", string prefixText = " AND ")
        {
            return @$" {prefixText} 
                      ( {filterColumnName} in 
                        (
                            SELECT ChildMerchantID 
                            FROM [PGS_Master].[dbo].[MerchantHierarchyChildMapping] WITH (NOLOCK)
                            WHERE MerchantID in @FilterMerchantIDsByMultipleHierarchyMerchant
                        ) 
                        OR 
                       {filterColumnName} in @FilterMerchantIDsByMultipleHierarchyMerchant
                      ) ";
        }

        public static string GenerateMerchantHierarchyWhereSQL(int merchantID, string filterColumnName = "MerchantID")
        {
            //防呆。如果傳0是很可怕的事情。(會查到不該查的資料、查到別人家的資料)
            if (merchantID <= 0)
            {
                merchantID = 99999999;
            }

            string keyword = GenerateHierarchyLikeKeyword(merchantID);
            return @$" AND {filterColumnName} in (
                            SELECT MerchantID from [PGS_Master].[dbo].[MerchantHierarchy] WITH (NOLOCK)
                            WHERE Hierarchy LIKE CONCAT('%', '{keyword}' ,'%') 
                       ) ";
        }

        public static string GenerateMerchantHierarchyWhereSQL_V2(int merchantID, string filterColumnName = "MerchantID")
        {
            return @$" AND {filterColumnName} in (
                            SELECT ChildMerchantID from [PGS_Master].[dbo].[MerchantHierarchyChildMapping] WITH (NOLOCK)
                            WHERE MerchantID = {merchantID}
                       ) ";
        }



        public static string GenerateHierarchyLikeKeyword(int merchantID)
        {
            return $"{merchantID}/";
        }


        /// <summary>
        /// 適用情境: 若只篩選一筆MerchantID 或 少數幾家 (則代表只查特定商戶)
        /// </summary>
        public static List<int> SetListToEmptyWhenLargeFilter(List<int> merchantIDs, int maxLength = 10)
        {
            if (merchantIDs.IsEmptyOrNull())
                return merchantIDs;

            //如果超過10筆merchantIDs 就返回空字串(視為直接使用 hierarchy去篩選IDs)
            if (merchantIDs.Count() >= maxLength)
                return new List<int>();

            //保留原本的merchantIDs (可能的情境是 畫面端還是選擇指定要篩選特定某一家商戶)
            return merchantIDs;
        }

        public static (string Title, string Content) BindTemplateContentParameterValue(string jsonParameter, ContentTemplateParameterBindModel playerInfo, ContentBlockEntity matchContent)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(jsonParameter))
            {
                parameters = jsonParameter.TryJsonConvertToModel<Dictionary<string, object>>();
            }

            //基本用戶資訊
            foreach (var item in playerInfo.GetType().GetProperties())
            {
                string proprty = item.Name;
                if (!parameters.ContainsKey(proprty))
                {
                    parameters.Add(proprty, item.GetValue(playerInfo));
                }
            }

            string title = matchContent.Title;
            string content = matchContent.Description;

            foreach (var item in parameters)
            {
                string replaceColumnName1 = "{" + item.Key.ToLower() + "}";
                string replaceColumnName2 = "{" + item.Key.ToCamelCase() + "}";
                string replaceColumnName3 = "{" + item.Key + "}";
                string replaceValue = item.Value.ToString();

                title = title.Replace(replaceColumnName1, replaceValue);
                title = title.Replace(replaceColumnName2, replaceValue);

                content = content.Replace(replaceColumnName1, replaceValue);
                content = content.Replace(replaceColumnName2, replaceValue);
                content = content.Replace(replaceColumnName3, replaceValue);
            }

            return (title, content);
        }

    }


}
