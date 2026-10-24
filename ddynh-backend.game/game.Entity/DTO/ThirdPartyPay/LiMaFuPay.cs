using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.DTO.ThirdPartyPay
{
    /// <summary>
    /// 創建訂單
    /// </summary>
    public class LiMaFuPayOrderModel
    {
        /// <summary>
        /// 商户号:平台分配商户号
        /// </summary>
        public string merchant_id { set; get; }
        /// <summary>
        /// 订单号:上送订单号唯一, 字符长度20
        /// </summary>
        public string orderid { set; get; }
        /// <summary>
        /// 支付方式:WX:微信扫码，返回扫码跳转、WXH5：微信wap、ZFB：支付宝扫码，返回扫码跳转、ZFBH5:支付宝wap、YSF:云闪付、YLNET：银联扫码、YLNETH5：银联扫码wap、NETGATE:网银支付、QQ: QQ扫码、QQH5：QQ扫码wap、BAIDU:百度钱包、BAIDUH5：百度钱包wap、JD：京东支付、JDH5：京东支付wap
        /// </summary>
        public string paytype { set; get; }
        /// <summary>
        /// 服务端通知:服务端返回地址.（POST返回数据）
        /// </summary>
        public string notifyurl { set; get; }
        /// <summary>
        /// 页面跳转通知:页面跳转返回地址（POST返回数据）
        /// </summary>
        public string callbackurl { set; get; }
        /// <summary>
        /// 客户IP地址:客服端的地址（微信必须使用，否则有安全限制不能付款）
        /// </summary>
        public string userip { set; get; }
        /// <summary>
        /// 订单金额:商品金额
        /// </summary>
        public string money { set; get; }
        /// <summary>
        /// MD5签名
        /// </summary>
        public string sign { set; get; }
        /// <summary>
        /// 商戶密鑰
        /// </summary>
        public string key { set; get; }
    }

    /// <summary>
    /// 訂單返回Data Model
    /// </summary>
    public class LiMaFuPayReturnDataModel
    {
        /// <summary>
        /// url：为跳转地址，获取此地址直接跳转即可；
        /// </summary>
        public string url { set; get; }
        /// <summary>
        /// qrcode:二维码，
        /// </summary>
        public string qrcode { set; get; }
    }

    /// <summary>
    /// 訂單返回Model
    /// </summary>
    public class LiMaFuPayReturnModel
    {
        /// <summary>
        /// 1：正确、0：错误，此状态时，message中会有提示信息！
        /// </summary>
        public byte status { set; get; }
        /// <summary>
        /// 数据
        /// </summary>
        public LiMaFuPayReturnDataModel data { set; get; }
        /// <summary>
        /// 提示信息: 错误提示信息
        /// </summary>
        public string message { set; get; }
        /// <summary>
        /// 交易流水号:预留
        /// </summary>
        public string data2 { set; get; }
        public string page { set; get; }
    }

    /// <summary>
    /// 異步回調
    /// </summary>
    public class LiMaFuPayResponseModel
    {
        /// <summary>
        /// 商户编号:商户号
        /// </summary>
        public string merchant_id { set; get; }
        /// <summary>
        /// 平台订单号
        /// </summary>
        public string porder { set; get; }
        /// <summary>
        /// 订单号:商户订单号
        /// </summary>
        public string orderid { set; get; }
        /// <summary>
        /// 订单金额
        /// </summary>
        public string money { set; get; }
        /// <summary>
        /// 订单金额 轉數字型態
        /// </summary>
        public int moneyInt
        {
            get
            {
                return (int)Decimal.Parse(money);
            }
        }
        /// <summary>
        /// 交易状态:1 为成功，其余为失败
        /// </summary>
        public byte status { set; get; }
        /// <summary>
        /// 签名
        /// </summary>
        public string sign { set; get; }
    }
}
