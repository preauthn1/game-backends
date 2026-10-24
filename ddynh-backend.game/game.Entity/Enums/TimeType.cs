using FW.Core.Helper.Enum;

namespace game.Entity.Time
{
    /// <summary>
    /// 時間類型
    /// </summary>
    public enum TimeType : byte
    {
        [EnumDescription("创建时间")]
        CreateTime = 1,
        [EnumDescription("更新时间")]
        LastUpdateTime = 2,
        [EnumDescription("封锁时间")]
        ForbidTime = 3
    }
}
