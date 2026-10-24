using System.ComponentModel;

namespace target.Models.Enums
{
    public enum OneTimesJobStatusEnum
    {
        /// <summary>
        /// 等待執行
        /// </summary>
        Pending = 0,

        Success = 1,

        /// <summary>
        /// 執行中
        /// </summary>
        OnExecuted = 5,

        /// <summary>
        /// 需要Retry
        /// </summary>
        MustRetry = 7,

        Fail = 999

    }

}
