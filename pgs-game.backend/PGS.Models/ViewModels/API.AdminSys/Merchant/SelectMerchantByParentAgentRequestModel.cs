using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class OneToManySelectMerchantByParentAgentParameter
    {
        public int OwnerID { get; set; }

        public SelectMerchantByParentAgentRequestModel MerchantSetting { get; set; }
    }


    public class OneToManySelectMerchantByParentAgentResultModel
    {
        public int OneToManySettingID { get; set; }

        public int ParentMerchantID { get; set; }

        /// <summary>
        /// 這條樹所有商戶ID
        /// </summary>
        public List<int> AllHierarchyMerchantIDs { get; set; }

        public List<int> ExcludeMerchantIDs { get; set; }

        public List<int> AvailableMerchantIDs { get; set; }
    }



    public class MerchantSelectByParentAvailableResultModel
    {

        public int ParentMerchantID { get; set; }

        /// <summary>
        /// 這條樹所有商戶ID
        /// </summary>
        public List<int> AllHierarchyMerchantIDs { get; set; }

        public List<int> ExcludeMerchantIDs { get; set; }

        public List<int> AvailableMerchantIDs { get; set; }
    }



    public class UpdateEventMerchantAssignByAgentParameter
    {
        public int EventID { get; set; }


        public int OneToManySettingID { get; set; }

        /// <summary>
        /// 若陣列有值、會判斷是否有影響到指定的這些商戶。若沒有、則不進行DB更新儲存
        /// </summary>
        public List<int> AffectMerchantIDs { get; set; }


    }


    public class SelectMerchantByParentAgentRequestModel
    {
        /// <summary>
        /// 清空還原
        /// </summary>
        public bool IsResetToInitial { get; set; }

        public int MerchantID { get; set; }

        public List<int> ExcludeMerchantIDs { get; set; }
    }

}
