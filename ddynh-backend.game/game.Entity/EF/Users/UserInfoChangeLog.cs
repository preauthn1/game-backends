using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Users
{
    /// <summary>
    /// 用户信息变更记录
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class UserInfoChangeLog : BaseEntity
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserID
        {
            get;
            set;
        }

        /// <summary>
        /// 用户信息变化类型
        /// </summary>
        public UserInfoChangeType ChangeType
        {
            get;
            set;
        }

        /// <summary>
        /// 修改前的数据
        /// </summary>
        public string OldData
        {
            get;
            set;
        }

        /// <summary>
        /// 修改后的数据
        /// </summary>
        public string NewData
        {
            get;
            set;
        }

        /// <summary>
        /// IP
        /// </summary>
        public string IPAddress
        {
            get;
            set;
        }

        /// <summary>
        /// 说明
        /// </summary>
        public string Memo
        {
            get;
            set;
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime WriteTime
        {
            get;
            set;
        }

        public virtual UserInfo User { get; set; }
    }


}
