using game.Entity.Game;
using game.Entity.ViewModels;
using game.Service.DAO;
using game.Service.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game.Infrastructure.Extensions;

namespace game.Service.Job.ServiceImplement.Services.ReferenceService.Services
{
    public class EGamingCommonDatabaseProcessService :　BaseService , IThirdPartyGameCommonDatabaseProcessService
    {
        public EGamingBetRecordRepository BetRecordRepository { get; set; }

        public EGamingDataMigratePendingQueueRepository DataMigratePendingQueueRepository { get; set; }


        public List<QueryNotCalculateFlowAmountModel> GetNotCalculateBetRecords()
        {
            var queryBetRecords = BetRecordRepository.GetNotCalculateFlowAmountRecords();

            foreach (var item in queryBetRecords)
            {
                item.GameID = GameListEnum.EGaming_BJL.ToInteger();
            }

            return queryBetRecords;
        }


        public void UpdateCalculateQueueStatusToDone(string jobIdentity, List<string> betRecords)
        {
            DataMigratePendingQueueRepository.UpdateIsDone(jobIdentity , betRecords);
        }


        public int GetGameID(int gameMerchantID, string gameName = "")
        {
            return GameListEnum.EGaming_BJL.ToInteger();
        }

        public void TransformDataToGameLog()
        {
            BetRecordRepository.TransformDataToGameLog();
        }

    }
}
