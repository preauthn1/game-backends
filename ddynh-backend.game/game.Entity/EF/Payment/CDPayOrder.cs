using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using game.Entity.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Payment
{
    /// <summary>
    /// 订单
    /// </summary>
    public class CDPayOrder : BaseEntity
    {
        public int PayTypeID { get; set; }
        public int UserID { get; set; }
        public int Amount { get; set; }
        public CDOrderStatus Status { get; set; }
        public string IPAddress { get; set; }
        public string TransID { get; set; }
        public string TransData { get; set; }
        public DateTime CreateTime { get; set; }
        public int? GWID { get; set; }
        public int Id { get; set; }
    }

    /// <summary>
    /// 订单状态
    /// </summary>
    public enum CDOrderStatus : byte
    {
        [EnumDescription("待支付")]
        Waiting = 1,
        [EnumDescription("已支付")]
        Paied = 2,
        [EnumDescription("支付失败")]
        fail = 3
    }
}
