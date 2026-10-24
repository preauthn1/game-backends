using game.Entity.Event;
using game.Entity.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using game.Entity.RawEntity;

namespace game.AdminSystem.Models.ViewModels
{

    public class SlotGameHistoryLogAdminUpdateViewModel : SlotGameHistoryLogRawEntity
    {

        public bool IsUpdatePage { get; set; }

    }



    /// <summary>
    /// 顯示資料用
    /// </summary>
    public class SlotGameHistoryLogDisplayViewModel : SlotGameHistoryLogRawEntity
    {

        public string CreateTimeText { get; set; }

        public string PlatformName { get; set; }
        
        public string GameName { get; set; }

        public string ServerName { get; set; }
        
        public string GameTypeText { get; set; }

        public string LogTypeText { get; set; }



        public string GameDataConvert { get; set; }


    }


    public class QuerySlotGameHistoryLogJQueryRequestModel : JQueryDataTablePagingModel
    {


        public int? GameID { get; set; }


        public int? UserID { get; set; }


        public int? PlatformID { get; set; }


        public int? GameType { get; set; }


        public DateTime? CreateBeginTime { get; set; }


        public DateTime? CreateEndTime { get; set; }



    }



}