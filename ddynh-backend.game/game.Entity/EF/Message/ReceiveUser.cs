using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using game.Entity.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Message
{
    /// <summary>
    /// 用户消息
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class ReceiveUser : BaseEntity
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public int MsgID
        {
            get;
            set;
        }

        /// <summary>
        /// 消息內容
        /// </summary>
        public string MsgContent
        {
            get;
            set;
        }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserID
        {
            get;
            set;
        }

        /// <summary>
        /// 登入名稱
        /// </summary>
        public string LoginName
        {
            get;
            set;
        }

        /// <summary>
        /// 用户等级
        /// </summary>
        public int GradeID
        {
            get;
            set;
        }

        /// <summary>
        /// 消息状态
        /// </summary>
        public MessageStatus Status
        {
            get;
            set;
        }
        
        /// <summary>
        /// 读时间
        /// </summary>
        public DateTime? ReadTime
        {
            get;
            set;
        }

        /// <summary>
        /// 所属平台
        /// </summary>
        public string Platform
        {
            get;
            set;
        }

        public virtual UserInfo User
        {
            get;
            set;
        }

        public virtual Message Message
        {
            get;
            set;
        }
    }

 

}
