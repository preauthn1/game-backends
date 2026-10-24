using System.Collections.Generic;

namespace target.Models.ViewModels
{



    public class MerchantHierarchyTreeviewRequestModel
    {
        public int? RootMerchantID { get; set; }


        /// <summary>
        /// 根據樹狀圖裡面的某一位成員merchantID 來搜尋樹
        /// </summary>
        public int? SearchHierarchyMemberMerchantID { get; set; }


        /// <summary>
        /// 指定只返回某位代理的tree 而非整個組織的tree
        /// </summary>
        public bool IsShowSpecificTreeOnly { get; set; }
    }


    public class MerchantHierarchyCreateRequestModel
    {
        public int TargetMerchantID { get; set; }

        public int ParentMerchantID { get; set; }
    }


    public class MerchantHierarchyDeleteRequestModel
    {
        public int TargetMerchantID { get; set; }
    }



    public class MerchantHierarchyTreeviewViewModel
    {
        public int RootMerchantID { get; set; }

        public int HierarchyMaxLayerLimit { get; set; }

        public int TotalAgentCount { get; set; }

        public MerchantHierarchyTreeviewItemViewModel Root { get; set; }
    }

    public class MerchantHierarchyTreeviewItemViewModel
    {
        public int MerchantID { get; set; }

        public int ParentMerchantID { get; set; }

        public int Layer { get; set; }

        public string Name { get; set; }

        public string MerchantHierarchy { get; set; }

        public string CopyInfo { get; set; }


        public List<MerchantHierarchyTreeviewItemViewModel> Childs { get; set; }

    }

}
