using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.CultureText;


namespace target.Models.Enums
{
    /// <summary>
    /// 前台廣告專用
    /// </summary>
    public enum LinkTypeEnum
    {
        None = 0,

        ToGamePage = 21,

        ToInternalLink = 98,

        ToExternalLink = 99,

        LobbyPage_Quest = 2001,

        LobbyPage_LuckyWheel = 2005,

        /// <summary>
        /// 紅包說明頁 (有設置文案才會有)
        /// </summary>
        LobbyPage_RedEnvelopeDescription = 2009,
    }

}
