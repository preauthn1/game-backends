using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Message
{
    /// <summary>
    /// 消息
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class MessageList:BaseEntity
    {
        /// <summary>
        /// 题目
        /// </summary>
        [Required]
        [StringLength(128)]
        public String Title { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        [Required]
        [StringLength(1024)]
        public String MsgContent { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public MessageType Type { get; set; }

        /// <summary>
        /// 用户
        /// </summary>
        public int FromUserID { get; set; }

        /// <summary>
        /// 种类
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
