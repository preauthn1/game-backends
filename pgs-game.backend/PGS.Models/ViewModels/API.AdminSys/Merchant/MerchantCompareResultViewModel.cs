using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class MerchantCompareResultViewModel
    {
        public MerchantCompareBasicInfoViewModel LeftMerchant { get; set; }

        public MerchantCompareBasicInfoViewModel RightMerchant { get; set; }


        public List<MerchantCompareItemViewModel> CompareColumns { get; set; }

        public List<MerchantCompareItemViewModel> AdvanceSettings { get; set; }

    }



    public class MerchantNameExistCheckResult
    {
        public int MerchantID { get; set; }

        public bool IsExist { get; set; }
    }



    public class MerchantCompareBasicInfoViewModel
    {

        public int MerchantID { get; set; }

        public string Name { get; set; }

    }


    public class MerchantCompareItemViewModel
    {
        public string DisplayName { get; set; }

        public bool IsEqual { get; set; }

        public bool IsSkipCompare { get; set; }

        public bool IsAllowCopy { get; set; }


        public object LeftValue { get; set; }

        public object RightValue { get; set; }
    }




}
