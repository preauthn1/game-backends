using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.ViewModels
{

    public class GameLaunchViewModel 
    {

        public string RedirectUrl { get; set; }


        public long TransferAmount { get; set; }

    }


    public class GameLaunchRequestViewModel
    {

        public int UserID { get; set; }

        public string Certificate { get; set; }

        public int GameID { get; set; }

        public string IPAddress { get; set; }

        public bool IsTryPlayMode { get; set; }

        /// <summary>
        /// 若有指定此參數，會跳過一些檢查(測試使用)
        /// </summary>
        public bool IsSkipCheck { get; set; }

    }




}
