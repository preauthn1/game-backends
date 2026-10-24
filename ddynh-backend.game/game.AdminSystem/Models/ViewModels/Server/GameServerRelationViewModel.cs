using game.Entity.Area;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace game.Entity.Game
{
   
    public class GameServerRelationViewModel
    {
        public int ServerID { get; set; }

        public string ServerName { get; set; }

        public string ShowName { get; set; }

    }




    public class GameServerRelationSettingItemViewModel : Platform
    {
        public bool IsActive { get; set; }

    }







}