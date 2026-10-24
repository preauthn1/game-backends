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
    public class PayGatewayConvertTypeMappingEntity
    {
        [Key]
        public int Id { get; set; }
        public int GWID { get; set; }
        public int ConvertType { get; set; }


    }
}
