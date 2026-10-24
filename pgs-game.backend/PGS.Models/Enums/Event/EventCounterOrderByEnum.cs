using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.Attributes;
using target.Models.Attributes.ColumnValue;
using target.Models.CultureText;


namespace target.Models.Enums
{

    public enum EventCounterOrderByEnum
    {
        //*註 這邊的 TotalBetAmount、TotalPayoutAmount 這些需要更DB欄位名稱一致。是SQL order by查詢用的
        //*註 這邊的 TotalBetAmount、TotalPayoutAmount 這些需要更DB欄位名稱一致。是SQL order by查詢用的
        //*註 這邊的 TotalBetAmount、TotalPayoutAmount 這些需要更DB欄位名稱一致。是SQL order by查詢用的

        [ExtraAttributeValue("TotalBetAmount")]
        TotalBetAmount = 0,

        [ExtraAttributeValue("TotalPayoutAmount")]
        TotalPayoutAmount = 11,

        [ExtraAttributeValue("MaxPayoutRate")]
        MaxPayoutRate = 77,
    }
}
