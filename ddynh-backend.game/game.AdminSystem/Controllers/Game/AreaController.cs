using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using game.Entity.Area;
using game.Infrastructure;
using game.Entity.AdminSys;
using FW.Core.Helper;
using DataTables.Mvc;
using Newtonsoft.Json;

namespace game.AdminSystem.Controllers.Game
{
    public class AreaController : BaseController
    {
        private FW.Core.AdminSys.Provider.IAdminSysProvider _adminSysProvider;
        public AreaController(FW.Core.AdminSys.Provider.IAdminSysProvider adminSysProvider)
        {
            _adminSysProvider = adminSysProvider;
        }

        public ActionResult List(int? platformId, int? areaID)
        {
            var list = SiteHelper.GetAPIResult<List<Area>>(string.Format("/Area/GetArea?platformId={0}&areaID={1}", platformId, areaID));
            return View(list);
        }

        public ActionResult Add()
        {
            var adminUsers = SiteHelper.GetAPIResult<PagedList<AdminUser>>(string.Format("/AdminSys/GetUser?pageIndex=1&pageSize={0}", int.MaxValue));
            ViewBag.AdminUsers = adminUsers.PageData;

            return View();
        }

        [HttpPost]
        public ActionResult Add(Area model)
        {
            var adminId = SiteService.GetAdminUserID();
            model.AdminID = adminId;

            var result = SiteHelper.PostAPIResult<Area>("/Area/AddArea", model);
            if (result.Result)
                return ConfirmReturnList("List");
            else
                return Alert(result.Message);
        }

        public ActionResult Edit(int id)
        {
            var area = SiteHelper.GetAPIResult<Area>(string.Format("/Area/GetAreaById?areaID={0}", id));
            if (area == null)
                return Alert("区域不存在");
            ViewBag.areaID = area.Id;
            var adminUsers = SiteHelper.GetAPIResult<PagedList<AdminUser>>(string.Format("/AdminSys/GetUser?pageIndex=1&pageSize={0}", int.MaxValue));
            ViewBag.AdminUsers = adminUsers.PageData;

            return View(area);
        }

        [HttpPost]
        public ActionResult Edit(Area model, string areaID)
        {
            var result = SiteHelper.GetAPIResult<APIResult>(string.Format("/Area/ModifyArea?areaID={0}&oldAreaID={1}&AreaName={2}&clientPlatform={3}&platformID={4}&memo={5}&adminID={6}&sort={7}&shareURL={8}&showConvertLimit={9}&showAllGameLimit={10}",
                model.Id, areaID, model.AreaName, model.ClientPlatform, model.PlatformID, model.Memo, SiteService.GetAdminUserID(), model.Sort, model.ShareURL, model.ShowConvertLimit, model.ShowAllGameLimit));
            if (result.Result)
                return ConfirmReturnList("List");
            else
                return Alert(result.Message);
        }

        public ActionResult Delete(int id)
        {
            var result = SiteHelper.GetAPIResult<APIResult>(string.Format("/Area/DeleteArea?areaID={0}", id));
            return Alert(result.Message, "/Area/List?_r=" + new Random().NextDouble());
        }


        #region 管理平台

        public ActionResult AdminList()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetAdminList([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string loginName,int? organizationID)
        {
            var draw = requestModel.Draw;
            var start = requestModel.Start;
            var length = requestModel.Length;
            var pageIndex = Math.Ceiling((decimal)start / length) + 1;

            string organizationIDs = organizationID.ToString();
            if (organizationID == null)
                organizationIDs = GetCurrentAdminUserOrgIdOrEmptyText();

            var api = SiteHelper.GetAPIResult<PagedList<AdminUser>>($"AdminSys/GetUsersPagingByOrgID?orgId={organizationIDs}&UserNameKeyword={loginName}&PageIndex={pageIndex}&PageSize={length}");

            List<AdminUserViewModel> viewModels = ConvertToViewModels(api.PageData);

            return Json(new
            {
                draw = draw,
                recordsTotal = api.TotalCount,
                recordsFiltered = api.TotalCount,
                success = true,
                data = viewModels
            });
        }



        public ActionResult GetAdminPlatform(int adminID)
        {
            var GetPlatform = SiteHelper.GetAPIResult<List<Platform>>("/Area/GetPlatform");
            var GetArea = SiteHelper.GetAPIResult<List<Area>>("/Area/GetArea");


            var platforms = from pro in GetPlatform
                            select new
                            {
                                pro.Id,
                                pro.Name
                            };

            var area = from pro in GetArea
                       select new
                       {
                           AreaID = pro.Id,
                           pro.AreaName,
                           pro.PlatformID
                       };

            List<int> queryMyOwnPlatformIds = GetCurrentUserOrgnizationSupportPlatforms();

            //過濾此用戶可以看到的platform
            platforms = platforms.Where(x => queryMyOwnPlatformIds.Contains(x.Id)).ToList();
            area = area.Where(x => queryMyOwnPlatformIds.Contains(x.PlatformID)).ToList();


            //取出使用者當前已被設置的
            var userPlatformIDs = SiteHelper.GetAPIResult<List<Platform>>(string.Format("/Area/GetPlatformByAdminID?adminID={0}", adminID)).Select(m => m.Id).ToList();
            var userAreaIDs = SiteHelper.GetAPIResult<List<AreaAdminUser>>(string.Format("/Area/GetAreaByAdminID?adminID={0}", adminID)).Select(m => m.AreaID).ToList();

            return Json(new
            {
                platforms,
                area,
                userPlatformIDs,
                userAreaIDs,
            }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult AdminPlatform(int adminID, string areaIds)
        {
            var areaAdminUser = JsonConvert.DeserializeObject<List<AreaAdminUser>>(areaIds);
            var ret = SiteHelper.GetAPIResult<APIResult>(string.Format("/Area/RemoveAdminPlatform?adminID={0}", adminID));
            
            if (areaAdminUser != null)
            {
                areaAdminUser.ForEach(s =>
                {
                    ret = SiteHelper.GetAPIResult<APIResult>($"/Area/AddAdminPlatform?adminID={adminID}&platformID={s.PlatformID}&areaID={s.AreaID}");
                });
            }

            return Json(new
            {
                msg = "更新成功",
                status = true
            });
        }

        #endregion


        private List<AdminUserViewModel> ConvertToViewModels(List<AdminUser> queryData)
        {
            List<int> orgIds = queryData.Select(x => x.OrgID).ToList();

            var queryOrgs = GameApiData.GetOrganizationByIds(orgIds);

            var query = from pro in queryData
                        select new AdminUserViewModel
                        {
                            UserID = pro.UserID,
                            LoginName = pro.LoginName,
                            DisplayName = pro.DisplayName,
                            OrganizationName = queryOrgs.FirstOrDefault(x => x.OrgID == pro.OrgID)?.Name
                        };

            return query.ToList();
        }



        private List<int> GetCurrentUserOrgnizationSupportPlatforms()
        {
            int orgId = GetCurrentAdminUserOrgID();

            var query = SiteHelper.GetAPIResult<List<OrganizationPlatformMapping>>($"Organization/GetOrganizationPlatformMappingByOrgId?orgId={orgId}");
            return query.Select(x => x.PlatformID).ToList();
        }


    }
}