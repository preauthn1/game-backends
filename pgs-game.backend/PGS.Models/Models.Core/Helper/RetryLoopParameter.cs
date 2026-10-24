using System.Collections.Generic;

namespace target.Utility.Helper
{
    public class RetryLoopParameter
    {

        public RetryLoopParameter()
        {
            this.ExceptionNotifyLevel = LoopExceptionNotifyLevelEnum.AlwaysWhenException;
        }


        /// <summary>
        /// 指定2 = 迴圈會跑兩次
        /// </summary>
        public int MaxTimes { get; set; }


        public bool IsAsync { get; set; }


        public int DelayMilliseconds { get; set; }

        /// <summary>
        /// 迴圈最後一次 還是發生exception的話
        /// </summary>
        public LoopExceptionNotifyLevelEnum ExceptionNotifyLevel { get; set; }

        /// <summary>
        /// 錯誤通知的tag
        /// </summary>
        public string Tag { get; set; }


        public RetryLoopExtraSetting ExtraSetting { get; set; }
    }



    public class RetryLoopExtraSetting
    {
        public List<string> IgnoreNotifyKeyword { get; set; }


        public string LogExtraInfo { get; set; }
    }






    public class RetryLoopMethodParameter
    {
        /// <summary>
        /// 記錄第幾次執行
        /// </summary>
        public int LoopTimes { get; set; }
    }


    public enum LoopExceptionNotifyLevelEnum
    {
        /// <summary>
        /// 每次迴圈發生exception都會通知
        /// </summary>
        AlwaysWhenException = 0,

        /// <summary>
        /// 迴圈最後一次發生exception才通知
        /// </summary>
        OnlyWhenLastTimeExceptioin = 5,

        /// <summary>
        /// 不要發通知
        /// </summary>
        Never = 999,
    }


}
