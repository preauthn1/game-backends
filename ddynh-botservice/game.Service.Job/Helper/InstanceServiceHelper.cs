using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Game;
using game.Service.DAO;
using game.Infrastructure;
using game.Entity.ViewModels;
using game.Infrastructure.Extensions;
using game.Infrastructure.Helper;
using game.Entity;
using example.invalid;
using game.Service.Job.ServiceImplement.Services.ReferenceService.Services;

namespace game.Service.Service
{
    public class InstanceJobServiceHelper
    {

        public static IThirdPartyGameCommonDatabaseProcessService InstanceByMerchantType(
            GameMerchantEnum gameMerchant , 
            GameListEnum? gameID = null)
        {
            switch (gameMerchant)
            {
                case GameMerchantEnum.EGaming:
                    return new EGamingCommonDatabaseProcessService();

                case GameMerchantEnum.APL:

                    if(gameID == GameListEnum.APL_Fish)
                        return new APLFishCommonDatabaseProcessService();
                    else
                        return new APLCommonDatabaseProcessService();

                case GameMerchantEnum.BG:
                    return new BGCommonDatabaseProcessService();

                default:
                    throw new TerproException(ApiErrorCode.NotImplement);
            }
        }


    }

}
