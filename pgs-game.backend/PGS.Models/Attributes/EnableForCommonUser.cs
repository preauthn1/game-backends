using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using target.Models.Extensions;

namespace target.Utility.Helper
{
    /// <summary>
    /// 是否開放一般後台帳號使用特定功能
    /// </summary>
    public class EnableForCommonUser : System.Attribute
    {

    }

}
