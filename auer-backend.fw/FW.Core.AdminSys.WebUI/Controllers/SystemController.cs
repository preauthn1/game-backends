using FW.Core.AdminSys.Provider;
using FW.Core.AdminSys.Provider.Entity;
using FW.Core.Auth;
using FW.Core.Auth.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FW.Core.AdminSys.WebUI.Controllers
{
    public class SystemController : AdminSysBaseController
    {
        private IAdminSysProvider _adminSysProvider;
        private ISysInfoProvider _sysInfoProvider;
        private AuthManager _authManager;
        private IRightTokenBuilder _rightTokenBuilder;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="adminSysProvider"></param>
        /// <param name="sysInfoProvider"></param>
        /// <param name="authManager"></param>
        /// <param name="rightTokenBuilder"></param>
        public SystemController(IAdminSysProvider adminSysProvider, ISysInfoProvider sysInfoProvider, AuthManager authManager, IRightTokenBuilder rightTokenBuilder)
        {
            _adminSysProvider = adminSysProvider;
            _sysInfoProvider = sysInfoProvider;
            _authManager = authManager;
            _rightTokenBuilder = rightTokenBuilder;
        }

        /// <summary>
        /// 收藏夹列表
        /// </summary>
        /// <returns></returns>
       [HttpGet]
        public ActionResult SavePageList()
        {
           int userID = Convert.ToInt32(_authManager.CurrentUser.UserID);
            return ThemeableView("SavePageList", _adminSysProvider.GetQuickAction(userID));
        }

       [ChildActionOnly]
       public ActionResult QuickAction()
       {
           int userID = Convert.ToInt32(_authManager.CurrentUser.UserID);
           string url = Request.Url.ToString();
           var quickAction = _adminSysProvider.GetQuickActionByPage(userID, url);

           var currentRight = getCurrentPageRight();
           if (currentRight != null)
           {
               ViewBag.pageName = currentRight.RightName;
           }

           return ThemeableView("QuickAction", quickAction);
       }

        /// <summary>
        /// AJAX 调用，新增收藏
        /// </summary>
        /// <param name="act"></param>
        /// <returns></returns>
       [HttpGet]
       public ActionResult AddQuickActionFast(QuickAction act)
       {
           int userID = Convert.ToInt32(_authManager.CurrentUser.UserID);
           string message = "";
           act.UserID = userID;
           var quickAction = act;
           if (_adminSysProvider.AddQuickAction(quickAction, ref message))
           {
               return Content("true");
           }

           return Content("false");
       }

        /// <summary>
        /// AJAX调用，删除收藏
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
       [HttpGet]
       public ActionResult DelQuickActionFast(string url)
       {
           int userID = Convert.ToInt32(_authManager.CurrentUser.UserID);
           var qa = _adminSysProvider.GetQuickActionByPage(userID, url);
           if (qa != null)
           {
               string message = "";
               if (_adminSysProvider.DelQuickAction(qa.Id, userID, ref message))
               {
                   return Content("true");
               }
           }

           return Content("false");
       }


        /// <summary>
        /// 添加收藏夹
        /// </summary>
        /// <returns></returns>
       [HttpGet]
        public ActionResult AddQuickAction()
       {
           return ThemeableView("AddQuickAction");
       }

        /// <summary>
        /// 添加收藏夹
        /// </summary>
        /// <param name="qa"></param>
        /// <returns></returns>
        [HttpPost]
       public ActionResult AddQuickAction(QuickAction qa)
       {
            int userID = Convert.ToInt32(_authManager.CurrentUser.UserID);
            qa.UserID = userID;
           string message = "";
           if (_adminSysProvider.AddQuickAction(qa, ref message))
           {
               return ConfirmContinue(Url.Action("SavePageList"));
           }

           return Alert(message);
       }

        /// <summary>
        /// 修改收藏夹
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ModifyQuickAction(int id)
        {
            return ThemeableView("ModifyQuickAction", _adminSysProvider.GetQuickActionByID(id));
        }

        /// <summary>
        /// 修改收藏夹
        /// </summary>
        /// <param name="qa"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ModifyQuickAction(QuickAction qa)
        {
            int userID = Convert.ToInt32(_authManager.CurrentUser.UserID);
            qa.UserID = userID;
            string message = "";
            if (_adminSysProvider.ModifyQuickAction(qa, ref message))
            {
                return ConfirmReturnList(Url.Action("SavePageList"));
            }
            return Alert(message);
        }

        /// <summary>
        /// 删除收藏夹
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult DelQuickAction(int id)
        {
            int userID = Convert.ToInt32(_authManager.CurrentUser.UserID);
            string message = "";
            if (_adminSysProvider.DelQuickAction(id, userID, ref message))
            {
                return RedirectToAction("SavePageList");
            }

            return Alert(message);
        }


       [NonAction]
       private Right getCurrentPageRight()
       {
           int userID = Convert.ToInt32(_authManager.CurrentUser.UserID);
           string rightToken = [REDACTED];
           var rt = _adminSysProvider.GetAllUserRight(userID).Where(r => String.Compare(r.RightToken, rightToken, true) == 0).FirstOrDefault();
           return rt;
       }

       [NonAction]
       private string getCurrentRightToken()
       {
           return _rightTokenBuilder.CreateRightToken(string.Empty).ToString(); ;
       }

        [ChildActionOnly]
       public ActionResult MyQuickAction()
       {
           int userID = Convert.ToInt32(_authManager.CurrentUser.UserID);
           return ThemeableView("MyQuickAction", _adminSysProvider.GetQuickAction(userID));
       }

	}
}