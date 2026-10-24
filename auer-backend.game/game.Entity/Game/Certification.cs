using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class Certification:BaseEntity
    {

        [StringLength(32)]
        public string CerCode { get; set; }
    }
}
