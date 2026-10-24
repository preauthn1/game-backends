using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.CultureText;


namespace target.Models.Enums
{

    public enum EventMultiplePrizeTypeEnum
    {

        /// <summary>
        /// 以BetAmount當基準 * 倍率  派發彩金
        /// </summary>
        SendBalanceByBetAmountRate = 21,

        /// <summary>
        /// 根據獎項派發現金
        /// </summary>
        SendBalanceWithCash = 25,


        /// <summary>
        /// 根據排名順序依次派獎
        /// </summary>
        SendBalanceByRank = 51,

    }


}
