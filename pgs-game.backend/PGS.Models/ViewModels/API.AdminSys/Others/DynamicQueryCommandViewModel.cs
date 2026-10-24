using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class DynamicQueryCommandViewModel
    {

        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string Memo { get; set; }
        public string FilterFormItems { get; set; }
        public int DefaultPageSize { get; set; }


        public CustomizedColumnDynamicQueryExtraSettingJson ExtraSetting { get; set; }

        public List<string> AvailableFilterFormItems { get; set; }

    }

    public class DynamicQueryCommandDropdownItemViewModel
    {
        public List<DynamicQueryCommandCategoryItemViewModel> EachCategory { get; set; }        
    }


    public class DynamicQueryCommandCategoryItemViewModel
    {
        public string Category { get; set; }

        public List<DynamicQueryCommandItemViewModel> Items { get; set; }        
    }


    public class DynamicQueryCommandItemViewModel
    {

        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string Memo { get; set; }
        public string FilterFormItems { get; set; }
        public List<string> AvailableFilterFormItems { get; set; }

    }



}
