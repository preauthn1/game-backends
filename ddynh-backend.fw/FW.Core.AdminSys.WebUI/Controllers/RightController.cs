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

            List<Right> queryRights = _provider.GetRight(null, parentID, null).OrderByDescending(r => r.Sort).ToList();

            var viewModels = ConvertToRightViewModels(queryRights);

            return ThemeableView("List", viewModels);
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
            var queryRights = _provider.GetRight(null, null, null).OrderByDescending(r => r.Sort).ToList();

            int orgID = GetCurrentAdminUserOrgID();

            //其實這邊效能應該滿差的(迴圈查DB)
            if(!IsSystemAdminOrganization(orgID))
            {
                //有些rightID是只有系統管理員可以設置 (非系統管理員要排除掉)
                List<int> excludeRightIds = AdmiSysWebUiApiDataHelper.GetSystemAdminRightIds();
                queryRights = queryRights.Where(x => !excludeRightIds.Contains(x.RightID)).ToList();
            }

            return ThemeableView("RightTree", new Tuple<List<Right>, List<int>>(queryRights, checkedRights));
        }

        private List<RightViewModel> ConvertToRightViewModels(List<Right> query)
        {
            List<int> rightIDs = query.Select(x => x.RightID).ToList();
            
            var queryUserRights = AdmiSysWebUiApiDataHelper.GetUserRightsByRightIDs(rightIDs);

            var queryRoleRights = AdmiSysWebUiApiDataHelper.GetRoleRightsByRightIDs(rightIDs);

            return query.Select(x => new RightViewModel
            {
                UserRightCount = queryUserRights.Where(z => z.RightID == x.RightID).Count(),
                UserIDsText = string.Join("、" , queryUserRights.Where(z => z.RightID == x.RightID).Select(z=> z.UserID)),
                RoleRightCount = queryRoleRights.Where(z => z.RightID == x.RightID).Count(),
                RoleIDsText = string.Join("、", queryRoleRights.Where(z => z.RightID == x.RightID).Select(z => z.RoleID)),
                IconCls = x.IconCls,
                PageUrl = x.PageUrl,
                ParentID = x.ParentID,
                RightID = x.RightID,
                RightName = x.RightName,
                RightToken = [REDACTED],
                RightType = x.RightType,
                ShowInMenu = x.ShowInMenu,
                Sort = x.Sort,
            })
            .ToList();
        }

    }
}