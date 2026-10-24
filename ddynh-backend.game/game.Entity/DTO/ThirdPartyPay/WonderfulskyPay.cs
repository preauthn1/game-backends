using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.DTO.ThirdPartyPay
{
    public class WonderfulskyPayOrderModel
    {
        /// <summary>
        /// 請求類型:固定值為“Payment”
        /// </summary>
        public string service { set; get; }
        /// <summary>
        /// 商戶編號:
        /// </summary>
        public string mid { set; get; }
        /// <summary>
        /// 商戶訂單號:此訂單號不能重複
        /// </summary>
        public string merchantid { set; get; }
        /// <summary>
        /// 支付金額:單位:元 (小數後兩位，確認整數可直接使用)
        /// </summary>
        public int amount { set; get; }
        /// <summary>
        /// 交易幣種:固定值為“CNY”
        /// </summary>
        public string currency { set; get; }
        /// <summary>
        /// 通知地址:支付完成後，系統會把支付成功的通知發到該參數網址。
        /// </summary>
        public string notifyurl { set; get; }
        /// <summary>
        /// 返回地址:支付完成後，系統會跳轉到該參數網址。
        /// </summary>
        public string returnurl { set; get; }
        /// <summary>
        /// 支付通道編碼:支付通道編碼詳見通道編碼表 此處不填將跳轉至收銀台
        /// </summary>
        public string channel { set; get; }
        /// <summary>
        /// 備註:此處不參加加密
        /// </summary>
        public string remark { set; get; }
        /// <summary>
        /// 簽名數據:加密後的加密串
        /// </summary>
        public string sign { set; get; }
        /// <summary>
        /// 商戶密鑰
        /// </summary>
        public string key { set; get; }
        public string rsaPublicKey { set; get; }
        public string rsaPrivateKey { set; get; }
    }

    public class WonderfulskyPayReturnModel
    {
        /// <summary>
        /// 請求結果:“1”:成功 “0”:失敗
        /// </summary>
        public byte result { set; get; }
        /// <summary>
        /// 二維碼連結:URL
        /// </summary>
        public string qrcodeurl { set; get; }
        /// <summary>
        /// 商戶編號
        /// </summary>
        public string mid { set; get; }
        /// <summary>
        /// 商戶訂單號
        /// </summary>
        public string merchantid { set; get; }
        /// <summary>
        /// 簽名數據:加密後的加密串
        /// </summary>
        public string sign { set; get; }
        /// <summary>
        /// 錯誤代碼
        /// </summary>
        public string code { set; get; }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string message { set; get; }
    }

    public class WonderfulskyPayResponseModel
    {
        /// <summary>
        /// 商戶編號
        /// </summary>
        public string mid { set; get; }
        /// <summary>
        /// 商戶訂單號:此訂單號不會重複
        /// </summary>
        public string merchantid { set; get; }
        /// <summary>
        /// 系統訂單號:此訂單號為我們系統對此訂單的代號
        /// </summary>
        public string systemid { set; get; }
        /// <summary>
        /// 支付金額:單位:元 (小數後兩位)
        /// </summary>
        public string amount { set; get; }

        public int amountInt { get { return (int)Decimal.Parse(amount); } }
        /// <summary>
        /// 訂單狀態:“1”:成功
        /// </summary>
        public string status { set; get; }
        /// <summary>
        /// 返回時間:此為返回通知時的系統時間
        /// </summary>
        public string time { set; get; }
        /// <summary>
        /// 備註:此處不參加加密
        /// </summary>
        public string remark { set; get; }
        /// <summary>
        /// 簽名數據:加密後的加密串
        /// </summary>
        public string sign { set; get; }
    }

    public class WonderfulskyPayResponseGetModel : WonderfulskyPayResponseModel
    {
        //天空付的回調可能使用GET或POST回傳，為了用兩個function接受天空付回調，這個與WonderfulskyPayResponseModel是相同的。
    }
}
