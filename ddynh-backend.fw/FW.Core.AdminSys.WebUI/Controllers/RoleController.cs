using FW.Core.AdminSys.Provider;
using FW.Core.AdminSys.Provider.Entity;
using FW.Core.Helper;
using FW.Core.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace FW.Core.AdminSys.WebUI.Controllers
{
    public class RoleController : AdminSysBaseController
    {
        private IAdminSysProvider _provider;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="provider"></param>
        public RoleController(IAdminSysProvider provider)
        {
            _provider = provider;
        }

        public ActionResult List()
        {
            //這功能改起來挺複雜的 先延後好了
            string orgIdText = GetCurrentAdminUserOrgIdOrEmptyText();
            var queryRoles = AdmiSysWebUiApiDataHelper.GetRolesByOrgId(orgIdText);

            var queryOrgs = AdmiSysWebUiApiDataHelper.GetOrganizations(orgIdText);

            foreach(var item in queryRoles)
            {
                //借用memo的欄位來用 (偷懶寫法)
                item.Memo = queryOrgs.FirstOrDefault(x => x.OrgID == item.OrgID)?.Name;
            }

            return ThemeableView("List", queryRoles);
        }

        [HttpGet]
        public ActionResult Add()
        {
            string orgIdText = GetCurrentAdminUserOrgIdOrEmptyText();

            ViewBag.OrgDropdown = AdmiSysWebUiApiDataHelper.GetOrganizations(orgIdText)
                .Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.OrgID.ToString()
                })
               .ToList(); ;

            return ThemeableView("Add");
        }

        [HttpPost]
        public ActionResult Add(RoleUpdateRequestViewModel model)
        {
            if (model == null)
                return Alert("参数错误");

            var result = AdmiSysWebUiApiDataHelper.CreateRole(model);

            if(result.Result)
            {
                return ConfirmContinue(Url.Action("List"));
            }

            return Alert(result.Message);
        }

        [HttpGet]
        public ActionResult Modify(int roleID)
        {
            Role query = _provider.GetRoleByID(roleID);

            //檢查是否查到別組織的資料
            CheckIsOwnPermissionForOrg(query.OrgID);

            return ThemeableView("Modify", query);
        }

        [HttpPost]
        public ActionResult Modify(RoleUpdateViewModel role)
        {
            string message = "";

            if(role.IsTemplateForCopyCheckBox == "on")
            {
                role.IsTemplateForCopy = true;
            }

            if (_provider.ModifyRole(role, ref message))
            {
                return ConfirmReturnList(Url.Action("List"));
            }

            return Alert(message);
        }

        [HttpGet]
        public ActionResult Del(int roleID)
        {
            Role query = _provider.GetRoleByID(roleID);

            //檢查是否查到別組織的資料
            CheckIsOwnPermissionForOrg(query.OrgID);

            string message = "";
            _provider.DeleteRole(roleID, ref message);
            return Alert(message, Url.Action("List"));
        }

        [HttpGet]
        public ActionResult RoleRight(int roleID)
        {
            var queryRole = _provider.GetRoleByID(roleID);

            //檢查是否查到別組織的資料
            CheckIsOwnPermissionForOrg(queryRole.OrgID);

            ViewBag.role = queryRole;
            ViewBag.roleRights = _provider.GetRoleRight(roleID, null)
                .Select(rr => rr.RightID)
                .ToList();

            return ThemeableView("RoleRight");
        }

        [HttpPost]
        public ActionResult RoleRight(int roleID, int[] ck)
        {
            string message = string.Empty;

            var OldRoleRight = _provider.GetRoleRight(roleID, null)
                .Select(rr => rr.RightID)
                .ToList();

            var add = ck.Except(OldRoleRight);
            var del = OldRoleRight.Except(ck);

            if (del != null && del.Count() > 0)
            {
                del.ToList().ForEach(s =>
                {
                    int id = Convert.ToInt32(s);

                    _provider.RemoveRoleRight(roleID, id, ref message);
                });
            }

            if (add != null && add.Count() > 0)
            {
                add.ToList().ForEach(s =>
                {
                    int id = Convert.ToInt32(s);

                    _provider.AddRoleRight(roleID, id, ref message);
                });
            }

            return RedirectToAction("RoleRight", new { roleID = roleID });
        }

    }


    public class RoleUpdateViewModel : Role
    {
        public string IsTemplateForCopyCheckBox { get; set; }

    }

}