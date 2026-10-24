using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.ViewModel
{
    public class UserInfoDetail
    {
        public int Id { get; set; }

        public int UserID { get; set; }

        /// <summary>
        /// 登陆名称
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 头像ID
        /// </summary>
        public int FaceID { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public Sex Sex { get; set; }

        /// <summary>
        /// 硬件类型
        /// </summary>
        public HDType HDType { get; set; }

        /// <summary>
        /// 硬件唯一码
        /// </summary> 
        public string HDCode { get; set; }

        /// <summary>
        /// 用户类型
        /// </summary>
        public UserType UserType { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 创建IP地址
        /// </summary>      
        public string CreateIPAddress { get; set; }

        /// <summary>
        /// 最后登陆时间
        /// </summary>
        public DateTime LastLogonTime { get; set; }

        /// <summary>
        /// 最后登录IP地址
        /// </summary>
        public string LastLogonIPAddress { get; set; }

        /// <summary>
        /// 最后登录機器號碼
        /// </summary>
        public string LastLogonHDCode { get; set; }

        /// <summary>
        /// 支付宝姓名
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// 支付宝账号
        /// </summary>
        public string AlipayAccount { get; set; }

        /// <summary>
        /// 銀行卡帳號
        /// </summary>
        public string UnionpayAccount { get; set; }

        /// <summary>
        /// 銀聯卡姓名
        /// </summary>
        public string UnionpayRealName { get; set; }

        /// <summary>
        /// 免費體驗幣
        /// </summary>
        public long ExperienceCash { get; set; }

        /// <summary>
        /// 第三方充值总额
        /// </summary>
        public long SumCharge { get; set; }

        /// <summary>
        /// 官方充值总额
        /// </summary>
        public long SumAgentCharge { get; set; }

        /// <summary>
        /// 贡献总税收
        /// </summary>
        public long SumTax { get; set; }

        /// <summary>
        /// 登录总数
        /// </summary>
        public int SumLoginTimes { get; set; }

        /// <summary>
        /// 总兑换金额
        /// </summary>
        public long SumConvert { get; set; }

        /// <summary>
        /// 总兑换次数
        /// </summary>
        public int SumConvertTimes { get; set; }

        /// <summary>
        /// 总手续费金额
        /// </summary>
        public long SumConvertTax { get; set; }

        public long SumConvertTotal
        {
            get
            {
                return SumConvert + SumConvertTax;
            }
        }

        /// <summary>
        /// 总免费金币
        /// </summary>
        public long? SumFreeMoney { get; set; }

        public long SumChargeTotal
        {
            get
            {
                return SumCharge + SumAgentCharge + (SumFreeMoney == null ? 0 : SumFreeMoney.Value);
            }
        }

        /// <summary>
        /// 管理员加金币
        /// </summary>
        public long? SumAdminMoney { get; set; }

        /// <summary>
        /// 用户提问总数
        /// </summary>
        public int QuestionCount { get; set; }

        /// <summary>
        /// 用户等级
        /// </summary>
        public int GradeID { get; set; }

        /// <summary>
        /// 渠道ID
        /// </summary>
        public int AreaID { get; set; }

        /// <summary>
        /// 所属渠道名称
        /// </summary>
        public string AreaName { get; set; }

        /// <summary>
        /// 平台ID
        /// </summary>
        public int PlatformID { get; set; }

        /// <summary>
        /// 所属平台名称
        /// </summary>
        public string PlatformName { get; set; }

        /// <summary>
        /// 用户现金数
        /// </summary>
        public long CashAmount { get; set; }

        /// <summary>
        /// 用户银行数
        /// </summary>
        public long BankAmount { get; set; }

        /// <summary>
        /// 用户总金币数
        /// </summary>
        public long AllCoinAmount { get { return CashAmount + BankAmount; } }

        public int Rate { get; set; }
        
        /// <summary>
        /// 分成比例上限
        /// </summary>
        public int TopUseRrate { get; set; }

        [Required(ErrorMessage = "必须输入")]
        [Range(0, 100, ErrorMessage = "超过设定上限")]      
        public decimal UserRate
        {
            get { return Convert.ToDecimal(this.Rate) * 0.1m; }
            set { this.Rate = Convert.ToInt32(Math.Floor(value * 10m)); }
        }

        public byte StageLevel { get; set; }

        /// <summary>
        /// 分成比例状态
        /// </summary>
        public int ModifyStatus { get; set; }

        /// <summary>
        /// 冻结状态
        /// </summary>
        public int UserStatus { get; set; }

        /// <summary>
        /// 代理申请状态
        /// </summary>
        public int AgentApplyStatus { get; set; }
    }
    /// <summary>
    /// 性别
    /// </summary>
    public enum Sex : byte
    {
        [Description("男")]
        Man = 0,
        [Description("女")]
        Woman = 1
    }

    /// <summary>
    /// 硬件类型
    /// </summary>
    public enum HDType : byte
    {
        [Description("PC")]
        PC = 1,
        [Description("安卓")]
        Android = 2,
        [Description("苹果")]
        IOS = 3
    }

    /// <summary>
    /// 用户类型
    /// </summary>
    public enum UserType : byte
    {
        [Description("正式账号")]
        Reg = 1,
        [Description("机器人")]
        Rebot = 2,
        [Description("游客号")]
        Fast = 3,
    }

}
