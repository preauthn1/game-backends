using System;
using System.Linq;
using System.Collections.Generic;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.BL.Services;
using target.Models.Models;
using target.Models.ViewModels.Base;
using target.Utility.Helper.Dependency;
using target.Models.CultureText;
using target.Utility.Extensions.Web;

namespace target.BL.Interface
{

    public class InternalUtilityService : BaseGameService, IInternalUtilityService
    {

        public ServerListRepository ServerListRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public MerchantGameSettingRepository MerchantGameSettingRepository { get; set; }
     
        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public OneToManyCustomizedSettingItemRepository OneToManyCustomizedSettingItemRepository { get; set; }

        public GameListCustomizedSettingRepository GameListCustomizedSettingRepository { get; set; }


        public List<DemoGameCategoryViewModels> GetMerchantServers(int merchantID)
        {
            var gameCategories = ExtensionOfEnum.GetEnumAllValue<GameTypeEnum>();

            var queryAllGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                Status = NormalActiveStatus.Enable.ToInteger(),
                AssignMerchantIDs = merchantID.ToListCollection()
            });

            List<int> enableGameIDs = queryAllGames.Select(z => z.GameID).ToList();

            var queryServers = ServerListRepository.GetByFilter(new QueryServerListRequestViewModel
            {
                MerchantIDs = merchantID.ToListCollection(),
                GameIDs = enableGameIDs
            });

            var convert = queryServers.Select(x =>
            {
                var parse = x.ReturnInjecteObject<DemoServerItemViewModels>();
                parse.GameType = queryAllGames.FirstOrDefault(z => z.GameID == x.GameID)?.GameType ?? 0;
                return parse;
            });

            var gameTypeGroup = convert.GroupBy(x => x.GameType).Select(z => z.Key).ToList();
            var gameIDGroupByGameServer = convert.GroupBy(x => x.GameID).Select(z => z.Key).ToList();

            List<DemoGameItemViewModels> listOfEachGames = new List<DemoGameItemViewModels>();
            List<DemoGameCategoryViewModels> listResult = new List<DemoGameCategoryViewModels>();

            var imageSetting = DbValueCacheHelper.GetImageDomainAndStyleType(merchantID);

            var gameSettingDic = GameListCustomizedSettingRepository.GetByFilter(new QueryGameListCustomizedSettingRequestViewModel
            {
                SettingKeys = new List<int>
                {
                    GameListCustomizedSettingKeyEnum.GameInfo_RTP.ToInteger(),
                    GameListCustomizedSettingKeyEnum.GameInfo_LinesWays.ToInteger(),
                    GameListCustomizedSettingKeyEnum.GameInfo_MaximumWin.ToInteger()
                }
            })
            .GroupBy(x => x.GameID)
            .ToDictionary(x => x.Key, x => x.ToList());

            var gameNameDic = DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(enableGameIDs);

            //這區塊只有pgs自製遊戲
            foreach (int gameID in gameIDGroupByGameServer)
            {
                var matchGame = queryAllGames.FirstOrDefault(x => x.GameID == gameID);
                var instance = new DemoGameItemViewModels
                {
                    GameID = gameID,
                    GameName = gameNameDic.GetDictionaryOrDefaultValue(gameID , ""),
                    GameType = matchGame?.GameType ?? 0,
                    GameCode = matchGame?.GameCode ?? "",
                    ImageUrl = AppHelper.GenerateGameImageUrl(imageSetting.ImageDomainUrl, matchGame?.GameCode , imageSetting.StyleType),
                    Servers = convert.Where(x => x.GameID == gameID).ToList()
                };

                SetGameRTP(instance, gameSettingDic);
                listOfEachGames.Add(instance);
            }


            foreach (int gameType in gameTypeGroup)
            {
                var instance = new DemoGameCategoryViewModels
                {
                    GameType = gameType,
                    GameTypeName = MultiLanguageTextHelper.GetEnumsCultureText((GameTypeEnum)gameType),
                    Games = listOfEachGames.Where(x => x.GameType == gameType).ToList()
                };

                listResult.Add(instance);
            }

            //嘗試取得三方遊戲
            var queryEnableGames = MerchantGameSettingRepository.GetByMerchantID(merchantID, NormalActiveStatus.Enable);

            List<int> merchantEnableGameIDs = queryEnableGames.Select(z => z.GameID).ToList();

            var thirdPartyVendorGames = queryAllGames
                .Where(z => AppHelper.IsExternalVendor(z.GameVendorType))
                .Where(x => merchantEnableGameIDs.Contains(x.GameID))
                .ToList();

