using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Service.DAO;
using game.Infrastructure;
using game.Entity.ViewModels.Query;
using game.Entity.Game;
using game.Entity;
using game.Infrastructure.Helper;
using game.Entity.Money;
using game.Infrastructure.Extensions;
using game.Entity.ViewModels;
using game.Entity.RawEntity;
using game.Infrastructure.Utility.ThirdParty;
using System.Threading.Tasks;
using game.Service.Job.ServiceImplement.Services.ReferenceService.Services;

namespace game.Service.Service
{
    public class DataMigrateCalculateService : BaseService, IDataMigrateService
    {

        public FlowAmountLimitLogRepository FlowAmountLimitLogRepository { get; set; }

        public UserInfoRepository UserInfoRepository { get; set; }

        public ThirdPartyBetFlowAmountCalculateRecordRepository ThirdPartyBetFlowAmountCalculateRecordRepository { get; set; }

        public JobExecuteRecordRepository JobExecuteRecordRepository { get; set; }

        public UserGameInfoRepository UserGameInfoRepository { get; set; }

        private IThirdPartyGameCommonDatabaseProcessService ThirdPartyGameCommonDatabaseProcessService { get; set; }


        public Guid _rndID = Guid.NewGuid();



        public void TransformDataToGameLog(int gameMerchantID , int gameID)
        {
            GameMerchantEnum gameMerchant = (GameMerchantEnum)gameMerchantID;

            //根據遊戲廠商不同 實例化不同的service
            InstanceByMerchantType(gameMerchant, (GameListEnum)gameID);

            ThirdPartyGameCommonDatabaseProcessService.TransformDataToGameLog();
        }



        public void CalculateFlowAmountProcess(int gameMerchantID, int gameID)
        {
            int jobType = GameMerchantJobTypeEnum.DataTransform_CalculateFlowAmount.ToInteger();

            GameMerchantEnum gameMerchant = (GameMerchantEnum)gameMerchantID;

            //根據遊戲廠商不同 實例化不同的service
            InstanceByMerchantType(gameMerchant, (GameListEnum)gameID);

            //根據不同遊戲商 要查的DB不同
            List<QueryNotCalculateFlowAmountModel> queryBetRecords = GetNotCalculateBetRecordsByGameMerchant();

            string jobIdentity = GetJobIdentity();

            //若無資料結束排程
            if (queryBetRecords.IsEmptyOrNull())
                return;
            
            //將這些資料更新為已處理
            List<string> betRecordNos = queryBetRecords.Select(x => x.BetRecordNo).ToList();
            ThirdPartyGameCommonDatabaseProcessService.UpdateCalculateQueueStatusToDone(jobIdentity, betRecordNos);

            //每個玩家的資料 groupBy起來統一處理
            var groupByUser = queryBetRecords.GroupBy(x => $"{x.UserID}_{x.GameID}").ToDictionary(x => x.FirstOrDefault().UserID, z => new 
            { 
                Items = z.ToList() ,
                UserID = z.FirstOrDefault().UserID,
                GameID = z.FirstOrDefault().GameID
            });

            foreach (var each in groupByUser)
            {
                int userID = each.Value.UserID;

                List<QueryNotCalculateFlowAmountModel> myRecords = each.Value.Items;


                Task.Factory.StartNew(() =>
                {
                    //更新稽核流水、總流水 (非同步處理)
                    UpdateFlowAmountAndWriteRecord(gameMerchantID ,each.Value.GameID , userID, myRecords , jobIdentity);

                });

            }

            var debugInfo = new
            {
                GameMerchantID = gameMerchantID,
                Users = groupByUser.Select(x => new
                {
                    UserID = x.Key,
                    BetRecordCount = x.Value.Items.Count(),
                    ValidBetAmount = x.Value.Items.Select(z => z.ValidBetAmount).Sum()
                })
            };

            //寫入一筆排程紀錄
            JobExecuteRecordRepository.Create(new JobExecuteRecordRawEntity
            {
                JobType = jobType,
                TotalCount = groupByUser.Count(),
                CreateTime = DateTimeHelper.GetCurrentTime(),
                Memo = gameMerchantID.ToString(),
                DebugInfo = debugInfo.ConvertToJsonString(),
                JobIdentity = GetJobIdentity()
            });

        }



