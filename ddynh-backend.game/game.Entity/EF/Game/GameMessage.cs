using System;
using System.ComponentModel.DataAnnotations;
using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using game.Entity.Area;

namespace game.Entity.Game
{
    /// <summary>
    /// 游戏消息
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class GameMessage : BaseEntity
    {
        /// <summary>
        /// 平台ID
        /// </summary>
        public int PlatformID { get; set; }
        /// <summary>
        /// 消息内容
        /// </summary>
        [Required, StringLength(512)]
        public string MsgContent
        {
            get;
            set;
        }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime BeginTime
        {
            get;
            set;
        }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime
        {
            get;
            set;
        }

        /// <summary>
        /// 频率,单位秒
        /// </summary>
        public int Interval
        {
            get;
            set;
        }

        /// <summary>
        /// 状态
        /// </summary>
        public GameMessageStatus Status
        {
            get;
            set;
        }

        /// <summary>
        /// 最后发送时间
        /// </summary>
        public DateTime? LastSendTime
        {
            get;
            set;
        }

        public virtual Platform Platform { get; set; }

        /// <summary>
        /// 全平台
        /// </summary>
        public byte AllPlatform { get; set; }
    }

   
}
