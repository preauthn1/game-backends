using System;
using FW.Core.Helper;
using FW.Core.Repository;

namespace game.Entity.Stat
{

    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class TotalGameOnLineLog : BaseEntity
    {

        /// <summary>
        /// 平台ID
        /// </summary>
        public int PlatformID
        {
            get;
            set;
        }
        /// <summary>
        /// 渠道
        /// </summary>
        public string AreaName
        {
            get;
            set;
        }
        
        /// <summary>
        /// 总数
        /// </summary>
        public int TotalCount
        {
            get;
            set;
        }
        /// <summary>
        /// PC
        /// </summary>
        public int PCCount
        {
            get;
            set;
        }
        /// <summary>
        /// iOS
        /// </summary>
        public int IOSCount
        {
            get;
            set;
        }
        /// <summary>
        /// Android
        /// </summary>
        public int AndroidCount
        {
            get;
            set;
        }
       
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime StatTime
        {
            get;
            set;
        }       
    }
}
