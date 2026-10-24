using Newtonsoft.Json;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels.API.AdminSys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class SystemConfigSettingPageViewModel
    {
        public string SqlInsertScript { get; set; }

        public string SqlQueryScript { get; set; }

        public int OwnerID { get; set; }

        public string OwnerName { get; set; }

        public int NumberOfSubAgents { get; set; }


        public List<SystemConfigSettingGroupViewModel> Groups { get; set; }

        public List<SystemConfigSettingGroupViewModel> GroupCategoryList { get; set; }
    }



    public class SettingEnumStandardFormatConvertModel
    {
        public DataCategoryEnum Group { get; set; }

        public int SettingKey { get; set; }

        public System.Enum SettingKeyEnum { get; set; }

    }





    public class SystemConfigSettingGroupViewModel
    {

        public int GroupID { get; set; }


        public string GroupName { get; set; }


        public string KeywordForSearch { get; set; }


        public bool IsAllDisableEdit { get; set; }



        public List<SystemConfigSettingViewModel> Items { get; set; }
    }



    public class SystemConfigSettingUpdateRequestModel
    {

        /// <summary>
        /// 如果是商戶設置那邊使用 會帶入商戶ID
        /// </summary>
        public int MerchantID { get; set; }


        /// <summary>
        /// 泛用命名 (如果是 遊戲商配置: 會是帶 vendorConfigID)
        /// </summary>
        public int OwnerID { get; set; }



        public List<SystemConfigSettingUpdateRequestItemModel> Items { get; set; }
    }


    public class OneToManySettingEazyRequestModel
    {

        public int OneToManyCustomizedSettingID { get; set; }


        /// <summary>
        /// 填空值會自動填入流水序號
        /// </summary>
        public string SubCategory { get; set; }


        public string SettingValue { get; set; }

        public string Memo { get; set; }

    }




    public class SystemConfigSettingUpdateRequestItemModel
    {

        public int SettingKey { get; set; }

        public string Value { get; set; }

        /// <summary>
        /// 在特定的編輯頁面 會有string類型的key
        /// </summary>
        public string SettingKeyText { get; set; }
    }



    public class SystemConfigSettingViewModel
    {

        /// <summary>
        /// MerchantCustomizedSettingKeyEnum
        /// </summary>
        public int SettingKey { get; set; }

        public string SettingKeyDisplayName { get; set; }

        public string Value { get; set; }

        /// <summary>
        /// 另一種編輯頁會使用 string類型的 SettingKey
        /// </summary>
        public string SettingKeyText { get; set; }


        /// <summary>
        /// 基本上是給前端的一些判斷
        /// </summary>
        public string DataTypeName { get; set; }

        public string Memo { get; set; }

        public string KeywordForSearch { get; set; }

        public string Debug1 { get; set; }


        [JsonIgnore]
        public DefineTypeCheckResultModel DefineType { get; set; }

        /// <summary>
        /// 部分欄位僅允許透過DB修改數據 不開放普通人員編輯
        /// </summary>
        public bool IsDisableEdit { get; set; }


        public List<DropdownValueViewModel> DataDropdowns { get; set; }


        public bool IsDropdownDataType
        {
            get
            {
                return DataDropdowns != null && DataDropdowns.Count() > 0;
            }
        }


    }


}
