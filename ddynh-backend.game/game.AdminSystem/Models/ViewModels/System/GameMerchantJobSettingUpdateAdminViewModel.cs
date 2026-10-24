using game.Entity.Event;
using game.Entity.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using game.Entity.RawEntity;

namespace game.AdminSystem.Models.ViewModels
{

    public class GameMerchantJobSettingAdminUpdateViewModel : GameMerchantJobSettingRawEntity
    {

        public bool IsUpdatePage { get; set; }

    }



    /// <summary>
    /// 顯示資料用
    /// </summary>
    public class GameMerchantJobSettingDisplayViewModel : GameMerchantJobSettingRawEntity
    {

        public string PlatformText { get; set; }

        public string GameMerchantName { get; set; }

        public string IsEnableText { get; set; }

        public string JobTypeText { get; set; }

        public string LastExecutedTimeText { get; set; }

        public string JobNextStarttTimeText { get; set; }

        public string HintMsg { get; set; }


    }


    public class QueryGameMerchantJobSettingJQueryRequestModel : JQueryDataTablePagingModel
    {
        public int? Status { get; set; }

        public int? GameMerchantID { get; set; }

        public int? PlatformID { get; set; }


    }



}