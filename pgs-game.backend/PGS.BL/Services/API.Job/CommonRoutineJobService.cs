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
using target.DA.Repository;
using System.Collections.Concurrent;
using target.Models;
using target.Utility.Helper;
using target.Models.ViewModels.GameVendor;
using target.Utility.Utility.ThirdParty;
using System.Threading.Tasks;
using game.Entity.Game;
using System.Threading;
using target.Utility;
using target.Utility.IOC;
using target.Core.Repository;
using System.Reflection;

namespace target.BL.Services
{

    public class CommonRoutineJobService : BaseJobService, ICommonRoutineJobService
    {
        public BetRecordRepository BetRecordRepository { get; set; }

        public BetRecordTagMappingRepository BetRecordTagMappingRepository { get; set; }

        public EazyTempValueRepository EazyTempValueRepository { get; set; }

        public MerchantHierarchyRepository MerchantHierarchyRepository { get; set; }

        public OneToManyCustomizedSettingItemRepository OneToManyCustomizedSettingItemRepository { get; set; }

        public PlayerTagMappingRepository PlayerTagMappingRepository { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }


        public SystemSPRepository SystemSPRepository = new SystemSPRepository();



        public MerchantHierarchyChildMappingRepository MerchantHierarchyChildMappingRepository { get; set; }


        public string RoutineJobsEarlyMorning_Every24Hours()
        {
            //檢查代理樹狀是否異常
            _ = CheckMerchantHierarchyTreeWeiredAsync();

            //如果項目越來越多 建議改成各自非同步處理
            CheckMissPerGameLogRecords();

            //執行定義好的SP
            ExecuteStoreProcedureList(OneToManyCustomizedSettingTypeEnum.RoutineJob_StoreProcedureList_24Hours);

            //刪除某些儲存體的資料 (e.g. Elasticsearch)
            RemoveStorageData();

            return "";
        }


        public string RoutineJobs_Every8Hours()
        {
            //檢查卡房
            CheckRoomLock();

            //檢查要補救的注單
            CheckSettleFailWagers();

            //執行定義好的SP
            ExecuteStoreProcedureList(OneToManyCustomizedSettingTypeEnum.RoutineJob_StoreProcedureList_8Hours);

            return "";
        }



        public string RoutineJobs_Every30Minutes()
        {
            CheckQueueDataToRecover(); //queue資料補救

            CheckAvatarMissPerGameToRecover();

            CheckElasticSearchHealth();

            CheckElasticSearchErrorLogAlert();

            CheckLockSQLs();

            return "";
        }

        public void CheckLockSQLs()
        {
            List<string> connTypes = new List<string>
            {
                DBConnType.GameLogDB,
                DBConnType.MasterDB,

                //DBConnType.SystemLogDB,
            };

            foreach (var connDB in connTypes)
            {
                int lockCount = SystemSPRepository.GetBlockSQLs(connDB);

                if (lockCount > 0)
                {
                    string msg = $"⚠️ CheckLockSQLs 似乎有異常狀況，DB: {connDB} 可能有SQL卡住Lock ";
                    SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_important_error);
                }
            }
        }



