using System;
using FW.Core.Helper.Enum;

namespace game.Entity.Game
{
 

    public enum Behavior
    {
        [EnumDescriptionAttribute("複製网址")]
        Copy = 0,

        [EnumDescriptionAttribute("连结广告位置")]
        OpenHttpLink = 1,

        [EnumDescriptionAttribute("一键入厅")]
        IntoGame = 2,

        [EnumDescriptionAttribute("無行為")]
        NoAction = 3,

        [EnumDescriptionAttribute("前往充值")]
        GoPayment = 4,

        [EnumDescriptionAttribute("前往苹果商店")]
        GoApple = 5,

        [EnumDescriptionAttribute("取得代理微信联络资讯")]
        GoWeChat = 6,

        [EnumDescriptionAttribute("跳转渠道官网")]
        GoOfficialSite = 7,
        
        [EnumDescriptionAttribute("前往邮件")]
        Go_UI_Mail = 101,

        [EnumDescriptionAttribute("前往公告")]
        Go_UI_Announecement = 102,

        [EnumDescriptionAttribute("前往活动详细说明")]
        Go_Specific_PromotionEvent = 151,

        [EnumDescriptionAttribute("前往保险箱")]
        Go_UI_MoneyBank = 201,
        
        [EnumDescriptionAttribute("前往兑换管理")]
        Go_UI_Exchange = 351,        
        
        [EnumDescriptionAttribute("前往个人信息")]
        Go_UI_UserInfo = 401,

        [EnumDescriptionAttribute("前往客服与帮助")]
        Go_UI_Help = 501,

        [EnumDescriptionAttribute("前往申请全民代理")]
        Go_UI_AgentApplyPopup = 7001,

        [EnumDescriptionAttribute("前往全民代理制度说明")]
        Go_UI_Agent = 7002,

        [EnumDescriptionAttribute("前往直播")]
        Go_UI_LiveStream = 9001,

        [EnumDescriptionAttribute("前往登入享好礼")]
        Go_UI_Event_RegisterPromotion = 9901,
        
        [EnumDescriptionAttribute("前往注册享好礼")]
        Go_UI_Event_DailyLoginPromotion = 9902,

     

    }


}
