using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.ViewModels
{



    public class BGGameMerchantApiQueryBetRecordRequestModel
    {

        public int PlatformID { get; set; }

        public DateTime BeginTimeUTC { get; set; }

        public DateTime EndTimeUTC { get; set; }
               
        public int PageIndex { get; set; }
        
        public int PageSize { get; set; }

    }



}
