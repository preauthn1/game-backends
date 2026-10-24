using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Stat
{
    /// <summary>
    /// 登录用户统计
    /// </summary>
     [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class LogonUserLog : BaseEntity
    {
         /// <summary>
         /// 总登录人数
         /// </summary>
         public int TotalLogonCount
         {
             get;
             set;
         }

         /// <summary>
         /// PC登录人数
         /// </summary>
         public int PCLogonCount
         {
             get;
             set;
         }

         /// <summary>
         /// IOS登录人数
         /// </summary>
         public int IOSLogonCount
         {
             get;
             set;
         }

         /// <summary>
         /// Android登录人数
         /// </summary>
         public int AndroidLogonCount
         {
             get;
             set;
         }

         /// <summary>
         /// 平台ID
         /// </summary>
         public int PlatformID
         {
             get;
             set;
         }

         /// <summary>
         /// 统计时间
         /// </summary>
         public DateTime StatTime
         {
             get;
             set;
         }
    }
}
