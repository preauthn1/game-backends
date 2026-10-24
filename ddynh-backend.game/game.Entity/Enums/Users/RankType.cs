
using System;
using FW.Core.Helper.Enum;

namespace game.Entity.Users
{
  

    public enum RankType : byte
    {
        /// <summary>
        /// 日賽排行
        /// </summary>
        [EnumDescription("日赛排行")]
        DayEvent = 1,
        /// <summary>
        /// 累积赛排行
        /// </summary>
        [EnumDescription("累积赛排行")]
        Cumulative = 2,
    }


}
