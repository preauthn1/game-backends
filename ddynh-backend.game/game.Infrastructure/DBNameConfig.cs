using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Infrastructure
{
    /// <summary>
    /// 数据库名
    /// </summary>
    public static class DBNameConfig
    {
        /// <summary>
        /// 系统库
        /// </summary>
        public const string DBName_AdminSys = "AdminSys";

        /// <summary>
        /// 中心库
        /// </summary>
        public const string DBName_CenterDB = "CenterDB";


        /// <summary>
        /// 代理
        /// </summary>
        public const string DBName_Club02 = "Club02";

        
        /// <summary>
        /// 网站数据库
        /// </summary>
        public const string DBName_WebSite = "WebSite";
                
        /// <summary>
        /// 每局游戏数据库
        /// </summary>
        public const string DBName_PerGameLog = "PerGameLog";
                
        /// <summary>
        /// 通知数据库
        /// </summary>
        public const string DBName_Notify = "Notify";

        /// <summary>
        /// 交易数据库
        /// </summary>
        public const string DBName_LogDB = "LogDB";
    }
}