            if (thirdPartyVendorGames.IsNotEmptyNull())
            {
                var thirdGames = thirdPartyVendorGames.Select(z =>
                {
                    var convert = new DemoGameItemViewModels
                    {
                        GameID = z.GameID,
                        GameCode = z.GameCode,
                        GameName = gameNameDic.GetDictionaryOrDefaultValue(z.GameID, ""),
                        GameType = z.GameType,
                        ImageUrl = AppHelper.GenerateGameImageUrl(imageSetting.ImageDomainUrl, z.GameCode, imageSetting.StyleType),
                        Servers = new List<DemoServerItemViewModels>
                            {
                                new DemoServerItemViewModels
                                {
                                    ServerID = 0,
                                    ServerName = "Demo",
                                    FlagLevel = 1,
                                    GameID = z.GameID,
                                    GameType = z.GameType
                                }
                            }
                    };

                    SetGameRTP(convert, gameSettingDic);
                    return convert;
                })
                .ToList();

                List<int> allGameTypes = thirdPartyVendorGames.Select(z => z.GameType).OrderBy(z => z).Distinct().ToList();

                //補上 listResult 沒有支援的遊戲分類
                foreach (int gameType in allGameTypes)
                {
                    bool isGameTypeExist = listResult.Any(x => x.GameType == gameType);

                    if (!isGameTypeExist)
                    {
                        listResult.Add(new DemoGameCategoryViewModels
                        {
                            Games = new List<DemoGameItemViewModels>(),
                            GameType = gameType,
                            GameTypeName = MultiLanguageTextHelper.GetEnumsCultureText((GameTypeEnum)gameType)
                        });
                    }
                }

                foreach (var item in listResult)
                {
                    var matchGameTypes = thirdGames.Where(z => z.GameType == item.GameType).ToList();

                    if (matchGameTypes.IsNotEmptyNull())
                    {
                        item.Games.AddRange(matchGameTypes);
                    }
                }

            }

            //手動加入一個叫做 ALL的全部分類
            var allGamesViewModels = listResult.SelectMany(x => x.Games).ToList();

            listResult.Add(new DemoGameCategoryViewModels
            {
                Games = allGamesViewModels,
                GameType = -99,
                GameTypeName = "All"
            });

            //指定遊戲排序
            var query = OneToManyCustomizedSettingItemRepository.GetByMasterSettingType(OneToManyCustomizedSettingTypeEnum.PgsSystem_RecommendGamesOrder, true);

            var gamesOrder = query.FirstOrDefault(x=> x.SubCategory == "1001");

            if(gamesOrder != null)
            {
                List<int> orderGameIDs = new SystemConfigSettingParseModel(0, gamesOrder.SettingValue).SettingValueOfListInt;

                foreach(var item in listResult)
                {
                    item.Games = item.Games
                        .OrderBy(x =>
                        {
                            if (orderGameIDs.Contains(x.GameID))
                            {
                                return orderGameIDs.IndexOf(x.GameID);
                            }
                            else
                            {
                                return x.GameID;
                            }
                        })
                        .ToList();
                }
            }

            return listResult;

        }


        private void SetGameRTP(DemoGameItemViewModels instance, Dictionary<int, List<GameListCustomizedSettingEntity>> gameSettingDic)
        {
            if (gameSettingDic.ContainsKey(instance.GameID))
            {
                var matchSettings = gameSettingDic[instance.GameID];

                //ps. 外層需要先指定要查哪些settingKeys
                instance.RTP = matchSettings.FirstOrDefault(x => x.SettingKey == GameListCustomizedSettingKeyEnum.GameInfo_RTP.ToInteger())?.SettingValue;
                instance.LineWays = matchSettings.FirstOrDefault(x => x.SettingKey == GameListCustomizedSettingKeyEnum.GameInfo_LinesWays.ToInteger())?.SettingValue;
                instance.MaximumWin = matchSettings.FirstOrDefault(x => x.SettingKey == GameListCustomizedSettingKeyEnum.GameInfo_MaximumWin.ToInteger())?.SettingValue;

                if (instance.MaximumWin.IsNotEmptyTextNotNull() && instance.LineWays.IsNotEmptyTextNotNull())
                {
                    instance.IsShowExtraInfo = true;
                }

                if(instance.LineWays.IsNotEmptyTextNotNull() && instance.LineWays.Contains("~"))
                {
                    instance.LineWays = instance.LineWays.Replace(" Ways", "").Replace("Ways", "");
                }
            }
        }


    }
}
