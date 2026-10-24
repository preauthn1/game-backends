using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.Game;
using game.Entity.ViewModels;
using game.Entity.ViewModels.Query;

namespace game.Service
{

    /// <summary>
    /// 各個三方遊戲。通用型的資料處理(背後的table可能不一樣)
    /// </summary>
    public interface IThirdPartyGameCommonDatabaseProcessService
    {
       
        void TransformDataToGameLog();


        void UpdateCalculateQueueStatusToDone(string jobIdentity, List<string> betRecords);


        List<QueryNotCalculateFlowAmountModel> GetNotCalculateBetRecords();


        int GetGameID(int gameMerchantID, string gameName = "");
    }
}
