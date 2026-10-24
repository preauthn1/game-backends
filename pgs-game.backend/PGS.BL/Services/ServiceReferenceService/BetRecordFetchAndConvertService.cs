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
using target.Utility.Helper;
using target.Utility;
using target.Models.ViewModels.GameVendor;
using target.Utility.Utility.ThirdParty;
using target.Utility.IOC;

namespace target.BL.Services
{

    public class BetRecordFetchAndConvertService : BaseService, IBetRecordFetchAndConvertService
    {

        public PlayerRepository PlayerRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public BetRecordTransactionNoMappingRepository BetRecordTransactionNoMappingRepository { get; set; }

        public BetRecordExtraInfoRepository BetRecordExtraInfoRepository { get; set; }

        public BetRecordTagMappingRepository BetRecordTagMappingRepository { get; set; }

        public ThirdPartyFetchRecordRepository ThirdPartyFetchRecordRepository { get; set; }

        public VendorConfigRepository VendorConfigRepository { get; set; }

        protected virtual VendorConfigEntity GetVendorConfig(int vendorConfigID)
        {
            return VendorConfigRepository.GetById(vendorConfigID);
        }

        protected virtual IGameVendorIntegrationService GetVendorService(GameVendorTypeEnum gameVendor)
        {
            switch (gameVendor)
            {
                case GameVendorTypeEnum.AGG:
                    return new AGGIntegrationService();

                case GameVendorTypeEnum.AGGLive:
                    return new AGGLiveIntegrationService();

                default:
                    return ServiceInstanceHelper.InstanceVendorService(gameVendor);
            }
        }

        protected virtual void SendCompletedMessage(string message)
        {
            MyTelegramUtility.SendMessage(new TelegramMessageParameter
            {
                Message = message,
                TelegramChannelEnum = TelegramChannelEnum.pgs_common_error
            });
        }

        /// <summary>
        /// 從遊戲商的抓注單API抓資料 & 寫入DB  (通常是debug需求才會使用)
        /// </summary>
        public void FetchApiAndInsertToTable(WagetFetchJobParameter model)
        {
            model.RuleFor(x => x.BeginDateTW).Rule_NotNullDateTime();
            model.RuleFor(x => x.EndDateTW).Rule_NotNullDateTime();
            model.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();

            DateTime begin = DateTimeHelper.GetCurrentTime();

            var queryVendorConfig = GetVendorConfig(model.VendorConfigID);

            if (queryVendorConfig == null)
                throw new TerproException(ApiErrorCode.NotFoundVendorConfig);

            var gameVendor = (GameVendorTypeEnum)queryVendorConfig.VendorID;

            switch (gameVendor)
            {
                case GameVendorTypeEnum.AGG:
                    FetchWagers_AGG(gameVendor, model);
                    break;

                case GameVendorTypeEnum.AGGLive:
                    FetchWagers_AGGLive(gameVendor, model);
                    break;

                default:
                    FetchWagers_Other(gameVendor, model);
                    break;
            }

            DateTime end = DateTimeHelper.GetCurrentTime();

            string msg = $"FetchApiAndInsertToTable 結束 , 總花費{(end - begin).TotalSeconds}秒";

            SendCompletedMessage(msg);
        }


