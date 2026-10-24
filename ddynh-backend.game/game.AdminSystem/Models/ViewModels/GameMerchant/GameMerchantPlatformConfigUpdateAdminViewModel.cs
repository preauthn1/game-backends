using game.Entity.Event;
using game.Entity.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace game.AdminSystem.Models.ViewModels
{

    public class GameMerchantPlatformConfigAdminUpdateViewModel : GameMerchantPlatformConfigRawEntity
    {

        public bool IsUpdatePage { get; set; }


        public Dictionary<string , string> ExtraSetting { get; set; }

    }



    /// <summary>
    /// 顯示資料用
    /// </summary>
    public class GameMerchantPlatformConfigDisplayViewModel : GameMerchantPlatformConfigRawEntity
    {
        public string PlatformName { get; set; }
        
        public string GameMerchantName { get; set; }

        public string UpdateTimeText { get; set; }

        public string CreateTimeText { get; set; }

    }


    public class QueryGameMerchantPlatformConfigJQueryRequestModel : JQueryDataTablePagingModel
    {
        public int? PlatformID { get; set; }

        public int? GameMerchantID { get; set; }
    }



}