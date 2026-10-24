using game.Entity.Event;
using game.Entity.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace game.AdminSystem.Models.ViewModels
{

    public class GameListAdminUpdateViewModel : GameListRawEntity
    {

        public bool IsUpdatePage { get; set; }

    }



    /// <summary>
    /// 顯示資料用
    /// </summary>
    public class GameListDisplayViewModel : GameListRawEntity
    {
        public string StatusText { get; set; }

        public string GameMerchantText { get; set; }

        public string GameMerchantWalletText { get; set; }

        public int GameServerCount { get; set; }

        public int AliasNameCount { get; set; }


    }


    public class QueryGameListJQueryRequestModel : JQueryDataTablePagingModel
    {
        public int? GameMerchantID { get; set; }


    }



}