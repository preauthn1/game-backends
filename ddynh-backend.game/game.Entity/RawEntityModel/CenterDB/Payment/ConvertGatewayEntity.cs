using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace game.Entity.Game
{

    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class ConvertGatewayEntity
    {
        [Key]
        public int ID { get; set; }
        public string GatewayName { get; set; }
        public byte ConvertType { get; set; }
        public byte Status { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdareTime { get; set; }
        public string UpdateBy { get; set; }

    }
}
