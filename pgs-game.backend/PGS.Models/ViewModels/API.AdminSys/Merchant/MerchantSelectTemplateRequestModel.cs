using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class MerchantSelectTemplateRequestModel
    {

        public int TemplateID { get; set; }


        /// <summary>
        /// DataCategoryEnum
        /// </summary>
        public int CategoryID { get; set; }


        /// <summary>
        /// MerchantSelectValueTypeEnum
        /// </summary>
        public int ValueType { get; set; }

        public string DisplayName { get; set; }

        public List<int> IncludeMerchantIDs { get; set; }

        public int? ParentMerchantID { get; set; }

        public List<int> ExcludeMerchantIDs { get; set; }

    }

    public class MerchantSelectTemplateViewModel
    {

        public int TemplateID { get; set; }

        /// <summary>
        /// DataCategoryEnum
        /// </summary>
        public int CategoryID { get; set; }


        /// <summary>
        /// MerchantSelectValueTypeEnum
        /// </summary>
        public int ValueType { get; set; }

        public string DisplayName { get; set; }

        public List<int> IncludeMerchantIDs { get; set; }

        public int ParentMerchantID { get; set; }

        public List<int> ExcludeMerchantIDs { get; set; }

        public List<int> AlwaysAddMerchantIDs { get; set; }


        


    }

}
