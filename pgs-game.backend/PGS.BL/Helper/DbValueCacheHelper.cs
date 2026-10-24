using target.Models;
using System;
using System.Collections.Generic;
using System.Text;
using target.Utility.Extensions;
using target.Models.Models;
using target.Models.Enums;
using target.Models.Table;
using target.BL.Interface;
using target.Utility.Helper;
using Lck.Utility.Extensions.Helper;
using target.DA.Repository;
using System.Linq;

namespace target.BL.Services
{
    /// <summary>
    /// 如果常常用到 又需要存取db的cache 可以考慮放這邊統一呼叫
    /// </summary>
    public static class DbValueCacheHelper
    {
        private static GameListRepository GameListRepository { get; set; }

        private static LanguageTextRepository LanguageTextRepository { get; set; }

        private static MerchantRepository MerchantRepository { get; set; }

        private static ServerListRepository ServerListRepository { get; set; }

        private static OrganizationRepository OrganizationRepository { get; set; }

        private static SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }

        private static EventRepository EventRepository { get; set; }

        private static MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }


        static DbValueCacheHelper()
        {
            GameListRepository = new GameListRepository();
            LanguageTextRepository = new LanguageTextRepository();
            MerchantRepository = new MerchantRepository();
            ServerListRepository = new ServerListRepository();
            OrganizationRepository = new OrganizationRepository();
            SystemConfigSettingRepository = new SystemConfigSettingRepository();
            MerchantCustomizedSettingRepository = new MerchantCustomizedSettingRepository();
            EventRepository = new EventRepository();
        }

        public static Dictionary<string, string> GetAllGameNameLanguageTextDicByCache()
        {
            return LanguageTextRepository.GetAllGameNameLanguageTextDicByCache();
        }


        public static GameListEntity GetGame(int gameID)
        {
            return GameListRepository.GetSingleByCache(gameID);
        }


        public static Dictionary<int, string> GetMultipleGamesImageUrlDic(List<int> gameIDs, int? merchantID)
        {
            var queryGames = GetGameNameBriefModels(gameIDs);

            var gameImageUrlDic = GetMultipleGamesImageUrlDic(queryGames, merchantID);

            return gameIDs.ToDictionary(gameID => gameID, gameID => gameImageUrlDic.GetDictionaryOrDefaultValue(gameID));
        }


        public static Dictionary<int, string> GetMultipleGamesImageUrlDic<T>(List<T> queryGames, int? merchantID) where T : IGamei18nName
        {
            var imageSetting = GetImageDomainAndStyleType(merchantID);

            return queryGames.ToDictionary(game => game.GameID, game =>
            {
                return AppHelper.GenerateGameImageUrl(imageSetting.ImageDomainUrl, game?.GameCode , imageSetting.StyleType);
            });
        }


        /// <summary>
        /// 取得遊戲圖片的域名
        /// </summary>
        public static GameImageDomainAndStyleModel GetImageDomainAndStyleType(int? merchantID)
        {
            if (merchantID.NotNullNotZeroNotMinus())
            {
                var queryMerchantSetting = MerchantCustomizedSettingRepository.GetMultipleMerchantSettingByCache(merchantID.Value, new List<MerchantCustomizedSettingKeyEnum>
                {
                    MerchantCustomizedSettingKeyEnum.MerchantCustomized_GamePageDomainUrl,
                    MerchantCustomizedSettingKeyEnum.MerchantCustomized_GameImageDomainUrl,
                    MerchantCustomizedSettingKeyEnum.GameImage_IconStyle,
                });

                return GetImageDomainAndStyleType(queryMerchantSetting);
            }
            else
            {
                var urlValue = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.LobbyPageGameImageUrl)?.SettingValue;
                return new GameImageDomainAndStyleModel
                {
                    ImageDomainUrl = urlValue,
                    StyleType = GameImageStyleTypeEnum.Normal
                };
            }
        }


        public static GameImageDomainAndStyleModel GetImageDomainAndStyleType(List<MerchantCustomizedSettingModel> queryMerchantSetting)
        {
            var imageDomain_p1 = queryMerchantSetting.GetBySettingKey(MerchantCustomizedSettingKeyEnum.MerchantCustomized_GameImageDomainUrl);
            var imageDomain_p2 = queryMerchantSetting.GetBySettingKey(MerchantCustomizedSettingKeyEnum.MerchantCustomized_GamePageDomainUrl);
            var styleType = queryMerchantSetting.GetBySettingKey(MerchantCustomizedSettingKeyEnum.GameImage_IconStyle);

            string domainUrl = string.Empty;

            if (!string.IsNullOrWhiteSpace(imageDomain_p1.SettingValue) ||
               !string.IsNullOrWhiteSpace(imageDomain_p1.SettingValue))
            {
                domainUrl = AppHelper.GetFirstNoneEmptyValue(imageDomain_p1.SettingValue, imageDomain_p2.SettingValue) + "/game-icon/";
            }
            else
            {
                domainUrl = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.LobbyPageGameImageUrl)?.SettingValue;
            }

            return new GameImageDomainAndStyleModel
            {
                ImageDomainUrl = domainUrl,
                StyleType = (GameImageStyleTypeEnum)styleType.SettingValueOfInt
            };
        }


        /// <summary>
        /// 裡面的gameName是不含多語系的
        /// </summary>
        public static List<GameListEntity> GetGames(IEnumerable<int> gameIDs)
        {
            if (gameIDs.IsEmptyOrNull())
                return new List<GameListEntity>();

            gameIDs = gameIDs.Distinct().ToList();

            return gameIDs
                .Select(gameID => GetGame(gameID))
                .Where(query => query != null)
                .ToList();
        }


        /// <summary>
        /// 裡面的gameName是不含多語系的
        /// </summary>
        public static List<Gamei18nNameBriefModel> GetGameNameBriefModels(IEnumerable<int> gameIDs)
        {
            if (gameIDs.IsEmptyOrNull())
                return new List<Gamei18nNameBriefModel>();

            gameIDs = gameIDs.Distinct().ToList();

            if (gameIDs.Count() > 30)
            {
                //筆數太多可能查DB比迴圈處理快
                var queryGames = GameListRepository.GetByFilter(new Models.ViewModels.QueryGameListRequestViewModel
                {
                    GameIDs = gameIDs.ToList()
                }, " GameID , GameName , LanguageCode , GameCode , GameVendorType , GameType , CreateTime ");

                return queryGames.Select(x => x.ToGameNameBriefModel()).ToList();
            }
            else
            {
                return gameIDs
                    .Select(gameID => GetGame(gameID))
                    .Where(query => query != null)
                    .Select(x => x.ToGameNameBriefModel())
                    .ToList();
            }
        }


        public static List<MerchantEntity> GetMerchants(IEnumerable<int> merchantIDs)
        {
            if (merchantIDs.IsEmptyOrNull())
                return new List<MerchantEntity>();

            merchantIDs = merchantIDs.Where(z => z != 0).Distinct().ToList();

            return merchantIDs
                .Select(merchantID => MerchantRepository.GetSingleByCache(merchantID))
                .Where(query => query != null)
                .ToList();
        }

        public static List<EventEntity> GetEvents(IEnumerable<int> eventIDs)
        {
            if (eventIDs.IsEmptyOrNull())
                return new List<EventEntity>();

            eventIDs = eventIDs.Where(z => z != 0).Distinct().ToList();

            return eventIDs
                .Select(id => EventRepository.GetSingleByCache(id))
                .Where(query => query != null)
                .ToList();
        }


        public static List<ServerBriefCacheModel> GetServers(IEnumerable<int> serverIDs)
        {
            serverIDs = serverIDs.Distinct().ToList();

            return serverIDs
                .Select(merchantID => ServerListRepository.GetBriefByCache(merchantID))
                .Where(query => query != null)
                .ToList();
        }


        public static List<OrganizationEntity> GetOrgs(IEnumerable<int> orgIDs)
        {
            orgIDs = orgIDs.Distinct().ToList();

            return orgIDs
                .Select(id => OrganizationRepository.GetSingleOrgByCache(id))
                .Where(query => query != null)
                .ToList();
        }


        public static List<LanguageTextEntity> GetLanguageTextsByCache(string languageTextKey)
        {
            return LanguageTextRepository.GetByLanguageCodeByCache(languageTextKey);
        }


        /// <summary>
        /// 抓出遊戲的顯示名稱 (LanguageText資料表存的)
        /// </summary>
        public static string GetGameNameFromLanguageText(int gameID, LanguageEnum? sepcificLanguage = null)
        {
            if (gameID <= 0)
                return string.Empty;

            GameListEntity queryGame = GetGame(gameID);

            return GetGameNameFromLanguageText(queryGame, sepcificLanguage);
        }


        public static string GetGameNameFromLanguageText<T>(T queryGame, LanguageEnum? sepcificLanguage = null) where T : IGamei18nName
        {
            if (queryGame == null)
                return $"undefined_{queryGame?.GameID}";

            string languageTextKey = queryGame.LanguageCode;

            //如果沒配置languageCode 直接返回gameName欄位
            if (string.IsNullOrWhiteSpace(languageTextKey))
                return queryGame.GameName;

            //從 LanguageText 資料表 抓資料
            string gameNameText = GetTargetTextByLanguageTextTable(languageTextKey, sepcificLanguage);

            if (string.IsNullOrWhiteSpace(gameNameText))
                return queryGame.GameName;

            return gameNameText;
        }


        public static string GetGameNameFromPreQueryLanguageTextDic<T>(T queryGame, Dictionary<string, string> languageTextDic, LanguageEnum? sepcificLanguage = null) where T : IGamei18nName
        {
            if (queryGame == null)
                return $"undefined_{queryGame.GameID}";

            string languageTextKey = queryGame.LanguageCode;

            //如果沒配置languageCode 直接返回gameName欄位
            if (string.IsNullOrWhiteSpace(languageTextKey))
                return queryGame.GameName;

            var gameTextOfThis = languageTextDic
                .Where(x => x.Key.Contains(languageTextKey))
                .ToDictionary(x => x.Key, x => x.Value);

            if (gameTextOfThis.IsEmptyOrNull())
                return queryGame.GameName;

            //如果參數有指定 指定要特定某個語系的文字
            if (sepcificLanguage.HasValue)
            {
                string underLineCode = sepcificLanguage.Value.ToUnderlineLangCode();

                string fullMatchKey = $"{languageTextKey}{underLineCode}";

                var matchFirst = gameTextOfThis
                    .OrderBy(z => z.Key == fullMatchKey ? 0 : 999)
                    .FirstOrDefault();

                return matchFirst.Value;
            }

            List<LanguageEnum> languages = MultiLanguageTextHelper.GetRequiredLanguages();

            foreach (var lan in languages)
            {
                string underLineCode = lan.ToUnderlineLangCode();

                string fullMatchKey = $"{languageTextKey}{underLineCode}";

                if (gameTextOfThis.ContainsKey(fullMatchKey))
                {
                    return gameTextOfThis[fullMatchKey];
                }
            }

            return queryGame?.GameName;
        }



        private static string GetTargetTextByLanguageTextTable(string languageTextKey, LanguageEnum? sepcificLanguage = null)
        {
            //查db的部分
            var queryTexts = GetLanguageTextsByCache(languageTextKey);

            if (queryTexts.IsEmptyOrNull())
                return string.Empty;

            //如果參數有指定 指定要特定某個語系的文字
            if (sepcificLanguage.HasValue)
            {
                string underLineCode = sepcificLanguage.Value.ToUnderlineLangCode();

                var matchFirst = queryTexts
                    .OrderBy(z => z.Language == underLineCode ? 1 : 999)
                    .FirstOrDefault();

                return matchFirst.Text;
            }

            List<LanguageEnum> languages = MultiLanguageTextHelper.GetRequiredLanguages();

            foreach (var lan in languages)
            {
                string targetLang = lan.ToString();
                var targetWord = queryTexts.FirstOrDefault(x => x.Language == targetLang);

                if (targetWord != null && targetWord.Text.IsNotEmptyTextNotNull())
                {
                    return targetWord.Text;
                }
            }

            return string.Empty;
        }


    }
}
