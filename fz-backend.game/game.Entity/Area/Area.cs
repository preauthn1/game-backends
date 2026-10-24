using FW.Core.Helper;
using FW.Core.Repository;

namespace game.Entity.Area
{
    [Database(Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class Area : BaseEntity
    {
        /// <summary>
        /// 渠道名称
        /// </summary>
        public string AreaName { get; set; }

        /// <summary>
        /// 客户端平台类型(Android,IOS)
        /// </summary>
        public string ClientPlatform { get; set; }

        public bool AreaStatus { get; set; }

        /// <summary>
        /// 渠道说明
        /// </summary>
        public string Memo { get; set; }

        /// <summary>
        /// 渠道所属管理员ID
        /// </summary>
        public int AdminID { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// QRCode分享位置
        /// </summary>
        public string ShareURL { get; set; }

        /// <summary>
        /// 平台ID
        /// </summary>
        public int PlatformID { get; set; }
        public virtual Platform Platform { get; set; }

        /// <summary>
        /// 顯示兌換的充值金額
        /// </summary>
        [MoneyInt]
        public long ShowConvertLimit { get; set; }

        /// <summary>
        /// 顯示其他遊戲的充值金額
        /// </summary>
        [MoneyInt]
        public long ShowAllGameLimit { get; set; }

        /// <summary>
        /// 顯示VIP金額
        /// </summary>
        [MoneyInt]
        public long ShowVIPAgent { get; set; }


        /// <summary>
        /// 顯示客服金額
        /// </summary>
        [MoneyInt]
        public long ShowCS { get; set; }


        /// <summary>
        /// 遊玩多少分鐘顯示客服
        /// </summary>
        public int PlaytimeCS { get; set; }
        /// <summary>
        /// 遊玩多少分鐘顯示兌換
        /// </summary>
        public int PlaytimeConvert { get; set; }
        /// <summary>
        /// 遊玩多少分鐘顯示VIP代理充值
        /// </summary>
        public int PlaytimeVIPAgent { get; set; }


        /// <summary>
        /// 常见问题解答
        /// </summary>
        public string FAQContent { get; set; }

        /// <summary>
        /// 充值提示
        /// </summary>
        public string PayTips { get; set; }

        /// <summary>
        /// 升級提示
        /// </summary>
        public string UpGradeMsg { get; set; }

        /// <summary>
        /// 公告訊息
        /// </summary>
        public string AnnMsg { get; set; }

        /// <summary>
        /// 公告開關
        /// </summary>
        public byte? AnnMsgShow { get; set; }


        /// <summary>
        /// 登入彈跳活動視窗
        /// </summary>
        public byte? ShowCampaignAtLogin { get; set; }

        /// <summary>
        /// 登入彈跳首充返利活動視窗
        /// </summary>
        public byte? ShowFirstDepositAtLogin { get; set; }

        /// <summary>
        /// 是否設定為预设公告使用
        /// </summary>
        public byte? SetDefault { get; set; }

        /// <summary>
        /// JSON設定值
        /// </summary>
        public string AreaConfig { get; set; }

        /// <summary>
        /// 遊客升級綁定帳號, 贈送金幣
        /// </summary>
        [MoneyInt]
        public long GiftMoney { get; set; }

        /// <summary>
        /// 註冊預設金幣
        /// </summary>
        [MoneyInt]
        public long DefaultMoney { get; set; }
        /// <summary>
        /// 支付圖像切換
        /// </summary>
        public byte? ShopNameID { get; set; }
        /// <summary>
        /// VIP圖像切換
        /// </summary>
        public byte? AgentNameID { get; set; }

        /// <summary>
        /// 兑换未达成强制显示
        /// </summary>
        public byte? ShowConvertPopup { get; set; }

        /// <summary>
        /// 兑换禁止提示文本
        /// </summary>
        public string ConvertPopupText { get; set; }

        /// <summary>
        /// 绑定手机显示兑换
        /// </summary>
        public byte? ShowConvertBindPhone { get; set; }

        /// <summary>
        /// 绑定手机显示所有游戏
        /// </summary>
        public byte? ShowAllGameBindPhone { get; set; }

        /// <summary>
        /// 绑定手机显示VIP代理充值
        /// </summary>
        public byte? ShowVIPAgentBindPhone { get; set; }

        /// <summary>
        /// 绑定手机显示客服
        /// </summary>
        public byte? ShowCSBindPhone { get; set; }

    }

    public class BundleAreaInfo
    {
        public int AreaID { get; set; }
        public int PlatformID { get; set; }
    }
}
