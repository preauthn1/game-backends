using FW.Core.Repository;
using game.Entity;
using game.Entity.AdminSys;
using game.Service.DAO;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace game.Service.Service
{
    public class AdminRoleService : IAdminRoleService
    {
        public RoleRepository _roleRepository;

        private IDatabaseFactory _dbFactory;


        public AdminRoleService(IDatabaseFactory dbFactory)
        {
            _dbFactory = dbFactory;
            _roleRepository = new RoleRepository();
        }


        public List<Role> GetRolesByFilter(QueryRoleRequestViewModel filter)
        {
            var query = _roleRepository.GetByFilter(filter);
            return query;
        }

        public List<TargetRolesAssinRightCountModel> GetTargetRolesAssignRightCount(int orgId)
        {
            var query = _roleRepository.GetTargetRolesAssignRightCount(orgId);
            return query.ToList();
        }
        

        public void CreateRoleAndRightByTemplateSetting(AutoGenerateRoleAndRightRequestViewModel model)
        {
            if (model.OrgId <= 0)
                throw new InvalidParameterException("OrgId");

            if (string.IsNullOrWhiteSpace(model.PrefixText))
                throw new InvalidParameterException("PrefixText");

            _roleRepository.CreateRoleAndRightByTemplateSetting(model.OrgId, model.PrefixText);
            return;
        }

    }
}
