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
using target.Models.ViewModels.API.AdminSys;
using Microsoft.Extensions.Caching.Memory;
using target.Models.Attributes;
using target.Utility.Extensions.Web;
using target.Core.Helper;
using example.invalid;
using example.invalid;
using target.Core.Repository;
using System.Text;
using target.Utility;

namespace target.BL.Services
{

    public class AdminSysInternalUtilityService : BaseAdminSysService, IAdminSysInternalUtilityService
    {

        public OneToManyCustomizedSettingItemRepository OneToManyCustomizedSettingItemRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public LanguageTextRepository LanguageTextRepository { get; set; }

        public GameListCustomizedSettingRepository GameListCustomizedSettingRepository { get; set; }

        public GeneralTagMappingRepository GeneralTagMappingRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }


        public GenerateSQLResultViewModel GenerateSqlInsertCommand(LazyCommonRequestModel filter)
        {
            filter.RuleFor(z => z.Table).Rule_NotEmptyText();

            string tableNameLower = filter.Table.ToLower();

            string resultSQL = string.Empty;

            switch (tableNameLower)
            {
                case "gamelist":

                    if (filter.GameIDs.IsNotEmptyNull())
                    {
                        StringBuilder sb = new StringBuilder();

                        foreach (var eachGame in filter.GameIDs)
                        {
                            sb.AppendLine(GenerateSQL_GameList(eachGame));
                        }

                        resultSQL = sb.ToString();
                    }
                    else
                    {
                        resultSQL = GenerateSQL_GameList((int)filter.TargetID);
                    }

                    break;
                case "gamelistcustomizedsetting":
                    if (filter.TargetID > 0)
                    {
                        filter.WhereSQL = $" WHERE SettingKey = {filter.TargetID} ";
                    }

                    resultSQL = GenerateSQL_AllData(filter.Table, filter.ExcludeColumns, filter.WhereSQL);
                    break;

                default:
                    resultSQL = GenerateSQL_AllData(filter.Table, filter.ExcludeColumns, filter.WhereSQL);
                    break;
            }

            return new GenerateSQLResultViewModel
            {
                SQL = resultSQL
            };
        }

        public FavoritSettingViewModel GetFavoritSetting(int userID, LazyCommonRequestModel filter)
        {
            var queeyItems = OneToManyCustomizedSettingItemRepository.GetByFilter(new QueryOneToManyCustomizedSettingItemRequestViewModel
            {
                OneToManyCustomizedSettingType = OneToManyCustomizedSettingTypeEnum.CopyMerchantSource_FavoritMerchants.ToInteger()
            });

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var result = new FavoritSettingViewModel();

            if (queeyItems.IsNotEmptyNull())
            {
                var merchants = queeyItems
                    .Where(z => z.SettingValue.IsNumeric())
                    .Select(x => new
                    {
                        MerchantID = Int32.Parse(x.SettingValue),
                        Memo = x.Memo
                    })
                    .ToList();

                result.CopyMerchantFavoritMerchants = new List<CopyMerchantSourceFavoritMerchantItem>();

                foreach (var item in merchants)
                {
                    var matchMerchant = MerchantRepository.GetSingleByCache(item.MerchantID);

                    result.CopyMerchantFavoritMerchants.Add(new CopyMerchantSourceFavoritMerchantItem
                    {
                        MerchantID = item.MerchantID,
                        MerchantName = matchMerchant?.Name,
                        WalletModeText = MultiLanguageTextHelper.GetEnumsCultureText((MerchantWalletModeEnum)matchMerchant.WalletMode).Replace("模式", ""),
                        Type = matchMerchant.Type,
                        TypeText = MultiLanguageTextHelper.GetEnumsCultureText((MerchantTypeEnum)matchMerchant.Type),
                        Memo = item.Memo,
                    });
                }
            }

            var queryInvisibleGameIDs = GameListCustomizedSettingRepository.GetByFilter(new QueryGameListCustomizedSettingRequestViewModel
            {
                SettingKey = GameListCustomizedSettingKeyEnum.DoNotIncludeThisGame_ForSeletedAll.ToInteger(),
                SettingValueMatch = "1"
            });

            result.InvisibleGameIDs_ForSelectedAll = queryInvisibleGameIDs.Select(x => x.GameID).ToList();

            var queryChannels = GeneralTagMappingRepository.GetByTagID(GeneralTagEnum.SkipSendNewGamePromotion.ToInteger());
            result.SkipSendNewGamePromotion_IMChannelIDs = queryChannels.Select(x => x.TargetID).ToList();
            result.IsEnableRDAdminMode = SettingKeyEnumLogicHelper.IsSuperAdminEnableEditForFiveMinutes();

            if (currentUser.IsSuperAdminOrg)
            {
                DateTime nowTW = DateTimeHelper.GetCurrentTime(8);
                string nowDateText = nowTW.ToString("yyyy-MM-dd");

                string debugSQL = $@"
-- 台灣時間轉UTC
DECLARE @begin datetime = DATEADD(HOUR , -8 , '{nowDateText} 00:00:00')
DECLARE @end   datetime = DATEADD(HOUR , -8 ,  GETDATE())
DECLARE @recent_1H   datetime = DATEADD(HOUR , -1 ,  GETUTCDATE())

SELECT TOP 100 * FROM [dbo].[ApiRequestLogExternalVendorAPIError] WITH (NOLOCK)
WHERE CreateTime >=  @begin AND CreateTime < @end -- AND DebugInfo2 = '三方單號XXXXXXXXX'

SELECT TOP 100 * FROM [dbo].[ApiRequestLogAdminSysAPIError] WITH (NOLOCK)
WHERE CreateTime >=  @begin AND CreateTime < @end AND DebugInfo != '401002' AND DebugInfo != '401001' -- AND RelativeUrl != '/users/login'
order by CreateTime desc
";

                result.DebugSQL_1 = debugSQL;
            }

            return result;
        }


