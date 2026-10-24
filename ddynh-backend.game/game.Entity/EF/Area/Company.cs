using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Area
{
    [Database(Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class Company : BaseEntity
    {
        /// <summary>
        ///  营运商名称
        /// </summary>
        public string Name { get; set; }
    }
}
