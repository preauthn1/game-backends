using FW.Core.Repository;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.RawEntity;
using game.Service.DAO;
using System.Collections.Generic;
using System.Linq;
using game.Infrastructure.Extensions;
using game.Entity.Game;
using FW.Core.Helper;
using game.Entity.Money;

namespace game.Service.Service
{
    public class ServerInfoManagementService : BaseService, IServerInfoManagementService
    {
        public ServerPlatformRelationsRepository ServerPlatformRelationsRepository { get; set; }

        public GameServerListRepository GameServerListRepository { get; set; }


        public List<ServerPlatformRelationsEntity> GetAllServerPlatformRelations()
        {
            var query = ServerPlatformRelationsRepository.GetAll();
            return query;
        }

        public CRUDResultModel CreateOrUpdateStatus(UpdateServerRelationRequestModel model)
        {
            var queryExist = ServerPlatformRelationsRepository.GetByUniqueCondition(model.PlatformID , model.ServerID);

            if(queryExist == null)
            {
                var queryServer = GameServerListRepository.GetById(model.ServerID);

                ServerPlatformRelationsRepository.Create(new ServerPlatformRelationsEntity
                {
                    ServerID = queryServer.ServerID,
                    GameID = queryServer.GameID,
                    PlatformID = model.PlatformID,
                    Status = NormalActiveStatusEnum.Enable.ToInteger()
                });
            }
            else
            {
                queryExist.Status = model.TargetStatus;
                ServerPlatformRelationsRepository.Update(queryExist);
            }

            return new CRUDResultModel(true);
        }


    }
}
