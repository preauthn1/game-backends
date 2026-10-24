using target.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.Table
{
    public class ApiRequestLogQueueTempModel
    {
        public ApiRequestLogEntity ApiRequestLogEntity { get; set; }


        public ApiRequestLogQueueExtraInfoV2026Model ExtraInfoV2 { get; set; }

    }


    public class ApiRequestLogQueueExtraInfoV2026Model
    {
        public string ReferOrderNo { get; set; }

        public string UniqueID { get; set; }

        public int MerchantID { get; set; }

        public int ErrorCode { get; set; }

        public int PlayerID { get; set; }

        public ElasticLogTableTypeEnum TableType { get; set; }
    }





}
