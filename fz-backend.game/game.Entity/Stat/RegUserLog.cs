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
    /// 注册用户统计
    /// </summary>
     [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class RegUserLog : BaseEntity
    {
         /// <summary>
         /// 总注册人数
         /// </summary>
         public int TotalRegCount
         {
             get;
             set;
         }

         /// <summary>
         /// IOS注册人数
         /// </summary>
         public int IOSRegCount
         {
             get;
             set;
         }

         /// <summary>
         /// Android注册人数
         /// </summary>
         public int AndroidRegCount
         {
             get;
             set;
         }

         /// <summary>
         /// PC注册人数
         /// </summary>
         public int PCRegCount
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
