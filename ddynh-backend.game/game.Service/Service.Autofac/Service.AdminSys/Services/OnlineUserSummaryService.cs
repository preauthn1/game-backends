using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Game;
using game.Service.DAO;
using game.Infrastructure;
using game.Entity.ViewModels;
using game.Infrastructure.Extensions;
using game.Infrastructure.Helper;

namespace game.Service.Service
{
    public class OnlineUserSummaryService : BaseService, IOnlineUserSummaryService
    {

        public GameUserListRepository GameUserListRepository { get; set; }


        public List<OnlineGameUserSummarySpResultModel> GetEachGameOnlineUsers(int adminUserID)
        {
            var query = GameUserListRepository.GetEachGameOnlineUserCount(adminUserID);
            return query;
        }

    }

}
