using target.Models.Enums;
using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryOneToManyCustomizedSettingItemRequestViewModel : PaginationParameter
    {

        public List<int> IDs { get; set; }

        /// <summary>
        /// 主表ID
        /// </summary>
        public int? OneToManyCustomizedSettingID { get; set; }

        public List<int> SettingKeys { get; set; }

        /// <summary>
        /// 比對數值相符
        /// </summary>
        public string SubCategory { get; set; }


        public string Keyword { get; set; }


        public int? TagID { get; set; }

        /// <summary>
        /// 未被標記
        /// </summary>
        public int? NotAssignTagID { get; set; }


        /// <summary>
        /// 這個是主表的type 對應 OneToManyCustomizedSettingTypeEnum
        /// </summary>
        public int? OneToManyCustomizedSettingType { get; set; }


        /// <summary>
        /// 有分配正式商戶
        /// </summary>
        public bool IsAssignProductionMerchantOnly { get; set; }



        /// <summary>
        /// Memo2 會存TG chat_ID
        /// </summary>
        public string Memo2Match { get; set; }



        public UniqueParentCustomizedSettingQuery UniqueOnwerQuery { get; set; }

    }


    public class UniqueParentCustomizedSettingQuery
    {
        public int OwnerID { get; set; }

        public OneToManyCustomizedSettingTypeEnum Type { get; set; }

    }



}
