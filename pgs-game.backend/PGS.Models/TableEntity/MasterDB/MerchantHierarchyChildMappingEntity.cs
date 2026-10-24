using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    [IsNotAutoKeyTable]
    public class MerchantHierarchyChildMappingEntity
    {
        [Key]
        public int MerchantID { get; set; }

        [Key]
        public int ChildMerchantID { get; set; }
    }
}
