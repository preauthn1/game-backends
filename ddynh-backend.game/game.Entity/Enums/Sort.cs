using FW.Core.Helper.Enum;

namespace game.Entity.AdminSys
{
   public enum SortName
    {
        [EnumDescription("充值金额")]
        ChargeAmount = 0,
        [EnumDescription("兌換金额")]
        ConvertAmount = 1,
        [EnumDescription("統計時間")]
        StatTime = 2,
        [EnumDescription("註冊用戶人数")]
        TotalRegCount = 3,
        [EnumDescription("活跃人数")]
        DAU = 4,
        [EnumDescription("注册设备数")]
        TotalLoginMac = 5,
        [EnumDescription("注册IP数")]
        TotalLoginIP = 6,
        [EnumDescription("绑定手机数")]
        TotalBindUser = 7,
        [EnumDescription("所属渠道")]
        Area = 99,
    }

    public enum SortLifting
    {
        [EnumDescription("倒敘")]
        DESC = 0,
        [EnumDescription("正敘")]
        ASC = 1
    }
}
