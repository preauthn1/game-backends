namespace target.Models.Attributes
{
    /// <summary>
    /// 不允許後台編輯
    /// </summary>
    public class DisableEdit : System.Attribute
    {

    }

    /// <summary>
    /// 如果是透過UI編輯 自動跳過
    /// </summary>
    public class SkipManualEdit : System.Attribute
    {

    }

    /// <summary>
    /// 僅測試環境允許編輯
    /// </summary>
    public class DisableEditWhenProduction : System.Attribute
    {

    }

}
