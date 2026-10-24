using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace game.Entity.Game
{

    public class GameMerchantPlatformConfigRawEntity
    {
        [Key]
        public int ID { get; set; }
        public int GameMerchantID { get; set; }
        public int PlatformID { get; set; }
        public string MerchantApiHost { get; set; }
        public string MerchantIdentity { get; set; }
        public string MerchantSecretPublic { get; set; }
        public string MerchantSecretPrivate { get; set; }
        public string AdminWebSiteUrl { get; set; }
        public int Status { get; set; }
        public string ConfigJsonValues { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string UpdateBy { get; set; }


    }
}
