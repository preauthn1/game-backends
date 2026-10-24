namespace game.Entity.DTO.GameMerchantApi.BG
{
    public class BGQueryUserResponseModel
    {
        public string id { get; set; }
        public BGQueryUserResultResponseModel result { get; set; }
        public object error { get; set; }
        public string jsonrpc { get; set; }
    }


    public class BGQueryUserResultResponseModel
    {
        public object birthday { get; set; }
        public object certType { get; set; }
        public int userStatus { get; set; }
        public string loginId { get; set; }
        public object gender { get; set; }
        public string parentPath { get; set; }
        public int awardPoint { get; set; }
        public string memo { get; set; }
        public object remark { get; set; }
        public object idNumber { get; set; }
        public string parentPathIncSelf { get; set; }
        public object userImage { get; set; }
        public object balance { get; set; }
        public string nickname { get; set; }
        public object tel { get; set; }
        public string currency { get; set; }
        public string sn { get; set; }
        public object wechatName { get; set; }
        public string realSn { get; set; }
        public string regType { get; set; }
        public object email { get; set; }
        public object passportNumber { get; set; }
        public object qq { get; set; }
        public object alipay { get; set; }
        public object address { get; set; }
        public object mobile { get; set; }
        public object wechat { get; set; }
        public object alipayName { get; set; }
        public int userId { get; set; }
        public int parentId { get; set; }
        public string regIp { get; set; }
        public int loginCount { get; set; }
        public int recommendUserId { get; set; }
        public object certNumber { get; set; }
        public int unreadNotice { get; set; }
        public string regTime { get; set; }
        public string name { get; set; }
        public object payPassword { get; set; }
        public object loginMobile { get; set; }
        public int status { get; set; }
    }



}
