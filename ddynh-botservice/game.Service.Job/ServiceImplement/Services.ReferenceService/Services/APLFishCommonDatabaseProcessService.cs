using game.Entity.ViewModels;
using game.Service.DAO;
using game.Service.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game.Infrastructure.Extensions;
using game.Entity.Game;
using game.Entity;

namespace game.Service.Job.ServiceImplement.Services.ReferenceService.Services
{
    public class APLFishCommonDatabaseProcessService : BaseService , IThirdPartyGameCommonDatabaseProcessService
    {

        public APLFishBetRecordRepository BetRecordRepository { get; set; }


        public APLFishDataMigratePendingQueueRepository DataMigratePendingQueueRepository { get; set; }



        public List<QueryNotCalculateFlowAmountModel> GetNotCalculateBetRecords()
        {
            var queryBetRecords = BetRecordRepository.GetNotCalculateFlowAmountRecords();
            
            foreach(var item in queryBetRecords)
            {
                //記得要轉絕對值
                item.ValidBetAmount = item.ValidBetAmount.ToAbsoluteValue();
                item.PayoutAmount = item.WinLossAmount + item.BetAmount;
            }

            return queryBetRecords;
        }

        public void UpdateCalculateQueueStatusToDone(string jobIdentity, List<string> betRecords)
        {
            DataMigratePendingQueueRepository.UpdateIsDone(jobIdentity, betRecords);
        }


        public int GetGameID(int gameMerchantID, string gameName = "")
        {
            return GameListEnum.APL_Fish.ToInteger();
        }


        public void TransformDataToGameLog()
        {
            BetRecordRepository.TransformDataToGameLog();
        }

    }
}
