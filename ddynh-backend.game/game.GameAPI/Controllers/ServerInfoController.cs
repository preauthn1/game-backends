using FW.Core.Helper;
using game.Entity;
using game.Entity.Game;
using game.Entity.Users;
using game.Infrastructure;
using game.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace game.GameAPI.Controllers
{
    public class ServerInfoController : BaseAPIController
    {
        private IServerInfoManagementService _serverInfoManagementService;

        public ServerInfoController(IServerInfoManagementService service)
        {
            _serverInfoManagementService = service;
        }

        public List<ServerPlatformRelationsEntity> GetAllServerPlatformRelations()
        {
            var queryAll = _serverInfoManagementService.GetAllServerPlatformRelations();
            return queryAll;
        }


        [HttpPost , HttpPut]
        public CRUDResultModel CreateOrUpdateStatus(UpdateServerRelationRequestModel model)
        {
            var result = _serverInfoManagementService.CreateOrUpdateStatus(model);
            return result;
        }


    }
}