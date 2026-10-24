using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Area;
using game.Entity.Users;
using game.Infrastructure;
using System;

namespace game.Entity.Agent
{
    /// <summary>
    /// 代理基本信息
    /// </summary>
    [Database(DBNameConfig.DBName_CenterDB)]
    public class AgentUserInfo : BaseEntity
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 微信号
        /// </summary>
        public string WXNo { get; set; }

        /// <summary>
        /// QQ号
        /// </summary>
        public string QQ { get; set; }

        /// <summary>
        /// 平台ID
        /// </summary>
        public int PlatformID { get; set; }

        /// <summary>
        /// 游戏用户ID
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 是否显示
        /// </summary>
        public bool IsShow { get; set; }

        /// <summary>
        /// 后台账号登录名称
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// 后台账号登录密码
        /// </summary>
        public string LoginPassword { get; set; }

        /// <summary>
        /// 后台账号ID
        /// </summary>
        public int AdminSysID { get; set; }

        /// <summary>
        /// 最后登录IP地址
        /// </summary>
        public string LastLoginIP { get; set; }

        /// <summary>
        /// 最后登陆时间
        /// </summary>
        public DateTime LastLoginTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        public virtual UserInfo User { get; set; }

        public virtual Platform Platform { get; set; }
    }
}