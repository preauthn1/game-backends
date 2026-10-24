using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Service
{    
    public enum UserLevel : byte
    {
        [Description("")]
        Level0 = 0,
        [Description("实习生")]
        Level1 = 1,
        [Description("初级归广员")]
        Level2 = 2,
        [Description("高级推广员")]
        Level3 = 3,
        [Description("资深归广员")]
        Level4 = 4,
        [Description("推广总监")]
        Level5 = 5,
        [Description("合伙人")]
        Level6 = 6
    }

    public enum Status : byte
    {
        [Description("停权")]
        State1 = 0,
        [Description("启用")]
        State2 = 1,
        [Description("其他")]
        State3 = 2,
     
    }    

    public static class EnumExtenstions
    {
        public static string ToDescription(this Enum value)
        {
            return value.GetType()
                .GetRuntimeField(value.ToString())
                .GetCustomAttributes<System.ComponentModel.DescriptionAttribute>()
                .FirstOrDefault()?.Description ?? string.Empty;
        }
    }
}
