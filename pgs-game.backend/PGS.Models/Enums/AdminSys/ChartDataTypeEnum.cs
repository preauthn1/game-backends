using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.CultureText;


namespace target.Models.Enums
{

    /// <summary>
    /// Chart Data 用的
    /// </summary>
    public enum ChartDataTypeEnum
    {
       
        Summary_TotalLogin = 1,
     
        Summary_AverageOnline = 2,

        Summary_TotalGame = 3,
     
        Game_TotalGame = 101,
      
        Game_OnlineUser = 102,

        MerchantGameSummary_TotalRound_Per30Minutes = 5101,

        MerchantGameSummary_TotalBetAmount_Per30Minutes = 5103,

        HomePage_EachCurrencyDailySummary = 6770,

        HomePage_EachCurrencyDailyTotalRound = 6778,
    }
}
