using FW.Core.AdminSys.Provider;
using FW.Core.AdminSys.Provider.Entity;
using System;
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
            return ThemeableView("List", _provider.GetRole(null, null));
        }

        [HttpGet]
        public ActionResult Add()
        {
            return ThemeableView("Add");
        }

        [HttpPost]
        public ActionResult Add(string roleName)
        {
            string message = "";
            if (_provider.CreateRole(roleName, ref message))
            {
                return ConfirmContinue(Url.Action("List"));
            }

            return Alert(message);
        }

        [HttpGet]
        public ActionResult Modify(int roleID)
        {
            return ThemeableView("Modify", _provider.GetRoleByID(roleID));
        }

        [HttpPost]
        public ActionResult Modify(Role role)
        {
            string message = "";
            if (_provider.ModifyRole(role, ref message))
            {
                return ConfirmReturnList(Url.Action("List"));
            }

            return Alert(message);
        }

        [HttpGet]
        public ActionResult Del(int roleID)
        {
            string message = "";
            _provider.DeleteRole(roleID, ref message);
            return Alert(message, Url.Action("List"));
        }

        [HttpGet]
        public ActionResult RoleRight(int roleID)
        {
            ViewBag.role = _provider.GetRoleByID(roleID);
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
}