        private void FetchWagers_AGG(GameVendorTypeEnum gameVendor, WagetFetchJobParameter model)
        {
            var vendorService = GetVendorService(gameVendor);

            int vendorConfigID = model.VendorConfigID;

            //AGG會自動調節前後加減1分鐘
            DateTime beginUTC = model.BeginDateTW.Value.AddHours(-8).AddMinutes(-1);
            DateTime endUTC = model.EndDateTW.Value.AddHours(-8).AddMinutes(1);

            List<string> specificGameCodes = AssignGameCodeByParameter(model);

            //AGG的分頁功能似乎有問題。需要自行切割時間到很短的間隔
            double minutesInterval = 3;

            if(model.SplitMinutes.NotNullNotZeroNotMinus())
            {
                minutesInterval = model.SplitMinutes.Value;
            }

            var splitTimes = ExtensionOfDatetime.SplitToDateListByMinutesRange(beginUTC, endUTC, minutesInterval);

            List<ThirdPartyFetchRecordBriefEntity> insertBriefItems = new List<ThirdPartyFetchRecordBriefEntity>();

            int loopTimes = 0;

            List<string> duplicateWagerIDs = new List<string>();
            HashSet<string> previousWagerIDs = new HashSet<string>();

            //如果是AGG 臨時排除一些單
            var extraExcludeWagers = GenerateExtraExcludeWagers();

            foreach (DateTime time in splitTimes)
            {
                loopTimes++;

                foreach (var gameCode in specificGameCodes)
                {
                    int page = 0;

                    while (page <= 20)
                    {
                        page++;

                        var apiResult = vendorService.FetchWagers(new ExternalVendorFetchWagersRequestParameter
                        {
                            StartTimeUTC = time,
                            EndTimeUTC = time.AddMinutes(minutesInterval),
                            VendorConfigID = vendorConfigID,
                            GameCode = gameCode,
                            Page = page
                        });

                        //2024/05 不確定注單撈出來的結果不正確 是否原因是因為 59:59秒這種寫法
                        if (apiResult.ConvertInsertEntity.IsNotEmptyNull())
                        {
                            //上次查詢跟這這次查詢 可能有重疊的資料
                            if(duplicateWagerIDs.IsNotEmptyNull())
                            {
                                apiResult.ConvertInsertEntity = apiResult.ConvertInsertEntity
                                    .Where(x => !duplicateWagerIDs.Contains(x.WagerID))
                                    .ToList();
                            }

                            duplicateWagerIDs = apiResult.ConvertInsertEntity.Select(x => x.WagerID).ToList();
                        }

                        if (apiResult.PageSize <= 0)
                            throw new TerproException(ApiErrorCode.BusinessLogicDeny, "請於遊戲商service填寫pageSize的值");

                        if (apiResult.ConvertInsertEntity.IsEmptyOrNull())
                            break;

                        insertBriefItems.AddRange(apiResult.ConvertInsertEntity);

                        if (apiResult.ConvertInsertEntity.Count() < apiResult.PageSize)
                        {
                            break;
                        }
                        else
                        {
                            string msg = $"FetchWagers_AGG {time.ToString("yyyy-MM-dd HH:mm:ss")} 筆數過多。可能會fetch資料異常";
                            SendMessageToMultipleChannel(msg);
                        }
                    }
                }

                //每一定次數就寫入資料 (累計幾10萬筆才寫會timeout)
                if (loopTimes % 20 == 19)
                {
                    insertBriefItems = insertBriefItems
                        .Where(x => !previousWagerIDs.Contains(x.WagerID))
                        .ToList();

                    insertBriefItems = insertBriefItems
                        .Where(x => !extraExcludeWagers.Contains(x.WagerID))
                        .ToList();

                    //避免重複單號的問題
                    insertBriefItems = insertBriefItems
                        .GroupBy(x => x.WagerID).Select(x => x.FirstOrDefault())
                        .ToList();

                    InsertToDB(gameVendor, insertBriefItems);

                    previousWagerIDs = new HashSet<string>(insertBriefItems.Select(x => x.WagerID).ToList());
                    insertBriefItems = new List<ThirdPartyFetchRecordBriefEntity>();
                }
            }

            InsertToDB(gameVendor, insertBriefItems);
        }


        private void FetchWagers_AGGLive(GameVendorTypeEnum gameVendor, WagetFetchJobParameter model)
        {
            var vendorService = GetVendorService(gameVendor);

            int vendorConfigID = model.VendorConfigID;
            DateTime beginUTC = model.BeginDateTW.Value.AddHours(-8);
            DateTime endUTC = model.EndDateTW.Value.AddHours(-8);

            List<string> specificGameCodes = AssignGameCodeByParameter(model);

            List<string> notEmptyGameCodes = specificGameCodes
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            if (notEmptyGameCodes.IsEmptyOrNull())
            {
                var queryEnableGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
                {
                    GameVendorTypes = gameVendor.ToInteger().ToListCollection(),
                    Status = NormalActiveStatus.Enable.ToInteger()
                });

                specificGameCodes = queryEnableGames
                    .Where(x=> x.GameID <= 330099) //大廳那些不要算進去
                    .Select(z => z.ThirdPartyGameCode)
                    .ToList();
            }

            //部分遊戲商API 限制間隔最長拉1小時
            double hourInterval = 4;

            var splitTimes = ExtensionOfDatetime.SplitToDateListByHourRange(beginUTC, endUTC, hourInterval);

            List<ThirdPartyFetchRecordBriefEntity> insertBriefItems = new List<ThirdPartyFetchRecordBriefEntity>();

            foreach (DateTime time in splitTimes)
            {
                foreach (var gameCode in specificGameCodes)
                {
                    int page = 0;

                    while (page <= 20)
                    {
                        page++;

                        var apiResult = vendorService.FetchWagers(new ExternalVendorFetchWagersRequestParameter
                        {
                            StartTimeUTC = time,
                            EndTimeUTC = time.AddHours(hourInterval).AddMilliseconds(-3),
                            VendorConfigID = vendorConfigID,
                            GameCode = gameCode,
                            Page = page
                        });

                        if (apiResult.PageSize <= 0)
                            throw new TerproException(ApiErrorCode.BusinessLogicDeny, "請於遊戲商service填寫pageSize的值");

                        if (apiResult.ConvertInsertEntity.IsEmptyOrNull())
                            break;

                        insertBriefItems.AddRange(apiResult.ConvertInsertEntity);

                        if (apiResult.ConvertInsertEntity.Count() < apiResult.PageSize)
                        {
                            break;
                        }
                    }
                }
            }

