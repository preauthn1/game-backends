using System;
using System.Collections.Generic;
using target.BL.Interface;
using target.BL.Services;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Models.Models;
using System.Linq;
using target.DA.Repository;
using target.Models.Enums;
using target.Utility.Helper;
using Lck.Utility.Extensions.Helper;
using target.Models;
using target.Utility.Extensions.Web;
using target.Utility;

namespace target.BL.Services
{

    public class GamesService : BaseVendorService, IGamesService
    {

        public GameListRepository GameListRepository { get; set; }

        public MerchantGameSettingRepository MerchantGameSettingRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }

        public OneToManyCustomizedSettingItemRepository OneToManyCustomizedSettingItemRepository { get; set; }

        public GameListCustomizedSettingRepository GameListCustomizedSettingRepository { get; set; }


        public const int DefaultGameOrderID = SystemConstant.DefaultGameOrderID; 



        /// <summary>
        /// 取得所有遊戲列表
        /// </summary>
        public List<GameInfoViewModel> GetMerchantEnableGames(GameInfoRequestModel model)
        {
            //檢查商戶狀態合法
            this.GetMerchantAndCheckIsValid(model.MerchantID, model.MerchantSecretKey);

            //檢查 VendorAPI設置維護
            CheckIsVendorAPIMaintain();

            model.ExtraParameter = model.ExtraParameter ?? new GameListExtraParameter();

            if (SettingHelper.IsProduction || SettingHelper.IsLocalhostWebSiteName)
            {
                //預計10/22後 開始檢查
                if(DateTimeHelper.GetCurrentTime() > new DateTime(2024 , 10 , 24))
                {
                    CheckRequestTooManyTimes(model.MerchantID, model.ConvertToJsonString().MD5Hash(), "GetMerchantEnableAllGames", 31);
                }
            }

            string cacheKey = CacheKey.VendorAPI_GetMerchantEnableGames(model.MerchantID);

            List<LanguageEnum> langs = new List<LanguageEnum>
            {
                LanguageEnum.zh_cn,
                LanguageEnum.en_us
            };

            //有部分客戶在自家大廳直接對接我方API 存取次數可能會比預期高
            var queryCache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
               {
                   //查遊戲資料
                   var queryAllGames = GameListRepository.GetAllStatusEnableGames();

                   var queryGameSettings = MerchantGameSettingRepository.GetByMerchantID(model.MerchantID, NormalActiveStatus.Enable);

                   List<int> merchantAvailableGames = queryGameSettings.Select(x => x.GameID).ToList();

                   var marioGameble = queryAllGames.FirstOrDefault(x => x.GameID == SystemConstant.Mario_GameID_Gameble);

                   if(!model.ExtraParameter.IsIncludeMerchantDisableGame)
                   {
                       queryAllGames = queryAllGames
                           .Where(x => merchantAvailableGames.Contains(x.GameID))
                           .ToList();
                   }

                   if(model.ExtraParameter.FilterGameVendorIDs.IsNotEmptyNull())
                   {
                       queryAllGames = queryAllGames
                           .Where(x => model.ExtraParameter.FilterGameVendorIDs.Contains(AppHelper.GetGameVendorByGameID(x.GameID)))
                           .ToList();
                   }

                   bool isAnyMarioSlotsGames = queryAllGames
                        .Where(x => x.GameVendorType == (int)GameVendorTypeEnum.Mario && x.GameType == (int)GameTypeEnum.Slot)
                        .Any();

                   //如果有開啟mario的slots遊戲。遊戲清單需加入gameble這款特殊分類
                   if (isAnyMarioSlotsGames)
                   {
                       if(!queryAllGames.Any(z=> z.GameID == marioGameble.GameID))
                       {
                           queryAllGames.Add(marioGameble);
                       }
                   }

                   List<i18nGameNameTempViewModel> langTextMappings = new List<i18nGameNameTempViewModel>();

                   foreach (LanguageEnum lang in langs)
                   {
                       var languageTextList = DbMultiLanguageTextHelper.GetMultipleGamesDisplayName(queryAllGames, lang);

                       var convertLangModels = languageTextList
                           .Select(z => new i18nGameNameTempViewModel
                           {
                               GameID = z.GameID,
                               LanguageCode = lang.ToDashLangCode(),
                               Text = z.LanguageText
                           })
                           .ToList();

                       langTextMappings.AddRange(convertLangModels);
                   }

                   return new
                   {
                       queryEnableGames = queryAllGames,
                       langTextMappings = langTextMappings,
                   };

               }, 5);

            if(SettingHelper.IsLocalhostWebSiteName)
            {
                var qqqq = queryCache.queryEnableGames.Where(z => z.GameID == 430998).ToList();
                string qqqqqqqqqqq = "";
            }
          
            var multipleGameNameDic = DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(queryCache.queryEnableGames);
                       
            var imageSetting = DbValueCacheHelper.GetImageDomainAndStyleType(model.MerchantID);

            var convertViewModels = queryCache
                .queryEnableGames
                .Select(x =>
                {
                    var model = new GameInfoViewModel
                    {
                        GameID = x.GameID,
                        GameCode = x.GameID.ToString(), //一直有客戶誤用 很煩。直接返回gameID
                        GameName = multipleGameNameDic.GetDictionaryOrDefaultValue(x.GameID),
                        GameType = x.GameType,
                        GameTypeName = MultiLanguageTextHelper.GetEnumsCultureText((GameTypeEnum)x.GameType),
                        CreateTime = x.CreateTime,
                        ImageUrl = AppHelper.GenerateGameImageUrl(imageSetting.ImageDomainUrl, x.GameCode, imageSetting.StyleType),
                        ImageUrl_H = AppHelper.GenerateGameImageUrl(imageSetting.ImageDomainUrl, x.GameCode, GameImageStyleTypeEnum.WithPGSIconHorizontalMode),
                        IsPrePaidMode = x.IsPrePaidMode,
                    };

                    return model;
                })
                .ToList();

