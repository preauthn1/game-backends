using game.Entity.Area;
using game.Entity.Event;
using game.Entity.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace game.AdminSystem.Models.ViewModels
{
    public class GameServerTreeManagementViewModel
    {

        public List<Platform> Platforms { get; set; }


        public List<GameServerTreeManagementGameCategoryItemViewModel> GameItems { get; set; }

    }


    public class GameServerTreeManagementGameCategoryItemViewModel
    {
        public int GameID { get; set; }

        public string GameName { get; set; }
        public List<GameServerTreeManagementGameServerItemViewModel> Servers { get; set; }

    }




    public class GameServerTreeManagementGameServerItemViewModel
    {
        public int ServerID { get; set; }

        public string ServerName { get; set; }

        public long MinMoney { get; set; }

        public List<int> AvailablePlatformIDs { get; set; }

    }



}