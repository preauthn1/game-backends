using game.Entity.Event;
using game.Entity.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace game.AdminSystem.Models.ViewModels
{

    public class GameMerchantAdminUpdateViewModel : GameMerchantRawEntity
    {

        public bool IsUpdatePage { get; set; }

    }



    /// <summary>
    /// 顯示資料用
    /// </summary>
    public class GameMerchantDisplayViewModel : GameMerchantRawEntity
    {
        public string StatusText { get; set; }

        public string CreateTimeText { get; set; }

        public string UpdateTimeText { get; set; }

    }


    public class QueryGameMerchantJQueryRequestModel : JQueryDataTablePagingModel
    {
        public int? GameMerchantID { get; set; }

    }



}