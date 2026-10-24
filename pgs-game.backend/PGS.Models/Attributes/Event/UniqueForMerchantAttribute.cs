using target.Models.Enums;

namespace target.Models.Attributes
{
    
    /// <summary>
    /// 每個商戶只能建一個此類活動
    /// </summary>
    public class UniqueForMerchantAttribute : System.Attribute
    {
       
    }

    /// <summary>
    /// 每個商戶在遊戲不重複的情況可以建多個活動
    /// </summary>
    public class UniqueForMerchantAndGameAttribute : System.Attribute
    {

    }


}
