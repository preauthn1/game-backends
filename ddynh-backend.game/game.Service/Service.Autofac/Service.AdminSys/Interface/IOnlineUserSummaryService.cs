using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.Users;
using game.Entity.Game;
using game.Entity.ViewModels;

namespace game.Service
{

    public interface IOnlineUserSummaryService
    {

        List<OnlineGameUserSummarySpResultModel> GetEachGameOnlineUsers(int adminUserID);


    }
}
