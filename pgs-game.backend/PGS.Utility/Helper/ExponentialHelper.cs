using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using target.Utility.Extensions;
using example.invalid;
using example.invalid;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.Runtime.Versioning;
using target.Models.Models;
using target.Models.CultureText;
using target.Models.Enums;
using DocumentFormat.OpenXml.Presentation;

namespace target.Utility.Helper
{

    /// <summary>
    /// 指數設計相關
    /// </summary>
    public static class ExponentialHelper
    {

        /// <summary>
        /// 根據遊玩局數 指數遞減決定合理的RTP
        /// 當 x小於 5。y= 150。
        /// 當 x=10。y= 120。
        /// 當 x=20 y= 100   
        /// 當 x=25 y= 98。
        /// 當 x > 25 y 永遠趨近於 98
        /// </summary>
        /// <returns></returns>
        public static decimal DetermineCasinoGame_SuspiciousRTP_ByRound(int x)
        {
            if (x <= 3)
            {
                return 200;
            }
            else
            {
                double A = 61.75;
                double k = 0.172;
                double C = 98;
                return (decimal)(A * Math.Exp(-k * (x - 4)) + C);
            }
        }


    }



}