using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.CultureText;


namespace target.Models.Attributes.ColumnValue
{      
    /// <summary>
    /// 不需要填數值 (本身已經是個true行為的標記了)
    /// </summary>
    public class ValueUnnecessary : System.Attribute
    {

    }


    /// <summary>
    /// 是否需要選時間區間
    /// </summary>
    public class DateTimeRangeValue : System.Attribute
    {

    }


    /// <summary>
    /// 如果標記此，且數量大於1，會迴圈派發prizeRecord
    /// </summary>
    public class LoopSendPrize : System.Attribute
    {

    }



    /// <summary>
    /// 是否允許小數點
    /// </summary>
    public class AllowFloat : System.Attribute
    {

    }

    /// <summary>
    /// 是否允許負數
    /// </summary>
    public class AllowMinus : System.Attribute
    {

    }


    /// <summary>
    /// 允許填0
    /// </summary>
    public class AllowZero : System.Attribute
    {

    }



    public class ValidateValueRange : System.Attribute
    {

        public ValidateValueRange(double min , double max)
        {
            this.Min = min;
            this.Max = max;
        }


        public double Min { get; set; }

        /// <summary>
        /// 不含數值本身
        /// </summary>
        public double Max { get; set; }
    }



    /// <summary>
    /// 在EventProcess的流程中 不insert獎品紀錄 而是透過自己的邏輯處理
    /// </summary>
    public class SkipInsertPrizeRecordWhenProcess : System.Attribute
    {

    }


}
