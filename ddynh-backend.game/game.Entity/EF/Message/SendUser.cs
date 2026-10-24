using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Message
{
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class SendUser : BaseEntity
    {

        /// <summary>
        /// 发送消息的用户ID
        /// </summary>
        public int UserID { get; set; }       

        /// <summary>
        /// 发送的信息
        /// </summary>
        public virtual Message Message
        {
            get;
            set;
        }

        /// <summary>
        /// 发件人信息
        /// </summary>
        public virtual UserInfo User
        {
            get;
            set;
        }
    }
}