            SetExtraProperty(convertViewModels);

            foreach (var item in convertViewModels)
            {
                item.GameNameMappings = queryCache.langTextMappings
                    .Where(z => z.GameID == item.GameID)
                    .Select(z => z.ReturnInjecteObject<i18nGameNameViewModel>())
                    .ToList();
            }

            //預設也會排序
            if (!model.SpecialOrderSequence.HasValue)
            {
                model.SpecialOrderSequence = DefaultGameOrderID;
            }

            //若有指定特殊排序
            if (model.SpecialOrderSequence.NotNullNotZeroNotMinus())
            {
                if (model.SpecialOrderSequence >= DefaultGameOrderID && model.SpecialOrderSequence < (DefaultGameOrderID + 1000))
                {
                    var queryItems = OneToManyCustomizedSettingItemRepository.GetByMasterSettingType(OneToManyCustomizedSettingTypeEnum.PgsSystem_RecommendGamesOrder);

                    var suggestOrderGameIDs = GetRecommendGamesOrder(queryItems, model.SpecialOrderSequence.Value);

                    //遊戲排序
                    convertViewModels = convertViewModels
                        .OrderBy(x =>
                        {
                            if (suggestOrderGameIDs.Contains(x.GameID))
                            {
                                return suggestOrderGameIDs.IndexOf(x.GameID);
                            }
                            else
                            {
                                return x.GameID;
                            }
                        })
                        .ToList();
                }
            }


            return convertViewModels;
        }

       
        /// <summary>
        /// 取得pgs推薦的遊戲排序
        /// </summary>
        private List<int> GetRecommendGamesOrder(List<OneToManyCustomizedSettingItemEntity> queryItems, int specialOrderSequence)
        {
            string targetOrderID = specialOrderSequence.ToString();

            var settingValue = queryItems.Where(x => x.SubCategory == targetOrderID).FirstOrDefault()?.SettingValue;

            var parseModel = new SystemConfigSettingParseModel(0, settingValue);

            if (parseModel.SettingValueOfListInt.IsNotEmptyNull())
            {
                return parseModel.SettingValueOfListInt;
            }

            return GetDefaultGamesOrder();
        }


        private List<int> GetDefaultGamesOrder()
        {
            return new List<int>
                    {
                        290088,
                        290089,
                        290038,
                        290038,
                        290041,
                        290080,
                        290083,
                        290050,
                        290039,
                        290046,
                        290062,
                        290049,
                        290074,
                        290025,
                        290051,
                        290031,
                        290081,
                        290044
                    };
        }





        private void SetExtraProperty(List<GameInfoViewModel> viewModels)
        {
            var queryGameCustomizedSettings = GameListCustomizedSettingRepository.GetByFilter(new QueryGameListCustomizedSettingRequestViewModel
            {
                SettingKeys = new List<int>
                {
                    GameListCustomizedSettingKeyEnum.IsForbiddenDirectLaunchGame.ToInteger(),
                    GameListCustomizedSettingKeyEnum.SupportedOrientation_Landscape.ToInteger(),
                    GameListCustomizedSettingKeyEnum.SupportedOrientation_Portrait.ToInteger()
                }
            });

            var gameSettingDic = queryGameCustomizedSettings
                .GroupBy(z => z.GameID)
                .ToDictionary(x => x.Key, x => x.ToList());

            List<int> forbiddenGameIDs = GetLaunchDisabledGameIDs(queryGameCustomizedSettings);

            foreach (var item in viewModels)
            {
                item.IsLaunchAvailable = !forbiddenGameIDs.Contains(item.GameID);

                if (gameSettingDic.ContainsKey(item.GameID))
                {
                    var settings = gameSettingDic[item.GameID];

                    var supportLandscape = settings.FirstOrDefault(z => z.SettingKey == GameListCustomizedSettingKeyEnum.SupportedOrientation_Landscape.ToInteger());
                    var supportPortrait = settings.FirstOrDefault(z => z.SettingKey == GameListCustomizedSettingKeyEnum.SupportedOrientation_Portrait.ToInteger());

                    if (supportLandscape != null && supportLandscape.SettingValue == "1")
                    {
                        item.SupportedOrientation = "Landscape";
                    }
                    else if (supportPortrait != null && supportPortrait.SettingValue == "1")
                    {
                        item.SupportedOrientation = "Portrait";
                    }
                }
            }
        }


        /// <summary>
        /// 禁止透過標準launch啟動的遊戲
        /// </summary>
        private List<int> GetLaunchDisabledGameIDs(List<GameListCustomizedSettingEntity> querySettings)
        {
            List<int> resultGameIDs = new List<int>();
            resultGameIDs.AddRange(SystemConstant.DisableLaunchGameIDs);

            int targetSettingKey = (int)GameListCustomizedSettingKeyEnum.IsForbiddenDirectLaunchGame;

            var queryForbiddenGames = querySettings
                .Where(x => x.SettingKey == targetSettingKey)
                .Where(q =>
                {
                    var parse = new SystemConfigSettingParseModel(q.SettingKey, q.SettingValue);
                    return parse.SettingValueOfBoolean;
                })
                .Select(x => x.GameID)
                .ToList();

            if (queryForbiddenGames.IsNotEmptyNull())
            {
                resultGameIDs.AddRange(queryForbiddenGames);
            }

            resultGameIDs = resultGameIDs.Distinct().ToList();
            return resultGameIDs;
        }


    }
}
