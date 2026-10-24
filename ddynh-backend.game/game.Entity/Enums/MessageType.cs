using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using game.Entity.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Message
{
  
    /// <summary>
    /// 消息类型
    /// </summary>
    public enum MessageType : byte
    {
        /// <summary>
        /// 系统消息
        /// </summary>
        [EnumDescription("系统消息")]
        System = 1
    }


    /// <summary>
    /// 消息 次分類
    /// </summary>
    public enum MessageSubType 
    {
        [EnumDescription("預設無分類")]
        Default = 1,

        [EnumDescription("支付逾時或支付失敗通知")]
        PaymentOrderFailNotify = 21
    }




}
