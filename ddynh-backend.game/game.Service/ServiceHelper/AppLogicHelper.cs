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
using game.Infrastructure.Extensions;
using game.Entity.Event;

namespace game.Infrastructure.Helper
{
    public class AppLogicHelper
    {
       
        /// <summary>
        /// 根據GameID判斷是否為三方遊戲
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        public static bool IsThirdPartyGame(int gameID)
        {
            return gameID >= (int)GameListEnum.APL_Lottery;
        }


        public static string ShowCashAmountOrDashText(int? gameID , long amount)
        {
            if (!gameID.HasValue)
                return amount.To_Cash();

            bool isThird = IsThirdPartyGame(gameID.Value);

            if (isThird && amount == 0)
                return "-";

            return amount.To_Cash();
        }

        public static List<int> GetRouletteEventTypes()
        {
            return new List<int>
            {
                ApplyEventTypeEnum.Roulette_Recharge.ToInteger(),
                ApplyEventTypeEnum.Roulette_FlowAmount.ToInteger(),
            };
        }


    }
}
