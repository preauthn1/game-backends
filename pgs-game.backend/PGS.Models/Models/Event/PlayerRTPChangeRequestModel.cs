using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class PlayerRTPChangeRequestModel
    {

        public int PlayerID { get; set; }

        public int GameID { get; set; }

        public decimal RTP { get; set; }

        /// <summary>
        /// 操作者
        /// </summary>
        public string Operator { get; set; }

        public string DebugReferenceUUID { get; set; }

    }



    public class PlayerRTPResetRequestModel
    {

        public int PlayerID { get; set; }


        public int GameID { get; set; }

        /// <summary>
        /// 操作者
        /// </summary>
        public string Operator { get; set; }


        public string DebugReferenceUUID { get; set; }


        public string Memo { get; set; }

    }



    public class PlayerRTPResetAllGameRequestModel
    {

        public int PlayerID { get; set; }


        public List<int> GameIDs { get; set; }

        /// <summary>
        /// 正常情境應該不會想要刪除風控調整的RTP
        /// </summary>
        public bool IsDeleteRiskControl { get; set; }


        public string DebugReferenceUUID { get; set; }

    }


    public class PlayerRTPChangeWhenEnterGameParameter
    {

        public int MerchantID { get; set; }


        public int PlayerID { get; set; }


        public int GameID { get; set; }


    }

    public class PlayerRTPChangeResultModel
    {

        public decimal NewRTP { get; set; }

    }



}
