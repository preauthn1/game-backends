using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.ViewModel
{
    public class UserLoginInfo
    {
        public int UserID { get; set; }
        public string NickName { get; set; }
        public string PWD { get; set; }
        public string Encrypt { get; set; }
        /// <summary>
        /// 全民代理
        /// </summary>
        public int Mode1 { get; set; }
        /// <summary>
        /// 流水代理
        /// </summary>
        public int Mode2 { get; set; }
        /// <summary>
        /// 裂变代理
        /// </summary>
        public int Mode3 { get; set; }
        /// <summary>
        /// 联运代理
        /// </summary>
        public int Mode4 { get; set; }
        /// <summary>
        /// 无限极代理
        /// </summary>
        public int Mode5 { get; set; }
        /// <summary>
        /// 礼码代理
        /// </summary>
        public int Mode6 { get; set; }        
        public string QRURL { get; set; }
        public int Admintype { get; set; }
        public RoleType Role { get; set; }
    }

    public enum RoleType : int
    {
        [Description("管理员")]
        Admin = 0,
        [Description("财务")]
        CFO = 1,
        [Description("总代理")]
        Biz = 2,
        [Description("代理")]
        Agent = 3

    }

    public enum ModeType : int
    {
        [Description("全民代理")]
        Mode1 = 1,
        [Description("流水代理")]
        Mode2 = 2,
        [Description("裂变代理")]
        Mode3 = 3,
        [Description("联运代理")]
        Mode4 = 4,
        [Description("无限极代理")]
        Mode5 = 5,
        [Description("礼码代理")]
        Mode6 = 6
    }
}
