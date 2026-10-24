using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using game.Entity.Area;
using game.Infrastructure;
using game.Entity.AdminSys;
using FW.Core.Helper;
using DataTables.Mvc;
using Newtonsoft.Json;
using game.Infrastructure.Extensions;
using game.Entity;
using game.Entity.Game;
using game.Service.DAO;
using game.AdminSystem.Models.ViewModels;
using game.Entity.Money;

namespace game.AdminSystem.Controllers.Game
{
    public class GameServerTreeManagementController : BaseController
    {
        
        public GameListRepository GameListRepository { get; set; }


        public GameServerListRepository GameServerListRepository { get; set; }


        public ServerPlatformRelationsRepository ServerPlatformRelationsRepository { get; set; }
        


        public ActionResult ListAndUpdate()
        {
            var queryMyPlatforms = DataService.GetMyPlatforms().OrderBy(x=> x.Id).ToList();

            var allGames = GameListRepository.GetAll();

            var allServers = GameServerListRepository.GetAll();

            var platformSettings = ServerPlatformRelationsRepository.GetAll();



            //查出所有platforms
            //查出所有games
            //查出所有gameServers

            var viewModel = new GameServerTreeManagementViewModel
            {
                Platforms = queryMyPlatforms,
                GameItems = allGames
                    .OrderBy(x=> x.GameID)
                    .Select(x=> new GameServerTreeManagementGameCategoryItemViewModel
                    {
                        GameID = x.GameID,
                        GameName = x.GameName,
                        Servers = allServers.Where(z=> z.GameID == x.GameID)
                            .OrderBy(z=> z.ServerID)
                            .Select(z=> new GameServerTreeManagementGameServerItemViewModel
                            {
                                ServerID = z.ServerID,
                                ServerName = z.ServerName,
                                MinMoney = z.Money,
                                AvailablePlatformIDs = platformSettings
                                    .Where(y=> y.ServerID == z.ServerID)
                                    .Where(y => y.Status == NormalActiveStatusEnum.Enable.ToInteger())
                                    .Select(y=> y.PlatformID)
                                    .ToList()
                            })
                            .ToList()
                    })
                    .ToList()
            };

            return View(viewModel);
        }

    }
}