            InsertToDB(gameVendor, insertBriefItems);
        }



        private void FetchWagers_Other(GameVendorTypeEnum gameVendor, WagetFetchJobParameter model)
        {
            var vendorService = GetVendorService(gameVendor);
                
            int vendorConfigID = model.VendorConfigID;
            DateTime beginUTC = model.BeginDateTW.Value.AddHours(-8);
            DateTime endUTC = model.EndDateTW.Value.AddHours(-8);
            List<string> specificGameCodes = AssignGameCodeByParameter(model);

            //部分遊戲商API 限制間隔最長拉1小時、或更短
            double minutesInterval = 60 * 6;

            if(gameVendor == GameVendorTypeEnum.Jili)
            {
                minutesInterval = 10;
            }

            if (model.SplitMinutes.NotNullNotZeroNotMinus())
            {
                minutesInterval = model.SplitMinutes.Value;
            }

            var splitTimes = ExtensionOfDatetime.SplitToDateListByMinutesRange(beginUTC, endUTC, minutesInterval);

            List<ThirdPartyFetchRecordBriefEntity> insertBriefItems = new List<ThirdPartyFetchRecordBriefEntity>();

            foreach (DateTime time in splitTimes)
            {
                foreach (var gameCode in specificGameCodes)
                {
                    int page = 0;

                    while (page <= 20)
                    {
                        page++;

                        var apiResult = vendorService.FetchWagers(new ExternalVendorFetchWagersRequestParameter
                        {
                            StartTimeUTC = time,
                            EndTimeUTC = time.AddMinutes(minutesInterval).AddMilliseconds(-3),
                            VendorConfigID = vendorConfigID,
                            GameCode = gameCode,
                            Page = page
                        });

                        if (apiResult.PageSize <= 0)
                            throw new TerproException(ApiErrorCode.BusinessLogicDeny, "請於遊戲商service填寫pageSize的值");

                        if (apiResult.ConvertInsertEntity.IsEmptyOrNull())
                            break;

                        insertBriefItems.AddRange(apiResult.ConvertInsertEntity);

                        if (apiResult.ConvertInsertEntity.Count() < apiResult.PageSize)
                        {
                            break;
                        }
                    }
                }
            }

            InsertToDB(gameVendor, insertBriefItems);
        }



        /// <summary>
        /// 將簡易型model寫入DB的 (並補上部分欄位資訊)
        /// </summary>
        /// <param name="vendor"></param>
        /// <param name="insertItems"></param>
        protected virtual void InsertToDB(GameVendorTypeEnum vendor, List<ThirdPartyFetchRecordBriefEntity> insertItems)
        {
            int vendorID = vendor.ToInteger();

            var gameList = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                GameVendorTypes = vendor.ToInteger().ToListCollection()
            });

            var insertEntity = insertItems.Select(item =>
                {
                    var entity = item.ReturnInjecteObject<ThirdPartyFetchRecordEntity>();
                    entity.GameID = gameList.FirstOrDefault(x => x.ThirdPartyGameCode == item.GameCode)?.GameID;
                    entity.GameVendor = vendorID;
                    entity.BetAmount = entity.BetAmount.ToAbs();
                    entity.StatDate = item.SettleTime.Value.AddHours(8).Date;
                    return entity;
                })
                .ToList();

            ThirdPartyFetchRecordRepository.BulkInsert(insertEntity);
        }

        private List<string> AssignGameCodeByParameter(WagetFetchJobParameter model)
        {
            List<string> specificGameCodes = new List<string>
            {
                "" //若不篩選遊戲、請填寫空字串 (array至少要有一筆)
            };

            if (model.GameCode.IsNotEmptyTextNotNull())
            {
                specificGameCodes = new List<string>
                {
                    model.GameCode
                };
            }

            return specificGameCodes;
        }

        private void CheckVendorConfigIsValid(GameVendorTypeEnum gameVendor, int vendorConfigID)
        {
            var queryVendorConfig = VendorConfigRepository.GetById(vendorConfigID);

            if (queryVendorConfig == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            if (queryVendorConfig.VendorID != gameVendor.ToInteger())
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, "gameVendor與VendorConfig不匹配");
        }


        private HashSet<string> GenerateExtraExcludeWagers()
        {
            string myText = @"";

            List<string> listOfIDs = AppHelper.AnalyzeTextFromRawTextBySpaceChar(myText);

            return new HashSet<string>(listOfIDs);
        }


    }
}
