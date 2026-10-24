using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.DTO.ThirdPartyPay
{
    public class DSDFPayOrderModel
    {
        /// <summary>
        /// 创建接入商公司时生成的的数字，登录风云聚合收付平台获取 
        /// </summary>
        public string cid { set; get; }
        /// <summary>
        /// 此订单的会员ID，运营网站系统的会员唯一标识 
        /// </summary>
        public string uid { set; get; }
        /// <summary>
        /// 此次操作的时间，格式: UNIX时间戳 秒值，超过３分钟则为非法 订单 (即此参数值和风云系统收到请求的时间差不能超过３分钟) 
        /// </summary>
        public long time { set; get; }
        /// <summary>
        /// 充值的金额，单位 元 ，最多可带两位小数点，如: 1.23, 2.6 
        /// </summary>
        public int amount { set; get; }
        /// <summary>
        /// 接入商网站生成的订单号，​保证唯一性，否则误上分概不负责 
        /// </summary>
        public string order_id { set; get; }
        /// <summary>
        /// 此次操作的会员IP地址，风云系统会获取会员的真实IP和此参数进行 比较，不相同则为非法订单。同时用于对会员进行省份分类。 
        /// </summary>
        public string ip { set; get; }
        /// <summary>
        /// 使用公司API KEY对参数组成的字符串进行签名，签名算法见示例代 码，字符串必须是​按此顺序组合，只此6个​： cid=xxx&uid=xxx&time=xxx&amount=xxx&order_id=xxx&ip=xxx 
        /// </summary>
        public string sign { set; get; }
        /// <summary>
        /// 订单状态改变后同步跳转地址，格式 http://example.invalid/path 
        /// </summary>
        public string syncurl { set; get; }
        /// <summary>
        /// 指定会员等级分组: beginner,junior,experienced,vip1,vip2,vip3 
        /// </summary>
        public string clevel { set; get; }
        /// <summary>
        /// 分组标识，如果有此参数，则只分配此分组内的收款卡 
        /// </summary>
        public string gflag { set; get; }
        /// <summary>
        /// 存款方式，可选，值如下：remit: 银行卡转账 ,qrcode: 二维码存款,online: 在线网银，quick: 快捷支付,quickp2p: 快捷直连 如果没有带入此参数，则自动进去风云聚合收付的收银台，由会员选 择存款方式 
        /// </summary>
        public string type { set; get; }
        /// <summary>
        /// 存款银行，可选，​如果没有传入 type 参数，则此参数无效​。如下：ALIPAY: 支付宝 , WebMM: 微信支付,QQPAY: QQ钱包，ABC: 农业银行 … (见后面的银行编码表) 
        /// </summary>
        public string tflag { set; get; }
        /// <summary>
        /// 自定义参数，推送订单异步通知原样返回 
        /// </summary>
        public string extend { set; get; }
        /// <summary>
        /// 存款银行卡姓名，可选，用于 type = remit/quickp2p 情况 
        /// </summary>
        public string from_username { set; get; }
        /// <summary>
        /// 存款银行卡号，可选，用于 type = remit/quickp2p 情况 
        /// </summary>
        public string from_cardnumber { set; get; }
        /// <summary>
        /// 存款附言，可选，用于 type = remit 情况 
        /// </summary>
        public string comment { set; get; }
        /// <summary>
        /// 身份证号， 可选，用于 type = quickp2p 情况 
        /// </summary>
        public string idnumber { set; get; }
        /// <summary>
        /// 手机号码，可选，用于 type = quickp2p 情况 
        /// </summary>
        public string phonenumber { set; get; }
        /// <summary>
        /// 值：0 / 1 / 2 / 3；  是否使用卡商代收  不传此参数或值为0 为不使用卡商 1=使用全局卡商并且速度优先  2=使用全局卡商并且费率优先  3=使用财务公司内部卡商 
        /// </summary>
        public string use_card_mch { set; get; }
        /// <summary>
        /// 值：1 / 20； 不传此值默认为 1 支付宝转卡的订单专用参数，支付宝转卡有多种模式，传递该参数决 定使用何种模式，不同的模式扫码后看到的页面不一样 
        /// </summary>
        public string alimode { set; get; }
        public string key { set; get; }
    }

    public class DSDFPayResponseModel
    {
        /// <summary>
        /// 接入商网站提交的订单号 
        /// </summary>
        public string order_id { set; get; }
        /// <summary>
        ///  in/out,  充值 or 提现 
        /// </summary>
        public string direction { set; get; }
        /// <summary>
        ///  金额，​单位 分​，没有小数点 (如果做了自动小数,则和原始金额不一样) 
        /// </summary>
        public long amount { set; get; }
        /// <summary>
        ///  xxxx,   金额，​单位 分​，没有小数点, 原始金额 
        /// </summary>
        public long orig_amount { set; get; }
        /// <summary>
        ///  xxx,   接入商网站的会员ID(提单API传递过来的) 
        /// </summary>
        public string customer_name { set; get; }
        /// <summary>
        /// xxx,   订单完成时间，UNIX时间戳秒值 
        /// </summary>
        public long verified_time { set; get; }
        /// <summary>
        ///  xxx,   订单创建时间，UNIX时间戳秒值 
        /// </summary>
        public long created_time { set; get; }
        /// <summary>
        ///  "verified / timeout / revoked"  ​此参数判断订单结果：verified = 成功支付 
        /// </summary>
        public string status { set; get; }
        /// <summary>
        /// “remit/qrcode/online/quick”,   会员存款方式 (充值有此参数) 
        /// </summary>
        public string type { set; get; }
        /// <summary>
        ///  “ALIPAY/WebMM/CMB/ABC...”  会员存款卡类型  (充值有此参数) 
        /// </summary>
        public string customer_bankflag { set; get; }
        /// <summary>
        /// 收款卡银行,   (充值有此参数) 
        /// </summary>
        public string to_bankflag { set; get; }
        /// <summary>
        /// 收款卡号,    (充值有此参数) 
        /// </summary>
        public string to_cardnumber { set; get; }
        /// <summary>
        /// 收款卡姓名,    (充值有此参数) 
        /// </summary>
        public string to_username { set; get; }
        /// <summary>
        /// 会员是否手机提单 (充值有此参数) 
        /// </summary>
        public bool is_mobile { set; get; }
        /// <summary>
        /// 订单是否分配的WAP收款卡 (充值有此参数) 
        /// </summary>
        public bool is_wap { set; get; }
        /// <summary>
        /// 会员提现，系统使用的出款卡  (提现有此参数) 
        /// </summary>
        public string out_cardnumber { set; get; }
        /// <summary>
        ///  会员提现，此笔订单的转账佣金 (提现有此参数)  ​单位 分 
        /// </summary>
        public string trans_fee { set; get; }
        /// <summary>
        /// 简易签名，签名内容：order_id=xxx&amount=xxx&verified_time=xxx，签名算法同上，见示例代码，不知如何获取 http raw body，可以使用此参数进行验签 
        /// </summary>
        public string qsign { set; get; }
    }
}