        /// <summary>
        /// 這支API有給外部使用 (因此請不要提供敏感資訊)
        /// </summary>
        public CommonFavoritSettingViewModel GetCommonFavoritSetting(LazyCommonRequestModel filter)
        {
            var expiredToken = [REDACTED];

            return new CommonFavoritSettingViewModel
            {
                EazyToken = [REDACTED],
                FileName_Transfer_CN = GenerateApiDocumentFileName(LanguageEnum.zh_cn, MerchantWalletModeEnum.TransferMode).DisplayFileName,
                FileName_Transfer_EN = GenerateApiDocumentFileName(LanguageEnum.en_us, MerchantWalletModeEnum.TransferMode).DisplayFileName,
                FileName_Seamless_CN = GenerateApiDocumentFileName(LanguageEnum.zh_cn, MerchantWalletModeEnum.SingleWalletMode).DisplayFileName,
                FileName_Seamless_EN = GenerateApiDocumentFileName(LanguageEnum.en_us, MerchantWalletModeEnum.SingleWalletMode).DisplayFileName,
            };
        }


        private (string RemoteFileName, string DisplayFileName) GenerateApiDocumentFileName(LanguageEnum lang, MerchantWalletModeEnum mode)
        {
            string lang_suffix = lang == LanguageEnum.zh_cn || lang == LanguageEnum.zh_tw ? "中文" : "EN";

            string remoteFileName = "";
            string displayFileName = "";

            string brandPrefix = "target";

            if (SettingHelper.IsBWG)
            {
                brandPrefix = "BWG";
            }

            string version_suffix = "";

            if (mode == MerchantWalletModeEnum.SingleWalletMode)
            {
                version_suffix = "_" + SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.Json_ApiDocument_SeamlessDocument_Version).SettingValue;
                remoteFileName = $"{brandPrefix}_SeamlessAPI_{lang_suffix}.pdf";
                displayFileName = $"{brandPrefix}_SeamlessAPI_{lang_suffix}{version_suffix}.pdf";
            }
            else
            {
                version_suffix = "_" + SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.Json_ApiDocument_TransferDocument_Version).SettingValue;
                remoteFileName = $"{brandPrefix}_TransferAPI_{lang_suffix}.pdf";
                displayFileName = $"{brandPrefix}_TransferAPI_{lang_suffix}{version_suffix}.pdf";
            }


            return (remoteFileName , displayFileName);
        }



        public RemoteFileDownloadModel DownalodApiDocumentFile(LazyCommonRequestModel model)
        {
            model.RuleFor(x => x.Language).Rule_NotEmptyText();
            model.RuleFor(x => x.Type).Rule_NotZeroNotMinus(); //對應 MerchantWalletModeEnum
            model.RuleFor(x => x.Token).Rule_NotEmptyText();

            AppHelper.CheckEazyTokenAlreadyExpired(model.Token);

            var language = model.Language == "en-us" ? LanguageEnum.en_us : LanguageEnum.zh_cn;

            var targetFileInfo = GenerateApiDocumentFileName(language, (MerchantWalletModeEnum)model.Type);

            Stream stream = null;

            using (var client = new WebClient())
            {
                stream = client.OpenRead($"{AppHelper.StaticResourceDownloadDomain()}/cs-download/{targetFileInfo.RemoteFileName}?v=" + DateTimeHelper.GetCurrentTimeTimestamp());
            }

            if (stream == null)
                throw new TerproException(ApiErrorCode.InternalServerError);

            return new RemoteFileDownloadModel
            {
                DownloadStream = stream,
                FileName = targetFileInfo.DisplayFileName,
            };
        }


        private string GenerateSQL_GameList(int gameID)
        {
            if (gameID <= 0)
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("gameID"));

            var queryGame = GameListRepository.GetById(gameID);

            if (string.IsNullOrWhiteSpace(queryGame.LanguageCode))
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel($"{gameID} LanguageCode"));

            var queryLanguage = LanguageTextRepository.GetByFilter(new QueryLanguageTextRequestViewModel
            {
                LanguageCodeLikeQuery = queryGame.LanguageCode
            });

            var querySettings = GameListCustomizedSettingRepository.GetByFilter(new QueryGameListCustomizedSettingRequestViewModel
            {
                GameID = queryGame.GameID
            });

            var list = CheckNullAndConvertToList(queryGame);

            string sql = string.Empty;

            sql += SQLHelper.GenerateInsertDataCommandByRows(GameListRepository.GetTableName(), list) + Environment.NewLine + Environment.NewLine;

            if (queryLanguage.IsNotEmptyNull())
            {
                sql += SQLHelper.GenerateInsertDataCommandByRows(LanguageTextRepository.GetTableName(), queryLanguage) + Environment.NewLine + Environment.NewLine;
            }

            if (querySettings.IsNotEmptyNull())
            {
                sql += SQLHelper.GenerateInsertDataCommandByRows(GameListCustomizedSettingRepository.GetTableName(), querySettings) + Environment.NewLine + Environment.NewLine;
            }

            return sql;
        }

        private string GenerateSQL_SingleDataQuery(string tableName, int targetID)
        {
            var rep = new SystemSPRepository();

            var queryAll = rep.GetAllDataByTableName(tableName);

            string sql = string.Empty;

            //sql += SQLHelper.GenerateInsertDataCommandByDictionary(GameListRepository.GetTableName(), queryAll, excludeColumns) + Environment.NewLine + Environment.NewLine;

            return sql;
        }


        private string GenerateSQL_AllData(string tableName, List<string> excludeColumns, string whereSQL)
        {
            if (RegexRuleHelper.IsMatchRequestBodySqlInjectionKeyword(whereSQL, isHighLevelCheck: false))
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("sql injection"));

            var rep = new SystemSPRepository();

            var queryAll = rep.GetAllDataByTableName(tableName, whereSQL);

            string sql = string.Empty;

            sql += SQLHelper.GenerateInsertDataCommandByDictionary(tableName, queryAll, excludeColumns) + Environment.NewLine + Environment.NewLine;

            return sql;
        }



        private List<T> CheckNullAndConvertToList<T>(T query) where T : class
        {
            if (query == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            return new List<T>
            {
                query
            };
        }


    }
}
