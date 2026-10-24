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
    /// 短消息
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class Message : BaseEntity
    {
        /// <summary>
        /// 消息标题
        /// </summary>
        [Required]
        [StringLength(128)]
        public string Title
        {
            get;
            set;
        }

        /// <summary>
        /// 消息内容
        /// </summary>
        [Required]
        [StringLength(1024)]
        public string MsgContent
        {
            get;
            set;
        }
        /// <summary>
        /// 消息类型
        /// </summary>
        public MessageType Type
        {
            get;
            set;
        }

        /// <summary>
        /// 发送者
        /// </summary>
        public int? FromUserID
        {
            get;
            set;
        }

        /// <summary>
        /// 排序号
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime
        {
            get;
            set;
        }

        /// <summary>
        /// 所属平台
        /// </summary>
        public String Platform
        {
            get;
            set;
        }

        /// <summary>
        /// 用户信息
        /// </summary>
        public virtual UserInfo User
        {
            get;
            set;
        }



    }

}
