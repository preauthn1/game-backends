using FW.Core.Repository;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.RawEntity;
using game.Service.DAO;
using System.Collections.Generic;
using System.Linq;
using game.Infrastructure.Extensions;

namespace game.Service.Service
{
    public class AdminRightService : BaseService, IAdminRightService
    {
        public SystemAdminRightSettingRepository _systemAdminRightSettingRepository { get; set; }
        public UserRightRepository _userRightRepository { get; set; }
        public RoleRightRepository _roleRightRepository { get; set; }

        private IDatabaseFactory _dbFactory;


        public AdminRightService(IDatabaseFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }


   
        public List<SystemAdminRightSetting> GetSystemAdminRightFunctions()
        {
            var query = _systemAdminRightSettingRepository.GetAll();
            return query;
        }


        public List<UserRightEntity> GetUserRightByRightID(List<int> rightIDs)
        {
            if (rightIDs.IsEmptyOrNull())
                return new List<UserRightEntity>();

            var query = _userRightRepository.GetByRightID(rightIDs);
            return query;
        }


        public List<RoleRightEntity> GetRoleRightByRightID(List<int> rightIDs)
        {
            if (rightIDs.IsEmptyOrNull())
                return new List<RoleRightEntity>();

            var query = _roleRightRepository.GetByRightID(rightIDs);
            return query;
        }


    }
}
