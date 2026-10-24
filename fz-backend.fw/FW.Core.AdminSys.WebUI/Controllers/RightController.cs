using FW.Core.AdminSys.Provider;
using FW.Core.AdminSys.Provider.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FW.Core.Web;
using FW.Core.AdminSys.Provider.RightFinder.Attribute;

namespace FW.Core.AdminSys.WebUI.Controllers
{
    public class RightController : AdminSysBaseController
    {
        private IAdminSysProvider _provider;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="provider"></param>
        public RightController(IAdminSysProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// 权限列表
        /// </summary>
        /// <param name="parentID"></param>
        /// <returns></returns>
        public ActionResult List(int parentID = 0)
        {
            ViewBag.parentID = parentID;
            ViewBag.TopID = parentID == 0 ? 0 : _provider.GetRightByID(parentID).ParentID;
            return ThemeableView("List", _provider.GetRight(null, parentID, null).OrderByDescending(r => r.Sort).ToList());
        }

        [HttpGet]
        public ActionResult Add(int parentID = 0)
        {
            return ThemeableView("Add", _provider.GetRightByID(parentID));
        }

        [HttpPost]
        public ActionResult Add(Right right)
        {
            if (!ModelState.IsValid)
            {
                return Alert(ModelState.GetValidateErrorMessage());
            }

            string message = "";
            if (_provider.AddRight(right, ref message))
            {
                return ConfirmContinue(Url.Action("List", new { parentID = right.ParentID}));
            }
            return Alert(message, Url.Action("List", new { parentID = right.ParentID }));
        }

        public ActionResult Del(int id)
        {
            var r = _provider.GetRightByID(id);

            string message = "";
            _provider.DeleteRight(id, ref message);

            return Alert(message, Url.Action("List", new { parentID = r.ParentID }));
        }

        [HttpGet]
        public ActionResult Modify(int id)
        {
            return ThemeableView("Modify", _provider.GetRightByID(id));
        }

        [HttpPost]
        public ActionResult Modify(Right right)
        {
            string message = "";
            if (_provider.ModifyRight(right, ref message))
            {
                var r = _provider.GetRightByID(right.RightID);
                return ConfirmReturnList(Url.Action("List", new { parentID = r.ParentID}));
            }
            return Alert(message);
        }

        [ChildActionOnly]
        public ActionResult RightTree(List<int> checkedRights)
        {
            return ThemeableView("RightTree", new Tuple<List<Right>, List<int>>(_provider.GetRight(null, null, null).OrderByDescending(r => r.Sort).ToList(), checkedRights));
        }



    }
}