using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryMerchantHierarchyRequestViewModel : PaginationParameter
    {
        public List<int> MerchantIDs { get; set; }

        public int? HierarchyOfTargerMerchant { get; set; }

        public List<int> FilterMerchantIDsByMultipleHierarchyMerchant { get; set; }

        public int? ExcludeMerchantID { get; set; }

        public bool ExcludeDeleteMerchants { get; set; }

        public int? ParentMerchantID { get; set; }

        public int? RootMerchantID { get; set; }




        /// <summary>
        /// 指定的代理層級min。從1開始 判斷>= 
        /// </summary>
        public int? MinLayer { get; set; }

        /// <summary>
        /// 指定的代理層級max。含指定的數字 (e.g. 若指定3  會查出包含三級代理) 判斷<= 
        /// </summary>
        public int? MaxLayer { get; set; }

    }


}
