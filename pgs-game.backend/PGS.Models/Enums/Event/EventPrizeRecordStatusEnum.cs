using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.CultureText;


namespace target.Models.Enums
{

    public enum EventPrizeRecordStatusEnum
    {

        AllocateSuccess = 1,


        /// <summary>
        /// 已使用 (etc. 抽獎券)
        /// </summary>
        Usage = 9,


        /// <summary>
        /// 已派發未領取
        /// </summary>
        AllocateSuccessNotReceive = 11,


        /// <summary>
        /// 已派發 且領取成功
        /// </summary>
        AllocateSuccessReceiveSuccess = 13,


    }
}
