using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class QueryDropdownMerchantRequestViewModel
    {

        public int? OrganizationID { get; set; }

        public bool IsIncludeCurrency { get; set; }

        public bool IsIncludeCategory { get; set; }

        public string Keyword { get; set; }




        /// <summary>
        /// 需跟 IsIncludeCategory 搭配使用。複製常用商戶會排最前面
        /// </summary>
        public bool IsIncludeFavoritCopyMerchantCategory { get; set; }

        /// <summary>
        /// 若有指定。則特定的category會排最前面
        /// </summary>
        public int? CategoryFirstOrderID { get; set; }

        /// <summary>
        /// 排除3級以下的代理 (精簡下拉選單)
        /// </summary>
        public bool IsExcludeSpecificAgent { get; set; }

        /// <summary>
        /// 精簡下拉選單 (會排除非活躍商戶)
        /// </summary>
        public bool IsSimplifyItems { get; set; }

        /// <summary>
        /// 會包含額外欄位資訊
        /// </summary>
        public bool IsIncludeExtraInfo { get; set; }

    }


}
