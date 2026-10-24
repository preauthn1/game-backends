using System;
using System.Collections.Generic;
using System.Web.Mvc;
using game.Entity.AdminSys;
using game.Entity.Money;
using game.Infrastructure.Extensions;
using game.Entity.Stat;
using game.Entity.Area;
using System.Linq;
using game.Entity;
using FW.Core.Helper;
using game.Infrastructure;

namespace game.AdminSystem.Controllers.SystemManagement
{
    public class OrganizationController : BaseController
    {
               

        public ActionResult List()
        {
            string orgIdText = GetCurrentAdminUserOrgIdOrEmptyText();

            var query = SiteHelper.GetAPIResult<List<Organization>>($"Organization/GetByOrgByIds?ids={orgIdText}");
            var queryEachCount = SiteHelper.GetAPIResult<List<EachOrganizationPlatformCountSummaryViewModel>>($"Organization/GetEachOrganizationCountSummaryInfo");
            query = query.OrderByDescending(x => x.Id).ToList();


            ViewBag.EachCount = queryEachCount ?? new List<EachOrganizationPlatformCountSummaryViewModel>();
            return View(query);
        }

        [HttpGet]
        public ActionResult Create()
        {
            List<Role> queryTemplateRoles = SiteHelper.GetAPIResult<List<Role>>($"AdminRole/GetRolesByFilter?IsFilterTemplateOnly=true");
            ViewBag.TemplateRoles = queryTemplateRoles;
            return View();
        }


        [HttpGet]
        public ActionResult Edit(int orgId)
        {   
            //檢查是否查到別組織的資料
            CheckIsOwnPermissionForOrg(orgId);

            var queryOrg = SiteHelper.GetAPIResult<Organization>($"Organization/GetByOrgById?id={orgId}");
            return View(queryOrg);
        }


        [HttpGet]
        public ActionResult ChangePlatform(int orgId)
        {
            //檢查是否查到別組織的資料
            CheckIsOwnPermissionForOrg(orgId);

            var queryOrg = SiteHelper.GetAPIResult<Organization>($"Organization/GetByOrgById?id={orgId}");
            var queryPlatforms = SiteHelper.GetAPIResult<List<Platform>>("Area/GetPlatform");
            var queryMappingSetting = SiteHelper.GetAPIResult<List<OrganizationPlatformMapping>>($"Organization/GetOrganizationPlatformMappingByOrgId?orgId={orgId}");
            var queryPlatformMappingOrgCount = SiteHelper.GetAPIResult<List<EachPlatformOrganizationCountViewModel>>($"Organization/GetEachPlatformOrganizationCount");

            if (queryOrg == null)
                throw new Exception("查无资料");

            var viewModel = new OrganizationPlatformMappingViewModel
            {
                OrgId = orgId,
                OrgName = queryOrg.Name,
                SelectedPlatformIds = queryMappingSetting.Select(x=> x.PlatformID).ToList(),
                Platforms = queryPlatforms.Select(x=> new PlatformChangeViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    MappingOrgCount = queryPlatformMappingOrgCount?.FirstOrDefault(z=> z.PlatformID == x.Id)?.TotalOrgCount ?? 0
                })
                .ToList()
            };

            return View(viewModel);
        }


        [HttpPost]
        public ActionResult Create(CreateOrganizationrRequestViewModel model)
        {
            model.CreateTime = DateTime.Now;           

            //Step 1. 建立org
            var result = SiteHelper.PostGameApi<CRUDResultModel>($"Organization/CreateOrganization" , model);
            
            if(result.Exception != null || !result.ResultObject.IsSuccess)
            {
                return Alert("资料建立失败，请稍后尝试");
            }

            //Step 2. 勾選建立預設角色
            if(model.IsGenerateTemplateCheck)
            {
                var postBody = new
                {
                    OrgId = result.ResultObject.ResultId,
                    PrefixText = model.ShortName,
                };

                var resultStep2 = SiteHelper.PostGameApi<string>($"AdminRole/CreateRoleAndRightByTemplateSetting", postBody);

                if (result.Exception != null || !result.ResultObject.IsSuccess)
                {
                    return Alert("范本资料建立失败，请稍后尝试");
                }
            }

            return ConfirmReturnList("List");
        }

        [HttpPost]
        public ActionResult Edit(CreateOrganizationrRequestViewModel model)
        {
            //檢查是否查到別組織的資料
            CheckIsOwnPermissionForOrg(model.OrgID);

            var resultStep = SiteHelper.PutGameApi<APIResult>($"Organization/UpdateOrganization", model);

            if (resultStep.Exception != null)
                return Alert("储存失败");

            return Alert(resultStep.ResultObject.Message);
        }



        [HttpPost]
        public ActionResult ChangePlatform(int orgId, string[] ck)
        {
            //檢查是否查到別組織的資料
            CheckIsOwnPermissionForOrg(orgId);

            var postData = new
            {
                OrgId = orgId,
                PlatformIds = ck?.Select(x => Int32.Parse(x)).ToList()
            };

            var result = SiteHelper.PutGameApi<string>($"Organization/UpdatePlatformMapping", postData);

            if (result.Exception != null)
                return Alert("储存失败");

            return ConfirmReturnList("List"); ;
        }




    }
}