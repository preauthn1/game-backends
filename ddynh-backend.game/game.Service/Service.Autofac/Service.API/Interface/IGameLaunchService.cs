using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.Users;
using game.Entity.Game;
using game.Entity.ViewModels;

namespace game.Service
{

    public interface IGameLaunchService
    {
        object TestCaseTestCase(GameLaunchRequestViewModel model);

        GameLaunchViewModel GameLaunch(GameLaunchRequestViewModel model);


    }
}
