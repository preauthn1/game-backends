using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class LobbyTokenLogEntity
    {
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// (此欄位已經沒意義了) LobbyTokenTypeEnum
        /// </summary>
        public int Type { get; set; }

        public string IdentityHash { get; set; }


        /// <summary>
        /// 新版為了解決 queryString字數過長的問題 將token濃縮成一組guid
        /// </summary>
        public Guid? IdentityGuid { get; set; }
        public string RawJwtToken { get; set; }
        public string RawRequestInfo { get; set; }

        public int MerchantID { get; set; }
        public int PlayerID { get; set; }
        public DateTime ExpiredTime { get; set; }
        public DateTime CreateTime { get; set; }


    }
}
