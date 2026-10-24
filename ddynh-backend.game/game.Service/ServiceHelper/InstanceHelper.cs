using FW.Core.Engine;
using game.Entity;
using game.Entity.Area;
using game.Entity.Game;
using game.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Infrastructure.Helper
{
    public class InstanceHelper
    {
       
        public static IBaseThirdPartyGameMerchantApiService InstanceServiceByGameMerchant(GameMerchantEnum gameMerchant)
        {
            switch(gameMerchant)
            {
                case GameMerchantEnum.APL:
                    return IOCHelper.Resolve<IAPLGameMerchantApiService>();

                case GameMerchantEnum.EGaming:
                    return IOCHelper.Resolve<IEGamingGameMerchantApiService>();

                case GameMerchantEnum.BG:
                    return IOCHelper.Resolve<IBGGameMerchantApiService>();

                default:
                    throw new TerproException(ApiErrorCode.NotImplement);
            }
        }

    }
}
