using System;
using FW.Core.Helper;
using FW.Core.Helper.Enum;

namespace FW.Core.Engine
{
    public enum LifeStyle
    {
        [EnumDescription("全局唯一实例")]
        Singleton,
        [EnumDescription("每次调用创建新实例")]
        Transient,
        [EnumDescription("生命周期内唯一实例")]
        LifetimeScope
    }
}
