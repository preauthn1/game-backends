using FW.Core.Helper;
using game.Entity;
using game.Entity.AdminSys;
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
    public class OrganizationController : BaseAPIController
    {
        private IOrganizationService _orgService;


        public OrganizationController(IOrganizationService service)
        {
            _orgService = service;
        }


        [HttpGet]
        public Organization GetByOrgById(int id)
        {
            return _orgService.GetOrganizationById(id);
        }


        [HttpGet]
        public List<Organization> GetByOrgByIds([FromUri]string ids)
        {
            List<int> IdValues = ConvertTextToListValue(ids);
            return _orgService.GetOrganizationByIds(IdValues);
        }


        [HttpPost]
        public CRUDResultModel CreateOrganization(Organization org)
        {
            int resultId = _orgService.CreateOrganization(org);
            return new CRUDResultModel
            {
                IsSuccess = true,
                ResultId = resultId
            };
        }


        [HttpPost , HttpPut]
        public APIResult UpdateOrganization(OrganizationrUpdateRequestViewModel org)
        {
            var result = _orgService.UpdateOrganization(org);
            return result;
        }


        [HttpGet]
        public List<OrganizationPlatformMapping> GetOrganizationPlatformMappingByOrgId(int orgId)
        {
            return _orgService.OrganizationPlatformMappingByOrgIds(orgId);
        }



        [HttpGet]
        public List<EachOrganizationPlatformCountSummaryViewModel> GetEachOrganizationCountSummaryInfo()
        {
            var result = _orgService.GetEachOrganizationCountSummaryInfo();
            return result;
        }


        [HttpGet]
        public List<EachPlatformOrganizationCountViewModel> GetEachPlatformOrganizationCount()
        {
            var result = _orgService.GetEachPlatformOrganizationCount();
            return result;
        }
               

        [HttpPost , HttpPut]
        public void UpdatePlatformMapping(UpdateOrganizationPlatformRequestViewModel model)
        {
            _orgService.UpdateOrganizationPlatformMapping(model);
            return;
        }

    }
}