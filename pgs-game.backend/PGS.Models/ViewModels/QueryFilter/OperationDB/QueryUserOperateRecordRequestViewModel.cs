using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class QueryUserOperateRecordApiRequestParameterModel : QueryUserOperateRecordRequestViewModel
    {
        public int? OperateType { get; set; }

        public int? MerchantID { get; set; }

        public int? PlayerID { get; set; }

        public string PlayerName { get; set; }

    }





    public class QueryUserOperateRecordRequestViewModel : PaginationParameter
    {

        /// <summary>
        /// UserOperateTypeEnum
        /// </summary>
        [SwaggerIgnore]
        public List<int> OperateTypes { get; set; }

        [SwaggerIgnore]
        public List<int> MerchantIDs { get; set; }

        [SwaggerIgnore]
        public List<int> TargetIDs { get; set; }

        public int? TargetID { get; set; }

        public string TargetKey { get; set; }

        public int? UserID { get; set; }

        /// <summary>
        /// 透過代理樹狀結構去篩選MerchantIDs (跟直接用MerchantIDs去 in query速度有差距)
        /// </summary>
        public int? FilterMerchantIDByHierarchyMerchant { get; set; }


        public DateTime? CreateTimeBegin { get; set; }

        public DateTime? CreateTimeEnd { get; set; }

        public string CreateIPAddress { get; set; }

        public string CreateIPAddressLikeQuery { get; set; }

        public List<int> ReferenceCreditMerchantIDs { get; set; }

        public int? OrganizationID { get; set; }

        public string DeviceID { get; set; }

        public string Memo { get; set; }

        public string UserName { get; set; }


    }


}
