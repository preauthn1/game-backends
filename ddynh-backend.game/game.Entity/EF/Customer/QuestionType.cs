using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Customer
{
    /// <summary>
    /// 客户反映问题类型
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class QuestionType:BaseEntity
    {
        /// <summary>
        /// 问题类型名称
        /// </summary>
        public string TypeName
        {
            get;
            set;
        }
        /// <summary>
        /// 键值对
        /// </summary>
        public string ChildTypeFileds
        {
            get;
            set;
        } 
    }

   
}
