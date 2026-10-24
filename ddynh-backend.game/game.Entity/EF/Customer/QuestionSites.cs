using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Customer
{
    /// <summary>
    /// 分站
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class QuestionSites : BaseEntity
    {
        /// <summary>
        /// 分站名称
        /// </summary>
        public string SiteName { get; set; }
    }

}
