using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.DTO.ThirdPartyPay
{
    /// <summary>
    /// 通匯order post model
    /// </summary>
    public class THPayOrderModel
    {
        /// <summary>
        /// 商户ID:您的商户唯一标识，注册后在基本资料里获得
        /// </summary>
        public string uid { set; get; }
        /// <summary>
        /// 金额:单位：元。精确小数点后2位
        /// </summary>
        public string price { set; get; }
        /// <summary>
        /// 支付渠道:9：云闪付；11：支付宝转银行卡； 持续更新中，详见开发者中心
        /// </summary>
        public int paytype { set; get; }
        /// <summary>
        /// 异步回调地址:用户支付成功后，我们服务器会主动发送一个post消息到这个网址。由您自定义。不要urlencode并且不带任何参数。例：http://example.invalid/notify_url
        /// </summary>
        public string notify_url { set; get; }
        /// <summary>
        /// 同步跳转地址:用户支付成功后，我们会让用户浏览器自动跳转到这个网址。由您自定义。不要urlencode并且不带任何参数。例：http://example.invalid/return_url
        /// </summary>
        public string return_url { set; get; }
        /// <summary>
        /// 商户自定义订单号:我们会据此判别是同一笔订单还是新订单。我们回调时，会带上这个参数。例：201010101041
        /// </summary>
        public string user_order_no { set; get; }
        /// <summary>
        /// 附加内容(非必填):回调时将会根据传入内容原样返回（为防止乱码情况，请尽量不填写中文）
        /// </summary>
        public string note { set; get; }
        /// <summary>
        /// 商户自定义用户唯一标识(非必填):我们会显示在您后台的订单列表中，方便您看到是哪个用户的付款，方便后台对账。强烈建议填写。可以填用户名、邮箱、主键
        /// </summary>
        public string cuid { set; get; }
        /// <summary>
        /// 日期时间(非必填):请求时间yyyy-MM-dd hh:mm:ss
        /// </summary>
        public string tm { set; get; }
        /// <summary>
        /// 签名:将参数1至6按顺序连Token一起，做md5-32位加密，取字符串小写。网址类型的参数值不要urlencode（例：uid + price + paytype + notify_url + return_url + user_order_no + token）
        /// </summary>
        public string sign { set; get; }
        /// <summary>
        /// 密碼
        /// </summary>
        public string token { set; get; }
    }
    /// <summary>
    /// 通匯建立訂單JSON请求的返回值
    /// </summary>
    public class THPayReturnModel
    {
        /// <summary>
        /// 0失败 1成功
        /// </summary>
        public byte Code;
        /// <summary>
        ///返回消息
        /// </summary>
        public string Msg;
        /// <summary>
        /// 系统订单号
        /// </summary>
        public string OrderNo;
        /// <summary>
        /// 支付渠道 11支付宝 9雲閃付
        /// </summary>
        public byte PayType;
        /// <summary>
        /// 唤醒app路径
        /// </summary>
        public string callurl;
        /// <summary>
        /// 收款码解析路径
        /// </summary>
        public string QRCodeLink;
        /// <summary>
        /// 实际支付金额(一定要把这个价格显示在支付页上，而不是订单金额)
        /// </summary>
        public string RealPrice;
        /// <summary>
        /// 订单有效期（秒），订单创建时间 + 有效期 小于当前时间则此单失效
        /// </summary>
        public int TimeOut;
        /// <summary>
        /// 订单创建时间
        /// </summary>
        public DateTime CreateTime;
    }

    /// <summary>
    /// 第三方異步通知
    /// </summary>
    public class THPayResponseModel
    {
        /// <summary>
        /// 您的自定义订单号:一定存在。是您在发起付款接口传入的您的自定义订单号
        /// </summary>
        public string user_order_no { set; get; }
        /// <summary>
        /// 平台生成的订单号:一定存在。是此订单在本服务器上的唯一编号
        /// </summary>
        public string orderno { set; get; }
        /// <summary>
        /// 支付流水号:一定存在。支付宝支付或微信支付的流水订单号
        /// </summary>
        public string tradeno { set; get; }
        /// <summary>
        /// 订单定价:一定存在。是您在发起付款接口传入的订单价格
        /// </summary>
        public string price { set; get; }
        /// <summary>
        /// 实际支付金额:一定存在。表示用户实际支付的金额。一般会和price值一致，如果同时存在多个用户支付同一金额，就会和price存在一定差额，差额一般在1-2分钱上下，越多人同时付款，差额越大
        /// </summary>
        public string realprice { set; get; }
        /// <summary>
        /// 您的自定义用户唯一标识:如果您在发起付款接口带入此参数，我们会原封不动传回
        /// </summary>
        public string cuid { set; get; }
        /// <summary>
        /// 附加内容:如果您在发起付款接口带入此参数，我们会原封不动传回
        /// </summary>
        public string note { set; get; }
        /// <summary>
        /// 订单状态:0发起支付，1发起失败，2未支付，3支付成功，4支付异常，5伪删除, 6已支付。
        /// </summary>
        public int status { set; get; }
        /// <summary>
        /// 签名:将参数1至5按顺序连Token一起，做md5-32位加密，取字符串小写。您需要在您的服务端按照同样的算法，自己验证此sign是否正确。只在正确时，执行您自己逻辑中支付成功代码。
        /// （拼接顺序：user_order_no + orderno + tradeno + price + realprice + token） 
        /// </summary>
        public string sign { set; get; }
        /// <summary>
        /// 签名，含订单状态:将参数1至5按顺序连Token一起，做md5-32位加密，取字符串小写。您需要在您的服务端按照同样的算法，自己验证此sign是否正确。只在正确时，执行您自己逻辑中支付成功代码。
        /// （拼接顺序：user_order_no + orderno + tradeno + price + realprice+status + token）
        /// </summary>
        public string sign2 { set; get; }
    }
}
