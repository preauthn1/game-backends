using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Money;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Stat
{
    /// <summary>
    /// 金币流向统计
    /// </summary>
     [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class MoneySourceLog : BaseEntity
    {
         /// <summary>
         /// 流向ID
         /// </summary>
         public int SourceID
         {
             get;
             set;
         }

         /// <summary>
         /// 金额
         /// </summary>
         [MoneyInt]
         public Int64 Amount
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

         public virtual MoneySourceType SourceType
         {
             get;
             set;
         }
    }
}
