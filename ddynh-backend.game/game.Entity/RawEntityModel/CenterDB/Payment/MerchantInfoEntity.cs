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
    public class MerchantInfoEntity
    {
        [Key]
        public int ID { get; set; }
        public long GWID { get; set; }
        public string InfoData { get; set; }
        public string Uuid { get; set; }
        public byte? MerchantStatus { get; set; }
        public byte Status { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public long? PlatformID { get; set; }

    }


}
