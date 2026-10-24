using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class OneToManyCustomizedSettingViewModel
    {
        public int OneToManyCustomizedSettingID { get; set; }

        /// <summary>
        /// 請參考 OneToManyCustomizedSettingType
        /// </summary>
        public int Type { get; set; }
        public string TypeName { get; set; }
        public long? OwnerID { get; set; }
        public string OwnerIDMemo { get; set; }
        public string DisplayName { get; set; }
        public string Memo { get; set; }
        public DateTime CreateTime { get; set; }

    }


    public class OneToManyCustomizedSettingCreateRequestModel
    {
        public int Type { get; set; }

        public string DisplayName { get; set; }

        public string Memo { get; set; }

        /// <summary>
        /// 要預先產生的category(逗號隔開)
        /// </summary>
        public string DefaultSubCategory { get; set; }


        public long? OwnerID { get; set; }


        public string OwnerIDMemo { get; set; }

    }


    public class OneToManyCustomizedSettingUpdateRequestModel
    {

        public OneToManyCustomizedSettingEntity 請參考此檔案定義 { get; set; }

    }


}
