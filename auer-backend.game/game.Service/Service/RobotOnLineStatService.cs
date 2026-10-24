using game.Entity.Stat;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace game.Service
{
    public class RobotOnLineStatService : IRobotOnLineStatService
    {
        private IRepositoryFactory _repoFactory;
        private IDatabaseFactory _dbFactory;
        public RobotOnLineStatService(IRepositoryFactory repoFactory, IDatabaseFactory dbFactory)
        {
            _repoFactory = repoFactory;
            _dbFactory = dbFactory;
        }


        public List<ServerOnLineRobotLog> GetServerOnLineRobotLog(int? gameID, int? serverID, DateTime? beginTime, DateTime? endTime)
        {
            return _repoFactory.CreateRepository<ServerOnLineRobotLog>()
                .Entities
                .Where(m => (gameID == null || m.GameServer.GameID == gameID) &&
                            (serverID == null || m.ServerID == serverID) &&
                            (beginTime == null || m.StatTime >= beginTime) &&
                            (endTime == null || m.StatTime <= endTime)
                )
                .OrderByDescending(m => m.StatTime)
                .ToList();
        }

        public List<GameOnLineRobotLog> GetGameOnLineRobotLog(int? gameID, DateTime? beginTime, DateTime? endTime)
        {
            return _repoFactory.CreateRepository<GameOnLineRobotLog>()
                .Entities
                .Where(m => (gameID == null || m.GameID == gameID) &&
                            (beginTime == null || m.StatTime >= beginTime) &&
                            (endTime == null || m.StatTime <= endTime)
                )
                .OrderByDescending(m => m.StatTime)
                .ToList();
        }

        public List<TotalGameOnLineRobotLog> GetTotalGameOnLineRobotLog(DateTime? beginTime, DateTime? endTime)
        {
            return _repoFactory.CreateRepository<TotalGameOnLineRobotLog>()
                .Entities
                .Where(m => (beginTime == null || m.StatTime >= beginTime) &&
                            (endTime == null || m.StatTime <= endTime))
                .OrderByDescending(m => m.StatTime)
                .ToList();
        }
    }
}
