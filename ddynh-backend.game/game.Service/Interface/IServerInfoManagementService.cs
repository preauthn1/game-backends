using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity;
using game.Entity.Game;
using game.Entity.Users;

namespace game.Service
{
  
    public interface IServerInfoManagementService
    {

        List<ServerPlatformRelationsEntity> GetAllServerPlatformRelations();

        CRUDResultModel CreateOrUpdateStatus(UpdateServerRelationRequestModel model);

    }
}
