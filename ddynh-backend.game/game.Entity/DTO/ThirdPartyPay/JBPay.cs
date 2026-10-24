using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.DTO.ThirdPartyPay
{
    /// <summary>
    /// 創建訂單資料格式
    /// </summary>
    public class JBPayOrderModel
    {
        /// <summary>
        /// 商戶帳號
        /// </summary>
        public string client_id { set; get; }
        /// <summary>
        /// 商戶密鑰
        /// </summary>
        public string key { set; get; }
        /// <summary>
        /// 付款方式: 請使用下列參數。ALI_H5: 支付寶 H5、ALI_QR: 支付寶掃碼、WEI_QR: 微信扫码、WEI_H5: 微信H5、IBK: 网银、UNION: 銀聯快捷、UNION_QR: 銀聯二維碼、QQ_QR: QQ錢包掃碼、QQ_H5: QQ錢包H5、XFT: 財付通、CLD: 雲閃付、JD_QR: 京東掃碼
        /// </summary>
        public string slug { set; get; }
        /// <summary>
        /// 商戶訂單編號
        /// </summary>
        public string order_id { set; get; }
        /// <summary>
        /// 交易金額(小數兩位)
        /// </summary>
        public string amount { set; get; }
        /// <summary>
        /// 用戶 IP
        /// </summary>
        public string client_ip { set; get; }
        /// <summary>
        /// 用戶設備:Enum:android、ios、pc
        /// </summary>
        public string client_device { set; get; }
        /// <summary>
        /// 異步通知網址
        /// </summary>
        public string notify_url { set; get; }
        /// <summary>
        /// 商戶其他付款資料，請用JSON格式，支付成功，原資料返回
        /// </summary>
        public string attach { set; get; }
        /// <summary>
        /// 銀行代號，slug選擇網銀支付(IBK)為必填
        /// </summary>
        public string bank_code { set; get; }
        /// <summary>
        /// 完成付款後導向的網址，slug選擇網銀支付(IBK)為必填
        /// </summary>
        public string return_url { set; get; }
        /// <summary>
        /// 簽名: 將 key 加入參數組、依字母升冪排序、清除空白參數，再用 MD 5 加密轉成小寫。
        /// </summary>
        public string sign { set; get; }
    }

    /// <summary>
    /// 創建訂單返回参数data內容
    /// </summary>
    public class JBPayReturnDataModel
    {
        /// <summary>
        /// 我方建立的訂單編號
        /// </summary>
        public string order_id { set; get; }
        /// <summary>
        /// 充值付款網址
        /// </summary>
        public string read_url { set; get; }
        /// <summary>
        /// 充值訊息
        /// </summary>
        public string message { set; get; }
    }

    /// <summary>
    /// 創建訂單返回参数
    /// </summary>
    public class JBPayReturnModel
    {
        /// <summary>
        /// 交易狀態 0:交易成功, 非 0 均為交易失敗
        /// </summary>
        public string code { set; get; }
        /// <summary>
        /// 錯誤充值訊息
        /// </summary>
        public string message { set; get; }
        /// <summary>
        /// 充值資料
        /// </summary>
        public JBPayReturnDataModel data { set; get; }
    }

    /// <summary>
    /// 異步回調模型
    /// </summary>
    public class JBPayResponseModel
    {
        /// <summary>
        /// 商戶帳號
        /// </summary>
        public string client_id { set; get; }
        /// <summary>
        /// 商戶單號
        /// </summary>
        public string merchant_order { set; get; }
        /// <summary>
        /// 交易金額
        /// </summary>
        public string amount { set; get; }
        /// <summary>
        /// 金额 轉數字型態
        /// </summary>
        public int amountMoney
        {
            get
            {
                return (int)Decimal.Parse(amount);
            }
        }
        /// <summary>
        /// 商戶自定義資料欄位
        /// </summary>
        public string attach { set; get; }
        /// <summary>
        /// 簽名:sign 將 key 加入參數組、依字母升冪排序、清除空白參數，再用 MD 5 加密後轉成小寫
        /// </summary>
        public string sign { set; get; }
    }
}
