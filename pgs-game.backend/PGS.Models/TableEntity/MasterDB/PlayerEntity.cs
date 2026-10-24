using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.Table
{

    public class PlayerEntity : IPlayerEntity
    {
        [Key]
        public int PlayerID { get; set; }

        public int MerchantID { get; set; }

        public string PlayerName { get; set; }

        public int PlayerType { get; set; }

        public string Currency { get; set; }

        public int Status { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? LastLoginTime { get; set; }

        public int FaceID { get; set; }

        public int Sex { get; set; }


        //2025/09 移除的欄位 若確認無異常。之後可刪

        //public DateTime UpdateTime { get; set; }
        //public string LanguageCode { get; set; }

    }


    public class PlayerBriefModel : IPlayerEntity
    {
        [Key]
        public int PlayerID { get; set; }
        public int MerchantID { get; set; }
        public string PlayerName { get; set; }
        public string Currency { get; set; }
        public int Status { get; set; }
    }



    public interface IPlayerEntity
    {
        public int PlayerID { get; set; }
        public int MerchantID { get; set; }
        public string PlayerName { get; set; }
        public int Status { get; set; }
    }


}
