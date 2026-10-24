using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.DTO.ThirdPartyPay
{
    /// <summary>
    /// 四方(新王朝)創建訂單POST模型
    /// </summary>
    public class HFZCPayOrderModel
    {
        /// <summary>
        /// 時間字串格式
        /// </summary>
        private string datetimeFormat = "yyyyMMddHHmmss";
        /// <summary>
        /// 建構式
        /// </summary>
        public HFZCPayOrderModel()
        {
            version = "V2";
            signType = "MD5";
            date = DateTime.Now.ToString(datetimeFormat);
        }
        /// <summary>
        /// 版本号 默认：V2
        /// </summary>
        public string version { set; get; }
        /// <summary>
        /// 签名类型 MD5  RSA
        /// </summary>
        public string signType { set; get; }
        /// <summary>
        /// 商户号 本系统统一分配
        /// </summary>
        public string merchantNo { set; get; }
        /// <summary>
        /// 时间戳 格式：YYYYMMDDhhmmss
        /// </summary>
        public string date { set; get; }
        /// <summary>
        /// 通道类型
        /// </summary>
        public int channleType { set; get; }
        /// <summary>
        /// 加密串
        /// </summary>
        public string sign { set; get; }
        /// <summary>
        /// 訂單號
        /// </summary>
        public string orderNo { set; get; }
        /// <summary>
        /// 金額 单位：元
        /// </summary>
        public long bizAmt { set; get; }
        /// <summary>
        /// 银行代码 部分通道需要(网关，暫時不接)
        /// </summary>
        public string bankCode { set; get; }
        /// <summary>
        /// 同步回调地址
        /// </summary>
        public string returnUrl { set; get; }
        /// <summary>
        /// 異步回調地址
        /// </summary>
        public string noticeUrl { set; get; }
        /// <summary>
        /// 会员ID 玩家id  部分通道需要
        /// </summary>
        public int customNo { set; get; }
        /// <summary>
        /// API密鑰
        /// </summary>
        public string token { set; get; }
    }

    /// <summary>
    /// 創建訂單響應數據Detail
    /// </summary>
    public class HFZCPayReturnDetail
    {
        /// <summary>
        /// rul
        /// </summary>
        public string PayURL { set; get; }
        /// <summary>
        /// 表单格式
        /// </summary>
        public string PayHtml { set; get; }
    }

    /// <summary>
    /// 創建訂單響應數據
    /// </summary>
    public class HFZCPayReturnModel
    {
        /// <summary>
        /// 响应码 0成功 -1失败
        /// </summary>
        public sbyte code { set; get; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string msg { set; get; }
        /// <summary>
        /// 回傳URL或表单格式
        /// </summary>
        public HFZCPayReturnDetail detail { set; get; }
    }

    /// <summary>
    /// 從四方POST回傳給PayCenter模型(因為可能是URL也可能是表單)
    /// </summary>
    public class HFZCPayReturnToPayCenterModel
    {
        /// <summary>
        /// 回傳值是FORM或是URL
        /// </summary>
        public string Method { set; get; }
        /// <summary>
        /// 回傳內容
        /// </summary>
        public string Html { set; get; }
    }

    /// <summary>
    /// 支付异步回调
    /// </summary>
    public class HFZCPayResponseModel
    {
        /// <summary>
        /// 商户訂單號
        /// </summary>
        public string orderNo { set; get; }
        /// <summary>
        /// 代付金額
        /// </summary>
        public string bizAmt { set; get; }
        /// <summary>
        /// 代付金額 字串轉數字
        /// </summary>
        public long bizAmtMoney
        {
            get
            {
                return (long)Decimal.Parse(bizAmt);
            }
        }
        /// <summary>
        /// 狀態 1 成功 2失败 0处理中
        /// </summary>
        public int status { set; get; }
        /// <summary>
        /// 狀態信息
        /// </summary>
        public string remark { set; get; }
        /// <summary>
        /// 常量 V2
        /// </summary>
        public string version { set; get; }
        /// <summary>
        /// 時間戳yyyyMMddHHmmss
        /// </summary>
        public string date { set; get; }
        /// <summary>
        /// 签名
        /// </summary>
        public string sign { set; get; }
    }

}
