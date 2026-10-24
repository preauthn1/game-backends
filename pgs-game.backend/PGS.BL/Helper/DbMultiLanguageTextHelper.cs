using target.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using target.Utility.Extensions;
using target.Models.Models;
using target.Models.Enums;
using target.Models.Table;
using target.BL.Interface;
using target.Utility.Helper;
using Lck.Utility.Extensions.Helper;

namespace target.BL.Services
{
    public class DbMultiLanguageTextHelper
    {

        public static string GetGameDisplayName(int gameID, LanguageEnum? specificLanguage = null)
        {
            if (gameID <= 0)
                return string.Empty;

            var sourceResult = GetGameDisplayNameAndGetSource(gameID, specificLanguage);

            if (sourceResult.IsLanguageI18nKeysDefined)
                return sourceResult.GameName;

            return sourceResult.DBGameName;
        }


        /// <summary>
        /// 1.  從多語系檔取得 遊戲名稱
        /// 2.  從cache(DB設置) 取得遊戲名稱
        /// </summary>
        public static (string GameName, string DBGameName, bool IsLanguageI18nKeysDefined) GetGameDisplayNameAndGetSource(int gameID, LanguageEnum? specificLanguage = null)
        {
            var i18nText = Geti18nGameName(gameID, specificLanguage);

            if (i18nText.IsLanguageI18nKeysDefined)
                return (i18nText.i18nText, string.Empty, true);

            //嘗試從資料表languageText(DB)抓文字
            string gameDisplayName = DbValueCacheHelper.GetGameNameFromLanguageText(gameID, specificLanguage);
            return (string.Empty, gameDisplayName, false);
        }


        /// <summary>
        /// 取得多個遊戲的多語系名稱、其實目前的寫法還是仍未最佳化 (還是有大量的迴圈單次單次存取redis的問題)
        /// </summary>
        public static Dictionary<int , string> GetMultipleGamesDisplayNameDic(IEnumerable<int> gameIDs, LanguageEnum? specificLanguage = null)
        {
            gameIDs = gameIDs.Distinct().ToList();

            var queryGames = DbValueCacheHelper.GetGameNameBriefModels(gameIDs);

            var languageResult = GetMultipleGamesDisplayName(queryGames, specificLanguage);

            return gameIDs.ToDictionary(gameID => gameID, z => languageResult.FirstOrDefault(x => x.GameID == z)?.LanguageText);
        }


        public static Dictionary<int, string> GetMultipleGamesDisplayNameDic<T>(List<T> queryGames, LanguageEnum? specificLanguage = null) where T: IGamei18nName
        {
            var languageResult = GetMultipleGamesDisplayName(queryGames, specificLanguage);
            return queryGames.ToDictionary(game => game.GameID, game => languageResult.FirstOrDefault(x => x.GameID == game.GameID)?.LanguageText);
        }


        /// <summary>
        /// 取得多個遊戲的多語系名稱、其實目前的寫法還是仍未最佳化 (還是有大量的迴圈單次單次存取redis的問題)
        /// </summary>
        public static List<GameLanguageTextTempModel> GetMultipleGamesDisplayName(List<int> gameIDs, LanguageEnum? specificLanguage = null)
        {
            gameIDs = gameIDs.Distinct().ToList();

            var queryGames = DbValueCacheHelper.GetGameNameBriefModels(gameIDs);

            var languageResult = GetMultipleGamesDisplayName(queryGames , specificLanguage);
            return languageResult;
        }

        /// <summary>
        /// 取得多個遊戲的多語系名稱
        /// 目前有稍微優化。(最初的版本對於redis存取幾百次有點不佳)
        /// </summary>
        public static List<GameLanguageTextTempModel> GetMultipleGamesDisplayName<T>(List<T> queryGames, LanguageEnum? specificLanguage = null) where T: IGamei18nName
        {
            if (queryGames.IsEmptyOrNull())
                return new List<GameLanguageTextTempModel>();

            List<GameLanguageTextTempModel> listResult = new List<GameLanguageTextTempModel>();

            //筆數過多的話。改成用新版寫法 2023/01
            if(queryGames.Count() > 20)
            {
                //直接預先查出幾百個遊戲名稱的languageText (減少了大量迴圈存取)
                var preQueryDic = DbValueCacheHelper.GetAllGameNameLanguageTextDicByCache();

                foreach (var game in queryGames)
                {
                    var i18nResult = Geti18nGameName(game.GameID, specificLanguage);

                    if (i18nResult.IsLanguageI18nKeysDefined)
                    {
                        listResult.Add(new GameLanguageTextTempModel(game.GameID, i18nResult.i18nText));
                    }
                    else
                    {
                        string dbResult = DbValueCacheHelper.GetGameNameFromPreQueryLanguageTextDic(game, preQueryDic, specificLanguage);
                        listResult.Add(new GameLanguageTextTempModel(game.GameID, dbResult));
                    }
                }
            }
            else
            {
                foreach (var game in queryGames)
                {
                    var i18nResult = Geti18nGameName(game.GameID, specificLanguage);

                    if (i18nResult.IsLanguageI18nKeysDefined)
                    {
                        listResult.Add(new GameLanguageTextTempModel(game.GameID, i18nResult.i18nText));
                    }
                    else
                    {
                        string dbResult = DbValueCacheHelper.GetGameNameFromLanguageText(game, specificLanguage);
                        listResult.Add(new GameLanguageTextTempModel(game.GameID, dbResult));
                    }
                }
            }


            return listResult;
        }


        public static (bool IsLanguageI18nKeysDefined, string i18nText) Geti18nGameName(int gameID, LanguageEnum? specificLanguage = null)
        {
            //三方遊戲直接認定為需撈DB 減少一點點運算           
            if (AppHelper.IsExternalVendorGame(gameID))
            {
                return (false, string.Empty);
            }

            var gameEnum = (GameListEnum)gameID;

            //嘗試從多語系檔(i18nKeys)抓文字
            var i18nText = MultiLanguageTextHelper.GetEnumsCultureText(gameEnum, specificLanguage);

            string fieldName = MultiLanguageTextHelper.GetEnums_i18n_FieldName(gameEnum);

            bool isResourceTextUndefined = fieldName == i18nText || string.IsNullOrWhiteSpace(i18nText);

            return (!isResourceTextUndefined, i18nText);
        }


    }
}
