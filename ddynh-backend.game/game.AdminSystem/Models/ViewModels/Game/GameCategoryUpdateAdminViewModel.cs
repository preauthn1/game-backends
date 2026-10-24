using game.Entity.Event;
using game.Entity.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace game.AdminSystem.Models.ViewModels
{
    public class GameCategoryAdminCreateViewModel
    {
        public string Name { get; set; }
    }



    public class GameCategoryAdminUpdateViewModel 
    {

        public List<GameCategoryEditItemViewModel> EachCategory { get; set; }

    }

    public class GameCategoryEditItemViewModel : GameCategoryEntity
    {

        public bool IsActive { get; set; }


        public List<GameCategoryGameSelectedViewModel> EacgGames { get; set; }

    }

    public class GameCategoryGameSelectedViewModel : GameCategoryEntity
    {
        public int ParentCategoryID { get; set; }

        public int GameID { get; set; }

        public string GameName { get; set; }

        public bool IsSelected { get; set; }

    }




    /// <summary>
    /// 顯示資料用
    /// </summary>
    public class GameCategoryDisplayViewModel : GameCategoryEntity
    {
        public string PlatformName { get; set; }

    }


    public class QueryGameCategoryJQueryRequestModel : JQueryDataTablePagingModel
    {
        public int? PlatformID { get; set; }


        public string PlatformIDsText { get; set; }

    }



}