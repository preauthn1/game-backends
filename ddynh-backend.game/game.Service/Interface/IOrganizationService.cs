using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.Users;
using game.Infrastructure;

namespace game.Service
{
    
    public interface IOrganizationService
    {
        Organization GetOrganizationById(int orgId);

        List<Organization> GetOrganizationByIds(List<int> ids);

        List<OrganizationPlatformMapping> OrganizationPlatformMappingByOrgIds(int orgId);

        List<EachOrganizationPlatformCountSummaryViewModel> GetEachOrganizationCountSummaryInfo();

        List<EachPlatformOrganizationCountViewModel> GetEachPlatformOrganizationCount();

        int CreateOrganization(Organization org);

        APIResult UpdateOrganization(OrganizationrUpdateRequestViewModel updateModel);


        void UpdateOrganizationPlatformMapping(UpdateOrganizationPlatformRequestViewModel model);
    }
}