        private void CheckElasticSearchHealth()
        {
            var ES_url = SettingHelper.ThirdParty.ElastisearchAPIUrl;

            if (string.IsNullOrWhiteSpace(ES_url))
                return;

            var indexResult = ElasticsearchHelper.GetAllIndex();

            if (!indexResult.IsOK)
            {
                string msg = $"⚠️ Elasticsearch似乎有異常狀況，網址: {indexResult.RequestUrl}";
                SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_important_error);
            }
        }


        public void CheckElasticSearchErrorLogAlert()
        {
            try
            {
                var begin = DateTimeHelper.GetCurrentTime().AddHours(-1);
                var end = DateTimeHelper.GetCurrentTime();

                var tw_day = DateTimeHelper.GetCurrentTime(8);

                bool isTuesday = tw_day.DayOfWeek == DayOfWeek.Tuesday;

                if (isTuesday)
                {
                    //這段時期常常維護 不檢查
                    if (tw_day.Hour >= 9 && tw_day.Hour <= 14)
                    {
                        return;
                    }
                }

                var targetIndex = ElasticLogTableTypeEnum.ApiRequestLogVendorAPIError;

                var indexResult = ElasticsearchHelper.GetEachApiErrorCount(targetIndex, begin, end);

                if (indexResult == null)
                    return;

                var apis = indexResult.aggregations.group_by_category.buckets;

                foreach (var api in apis)
                {
                    bool isAlert = false;

                    int alertCount = 1000;

                    if (api.key == "/v1/game-records")
                    {
                        alertCount = 5000;
                    }

                    if (api.doc_count >= alertCount)
                    {
                        string extraText = isTuesday ? "*今天星期二 " : "";
                        string msg = $"{Environment.NewLine}{extraText}{targetIndex.ToString()} 似乎異常 {api.key}，次數: {api.doc_count} 似乎不太正常";
                        SendMessageToMultipleChannel(msg);
                    }
                }

            }
            catch(Exception ex)
            {
                SendMessageToMultipleChannel($"CheckElasticSearchErrorLogAlert 發生錯誤 , {ex.GetMessageAndErrorLine()}");
            }
        }




        private void CheckAvatarMissPerGameToRecover()
        {
            if (SettingHelper.PromoteBrandType != (int)PromoteBrandTypeEnum.target)
                return;

            var queryWagers = BetRecordRepository.CheckAvatarMissPerGame();

            if (queryWagers.IsEmptyOrNull())
                return;

            string msg = $"⭐Avatar有{queryWagers.Count()}筆注單，可能已自動補救。 {string.Join("、", queryWagers.Select(x => x.BetRecordNoPGS))}";
            SendMessageToMultipleChannel(msg);

            SetSystemDefaultUserID();

            IBetRecordService BetRecordService = IOCHelper.Resolve<IBetRecordService>();

            //因為是針對Avatar 而且指定gameID了 情境都差不多那樣。
            foreach (var item in queryWagers)
            {
                BetRecordService.ManualWritePerGameLog(_defaultUserID, new BetRecordManualWritePerGameRequestModel
                {
                    IsReplaceGameTimeToNowTime = false,
                    IsUpdatePayoutAmount = false,
                    TransactionNoPGS = item.BetRecordNoPGS,
                    IsWriteSingleWalletNotifyQueue = true,
                    Memo = "Avatar遺失perGame自動補救",
                    TagID = SystemTagEnum.PerGameDetailLog_ManualWritePerGame.ToInteger(),
                });
            }
        }


        private void CheckQueueDataToRecover()
        {
            var items = EazyQueueHelper.GetMultipleItems();

            if (items.IsEmptyOrNull())
                return;

            foreach (var queueItem in items)
            {
                var dataType = queueItem.Type;

                try
                {
                    switch (dataType)
                    {
                        case EazyQueueTypeEnum.BetRecordTagMapping:
                            {
                                var insertEntity = queueItem.JsonValue.JsonConvertToModel<BetRecordTagMappingEntity>();
                                BetRecordTagMappingRepository.Create(insertEntity);
                                break;
                            }
                        case EazyQueueTypeEnum.EazyTempValue:
                            {
                                var insertEntity = queueItem.JsonValue.JsonConvertToModel<EazyTempValueEntity>();

                                if (string.IsNullOrWhiteSpace(insertEntity.Memo3))
                                {
                                    insertEntity.Memo2 = "Queue補救";
                                }

                                EazyTempValueRepository.Create(insertEntity);
                                break;
                            }
                        default:
                            break;
                    }

                    EazyQueueHelper.DeleteFromQueue(queueItem.UniqueKey);
                }
                catch (Exception ex)
                {
                    if (ex.IsSqlUniqueException())
                    {
                        EazyQueueHelper.DeleteFromQueue(queueItem.UniqueKey);
                    }
                    else
                    {
                        string msg = $"CheckQueueDataToRecover 出錯啦 UniqueKey欄位:{queueItem.UniqueKey} type:{dataType} , 此處發生error可能會持續發生、大概需要手動移除資料 , {ex.GetMessageAndErrorLine()}";
                        SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_job_api);
                    }
                }
            }
        }


        private void CheckRoomLock()
        {
            //查出資料
            //是否近4小時有投注
        }


        public void ExecuteStoreProcedureList(OneToManyCustomizedSettingTypeEnum settingType)
        {
            var queryLisfOfSP = OneToManyCustomizedSettingItemRepository.GetByFilter(new QueryOneToManyCustomizedSettingItemRequestViewModel
            {
                OneToManyCustomizedSettingType = settingType.ToInteger()
            });

            queryLisfOfSP = queryLisfOfSP
                .Where(x => x.SettingValue.IsNotEmptyTextNotNull())
                .ToList();

            if (queryLisfOfSP.IsEmptyOrNull())
                return;

            if (SettingHelper.IsMSTest)
                return;

            var spRepo = new SystemSPRepository();

            foreach (var item in queryLisfOfSP)
            {
                //非同步處理
                Task.Factory.StartNew(() =>
                {
                    var dbCategory = item.Memo; // DBConnType.MasterDB;

                    string validDbCategory = string.Empty;

                    try
                    {
                        validDbCategory = ConvertToValidDbCategoryName(dbCategory);

                        if (string.IsNullOrWhiteSpace(validDbCategory))
                            throw new TerproException(ApiErrorCode.BusinessLogicDeny, $"{dbCategory}查無對應的DB、請確認配置是否正確");

                        string spName = item.SettingValue;

                        var spResult = spRepo.ExecuteTargetProcedureAndCheckSuccess(validDbCategory, spName);

                        //若判斷SP執行失敗
                        if (!spResult.IsSuccess)
                        {
                            throw new TerproException(ApiErrorCode.BusinessLogicDeny, $"{spResult.ResultMsg}");
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = $"執行SP `{validDbCategory}` `{item.SettingValue}` 錯誤。{ex.GetMessageAndErrorLine()}";
                        SendMessageToMultipleChannel(msg);
                    }
                });
            }
        }


        private void CheckSettleFailWagers()
        {
            var targetTagID = SystemTagEnum.BetRecord_SettleProcess_NotCompleted;

            var begin = DateTimeHelper.GetCurrentTime().AddHours(-10);

            var queryFailWagers = BetRecordTagMappingRepository.GetByFilter(new QueryBetRecordTagMappingRequestViewModel
            {
                TagIDs = targetTagID.ToInteger().ToListCollection(),
                CreateTimeBegin = begin,
                CreateTimeEnd = DateTimeHelper.GetCurrentTime()
            });

            if (queryFailWagers.IsNotEmptyNull())
            {
                string msg = $"⚠️⚠️查到有 {queryFailWagers.Count()} 筆注單 結算中途失敗 。可能需要補救PerGame資料" + Environment.NewLine +
                             $" SELECT * FROM [PGS_GameLog].[dbo].[BetRecordTagMapping] Where TagID = {targetTagID.ToInteger()} AND CreateTime >= '{begin.ToString("yyyy-MM-dd")}' ";

                SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_important_error);
            }
        }

        private void RemoveStorageData()
        {

            var allEnumValues = ExtensionOfEnum.GetEnumAllValue<ElasticLogTableTypeEnum>();

            List<string> tables = new List<string>();

            int expiredDays = 30;

            if (SettingHelper.IsBWG)
            {
                expiredDays = expiredDays + 10;
            }

            DateTime expiredTime = DateTimeHelper.GetCurrentTime().AddDays(-1 * expiredDays); //時間可能會很短 不能diskSize太吃緊

            foreach (var item in allEnumValues)
            {
                tables.Add(TableNameConvertHelper.GetElasticsearchIndexName(item));
            }

            Task.Factory.StartNew(() =>
            {
                foreach (var index in tables)
                {
                    ElasticsearchHelper.DeleteDataByTime<object>(index, expiredTime);
                }
            });

        }


        private string ConvertToValidDbCategoryName(string dbName)
        {
            if (string.IsNullOrWhiteSpace(dbName))
                return string.Empty;

            string keyword = dbName.Replace("PGS_", "");

            foreach (var eachDbName in typeof(DBConnType).GetFields())
            {
                string fieldDbName = eachDbName.GetValue(null).ToString();

                if (fieldDbName.Contains(keyword))
                {
                    return fieldDbName;
                }
            }

            return string.Empty;
        }


        /// <summary>
        /// 檢查是否有perGameLog遺失的資料
        /// </summary>
        public void CheckMissPerGameLogRecords()
        {
            List<GameVendorTypeEnum> gameVendors = new List<GameVendorTypeEnum>
            {
                GameVendorTypeEnum.AGG,
                GameVendorTypeEnum.AGGLive,
                GameVendorTypeEnum.AGGFish,
            };

            //固定取昨日的時間
            double timeZone = 8;
            var begin = DateTimeHelper.GetCurrentTime(timeZone).To_00_00_00().AddDays(-1).AddHours(timeZone * -1);
            var end = begin.AddDays(1);

            //被標記帳務排除的玩家。會被排除查詢
            var excludePlayers = PlayerTagMappingRepository.GetByFilter(new QueryPlayerTagMappingRequestViewModel
            {
                TagIDs = SystemTagEnum.Player_GameVendorAcoountingIgnoreThisPlayer.ToInteger().ToListCollection()
            })
                .Select(x => x.PlayerID)
                .ToList();

            foreach (GameVendorTypeEnum gameVendor in gameVendors)
            {
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        CheckMissPerGameAndSendNotify(gameVendor, begin, end, excludePlayers);
                    }
                    catch (Exception ex)
                    {
                        string msg = $"CheckMissPerGameLogRecords 出錯啦。{gameVendor} , {ex.GetMessageAndErrorLine()}";
                        SendMessageToTelegram(msg);
                    }
                    ;
                });
            }
        }

        public async Task CheckMerchantHierarchyTreeWeiredAsync()
        {
            //只檢查1~2級代理
            var queryHierarchy = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
            {
                MinLayer = 1,
                MaxLayer = 2,
            });

            if (queryHierarchy.IsEmptyOrNull())
                return;

            await Task.Run(() =>
            {
                foreach (var eachMerchant in queryHierarchy)
                {
                    var queryCountByHierarchy = MerchantHierarchyRepository.GetCountByFilter(new QueryMerchantHierarchyRequestViewModel
                    {
                        HierarchyOfTargerMerchant = eachMerchant.MerchantID
                    });

                    var queryCountByChildMapping = MerchantHierarchyChildMappingRepository.GetCountByFilter(new QueryMerchantHierarchyChildMappingRequestViewModel
                    {
                        MerchantIDs = eachMerchant.MerchantID.ToListCollection()
                    });

                    if (queryCountByHierarchy != queryCountByChildMapping && !SettingHelper.IsMSTest)
                    {
                        string msg = $"CheckMerchantHierarchyTreeWeiredAsync 商戶:{eachMerchant.MerchantID} 樹狀比對異常{Environment.NewLine}筆數不符。{queryCountByHierarchy} vs {queryCountByChildMapping}";
                        SendMessageToMultipleChannel(msg);
                    }
                }
            });
        }


        public void CheckMissPerGameAndSendNotify(GameVendorTypeEnum gameVendor, DateTime beginUTC, DateTime endUTC, List<int> excludePlayerIDs)
        {
            var myFilter = new QueryMissPerGameExtraParameter
            {
                ExcludePlayerIDs = excludePlayerIDs,
                IsExcludeFinanceIgnore = true
            };

            var queryRecords = PerGameDetailLogRepository.GetMissPerGameLogRecords(gameVendor, beginUTC, endUTC, myFilter);

            if (queryRecords.IsNotEmptyNull())
            {
                var tagEnum = SystemTagEnum.BetRecord_MissPerGameLog;

                string msg = $"遊戲商{gameVendor.ToString()} , {beginUTC.AddHours(8).ToString("yyyy-MM-dd")} 時間，有{queryRecords.Count()} 筆注單缺少遺失perGame資料。已標記tag {tagEnum.ToInteger()}、需人工處理。(手動補perGame可能導致結算時間join不到)";

                if (queryRecords.Count() <= 5)
                {
                    msg = msg + Environment.NewLine + "單號: " + string.Join("、", queryRecords.Select(x => x.BetRecordNoPGS));
                }

                if (!SettingHelper.IsMSTest)
                {
                    NotifyToChannels(msg);
                }

                //標記tag (方便人工作業)
                foreach (var item in queryRecords)
                {
                    BetRecordTagMappingRepository.CreateIfNotExist(item.BetRecordNoPGS, tagEnum);
                }
            }
        }


        private void NotifyToChannels(string msg)
        {
            MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
            {
                Channel = SlackChannelEnum.n_pgs_job_api,
                Message = msg
            });

            MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
            {
                Message = msg,
                TelegramChannelEnum = TelegramChannelEnum.pgs_job_api
            });
        }



    }
}
