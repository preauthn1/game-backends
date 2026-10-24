using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using target.Models.Extensions;

namespace target.Models.CoreLibs
{

    /// <summary>
    /// 影響SQLHelper，用來標記此table非自動增長auto Identity
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class IsNotAutoKeyTable : System.Attribute
    {

    }

    /// <summary>
    /// 標記此的欄位 不會產生update語法
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public class DoNotUpdate : System.Attribute
    {

    }


    /// <summary>
    /// 標記此的欄位 不會自動new物件 (需手動)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public class DoNotAutoInject : System.Attribute
    {

    }



    /// <summary>
    /// 已廢棄的欄位 (insert或 update 都忽略他)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public class IgnoreWhenInsertUpdate : System.Attribute
    {

    }

}
