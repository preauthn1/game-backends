using System;
using System.Collections.Generic;
using System.Text;

namespace target.Models.Models.StoreProcedures
{
    public class Stat_MerchantDailySummaryParameterModel
    {
        /// <summary>
        /// 傳過來的是日期 (指定時區的) e.g. 2022-01-01 SP那邊會視為XX時區。再將日期轉成UTC+0
        /// </summary>
        public DateTime DateStart { get; set; }

        /// <summary>
        /// 傳過來的是日期 (指定時區的) e.g. 2022-01-01 SP那邊會視為XX時區。再將日期轉成UTC+0
        /// </summary>
        public DateTime DateEnd { get; set; }

        public double Timezone { get; set; }

        public string MerchantList { get; set; }


    }



    public class Stat_MerchantDailySummaryParameterModelV2
    {
        /// <summary>
        /// 傳過來的是日期 (指定時區的) e.g. 2022-01-01 SP那邊會視為XX時區。再將日期轉成UTC+0
        /// </summary>
        public DateTime DateStart { get; set; }

        /// <summary>
        /// 傳過來的是日期 (指定時區的) e.g. 2022-01-01 SP那邊會視為XX時區。再將日期轉成UTC+0
        /// </summary>
        public DateTime DateEnd { get; set; }

        public double Timezone { get; set; }


        public List<int> MerchantIDs { get; set; }

        public List<int> GameIDs { get; set; }


    }




}
