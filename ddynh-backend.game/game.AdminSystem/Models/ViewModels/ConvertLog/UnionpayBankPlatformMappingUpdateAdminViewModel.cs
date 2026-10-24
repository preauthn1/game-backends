using game.Entity.Area;
using game.Entity.Event;
using game.Entity.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace game.AdminSystem.Models.ViewModels
{

    public class UnionpayBankPlatformMappingAdminUpdateViewModel
    {
        public bool IsMultiplePlatforms { get; set; }

        public List<Platform> MyPlatforms { get; set; }

        public int PlatformID { get; set; }

        public string PlatformName { get; set; }

        public List<UnionpayBankPlatformMappingSelectedAdminUpdateViewModel> SettingItems { get; set; }

    }

    public class UnionpayBankPlatformMappingSelectedAdminUpdateViewModel
    {
        public int UnionpayBankCodeID { get; set; }

        public string UnionpayBankCodeDOMElementID { get; set; }

        public string UnionpayBankName { get; set; }

        public bool IsSelected { get; set; }
    }



    /// <summary>
    /// 顯示資料用
    /// </summary>
    public class UnionpayBankPlatformMappingDisplayViewModel : UnionpayBankPlatformMappingEntity
    {
        public string PlatformName { get; set; }

    }


    public class QueryUnionpayBankPlatformMappingJQueryRequestModel : JQueryDataTablePagingModel
    {
        public int? PlatformID { get; set; }


        public string PlatformIDsText { get; set; }

    }



}