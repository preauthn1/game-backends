using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    /// <summary>
    /// 記住我方傳給三方遊戲的合法sessionnToken
    /// </summary>
    public class VendorSessionTokenEntity
    {
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// 這邊存的是PGS自訂的token
        /// </summary>
        public string SessionToken { get; set; }
        public int VendorID { get; set; }
        public int GameID { get; set; }
        public int PlayerID { get; set; }

        /// <summary>
        /// 會對應 GameLaunchExtraSettingModel
        /// </summary>
        public string ExtraSettingJson { get; set; }

        public DateTime ExpiredTime { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string IPAddress { get; set; }


    }


    public class VendorSessionBriefModel
    {
        public int VendorID { get; set; }
        public int GameID { get; set; }
        public int PlayerID { get; set; }
        public DateTime ExpiredTime { get; set; }
    }




}
