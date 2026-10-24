using target.Models.ViewModels.API.AdminSys;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class CommonCustomizedSettingBatchEditModel
    {
        public int OwnerID { get; set; }

        public string OwnerName { get; set; }

        public List<CustomizedSettingItemEditModel> Items { get; set; }
    }

    public class CustomizedSettingItemEditModel
    {
        public int OnwerID { get; set; }

        public int SettingKey { get; set; }

        public string SettingKeyText { get; set; }

        public string Value { get; set; }

        /// <summary>
        /// 如果是多選
        /// </summary>
        public List<string> MultipleValues { get; set; }

        public List<DropdownValueViewModel> DataDropdowns { get; set; }
    }



}