        private void UpdateFlowAmountAndWriteRecord(int gameMerchantID, int gameID , int userID, List<QueryNotCalculateFlowAmountModel> myRecords , string jobIdentity)
        {
            //目前的機制不考慮改單的處理 (稽核流水那邊table設計 應該也無法處理)
            try
            {
                var now = DateTimeHelper.GetCurrentTime();

                long myTotalFlowAmount = myRecords.Sum(x => x.ValidBetAmount);

                if (myTotalFlowAmount > 0)
                {
                    //step 1. 更新稽核流水
                    FlowAmountLimitLogRepository.UpdateFlowAmountLimit(userID, myTotalFlowAmount);

                    //step 2. 更新sumStack
                    UserInfoRepository.UpdateSumFlowAmount(userID, myTotalFlowAmount);
                }

                //debug需要
                var queryBefore = UserGameInfoRepository.GetByFilter(new QueryUserGameInfoRequestModel
                {
                    UserIDs = userID.ToListCollection(),
                    GameIDs = gameID.ToListCollection()
                });

                //step 3. 寫入用戶遊戲統計
                var gameInfoModel = InstanceUserGameInfo(userID, gameID, myRecords);
                UserGameInfoRepository.CreateOrPlusAmount(gameInfoModel);


                var insertData = myRecords.Select(x => new ThirdPartyBetFlowAmountCalculateRecordRawEntity
                {
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    GameMerchantID = gameMerchantID,
                    GameID = gameID,
                    ReferenceBetRecordID = x.BetRecordID,
                    JobIdentity = jobIdentity
                })
                .ToList();

                //step 4. 紀錄已經處理過的單號
                ThirdPartyBetFlowAmountCalculateRecordRepository.CreateMany(insertData);


                //因為一直資料不正確 需要加強log資訊 作為debug使用
                string logText = $"{jobIdentity} userID: {userID} before:{queryBefore.ConvertToJsonString()} updateThisTimes: {gameInfoModel.ConvertToJsonString()}";
                WriteFile(logText, gameMerchantID);

            }
            catch(Exception ex)
            {
                string msg = $"UpdateFlowAmountAndWriteRecord 錯誤，{ex.ToString()}";

                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                {
                    Message = msg,
                    Channel = SlackChannelEnum.n_dd_job_third_party,
                });
            }
        }


        private void WriteFile(string text , int gameMerchantID)
        {
            string dateText = DateTimeHelper.GetCurrentTime().ToString("yyyyMMdd");
            string fileName = $"{dateText}_{gameMerchantID}_log.txt";

            try
            {
                Common.Log(text, fileName);
            }
            catch
            {

            }

        }


        private UserGameInfoEntity InstanceUserGameInfo(int userID , int gameID , List<QueryNotCalculateFlowAmountModel> myRecords)
        {
            return new UserGameInfoEntity
            {
                UserID = userID,
                DrawCount = myRecords.Count(x=> x.WinLossAmount == 0),
                WinCount = myRecords.Count(x => x.WinLossAmount > 0),
                LoseCount = myRecords.Count(x => x.WinLossAmount < 0),
                Score = myRecords.Select(x=> x.WinLossAmount).Sum(),
                GameID = gameID,
                GameTime = 0,
                LastWinLose = 0,
                BetAmount = myRecords.Select(x => x.BetAmount).Sum(),
                PayAmount = myRecords.Select(x => x.PayoutAmount).Sum(),
            };
        }



        private string GetJobIdentity()
        {
            string myValue = HeaderValueHelper.GetHeaderValueByKey(SystemConst.AdminSys_AlwaysHeaderMemo);

            if (string.IsNullOrWhiteSpace(myValue))
                return _rndID.ToString().Replace("-" , "").Truncate(15);

            return myValue.Truncate(25);
        }

        

        private List<QueryNotCalculateFlowAmountModel> GetNotCalculateBetRecordsByGameMerchant()
        {
            List<QueryNotCalculateFlowAmountModel> queryBetRecords = ThirdPartyGameCommonDatabaseProcessService.GetNotCalculateBetRecords();

            foreach(var item in queryBetRecords)
            {
                if(item.ValidBetAmount < 0)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny, "不允許 ValidBetAmount 為負數");
                }
            }

            return queryBetRecords;
        }



        private void InstanceByMerchantType(GameMerchantEnum gameMerchant, GameListEnum gameID)
        {
            ThirdPartyGameCommonDatabaseProcessService = InstanceJobServiceHelper.InstanceByMerchantType(gameMerchant , gameID);
        }


    }

}
