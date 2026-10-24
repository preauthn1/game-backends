using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.Game;
using game.Entity.ViewModels.Query;

namespace game.Service
{

    public interface IDataMigrateService
    {
        void CalculateFlowAmountProcess(int gameMerchantID , int gameID);


        void TransformDataToGameLog(int gameMerchantID, int gameID);
    }
}
