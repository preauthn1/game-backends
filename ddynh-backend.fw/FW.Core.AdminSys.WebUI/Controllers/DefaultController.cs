using System.Configuration;
using FW.Core.AdminSys.Provider;
using FW.Core.AdminSys.Provider.Entity;
using FW.Core.Auth;
using FW.Core.Auth.Token;
using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Google.Authenticator;
using FW.Core.Cache;

namespace FW.Core.AdminSys.WebUI.Controllers
{
    public class DefaultController : AdminSysBaseController
    {
        private IAdminSysProvider _adminSysProvider;
        private ISysInfoProvider _sysInfoProvider;
        private AuthManager _authManager;
        private IRightTokenBuilder _rightTokenBuilder;
        private ICacheManager _cache;


        private const string SYSTEMPANEL = "systempanel";

        private const string DefaultOrgName = "AE棋牌管理";

        

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="adminSysProvider"></param>
        /// <param name="sysInfoProvider"></param>
        /// <param name="authManager"></param>
        /// <param name="rightTokenBuilder"></param>
        public DefaultController(IAdminSysProvider adminSysProvider, ISysInfoProvider sysInfoProvider, AuthManager authManager, IRightTokenBuilder rightTokenBuilder , ICacheManager cache)
        {
            _adminSysProvider = adminSysProvider;
            _sysInfoProvider = sysInfoProvider;
            _authManager = authManager;
            _rightTokenBuilder = rightTokenBuilder;
            _cache = cache;
        }

        public ActionResult Index()
        {
            return ThemeableView("Index", _sysInfoProvider.LoadSystemInfo());
        }

        public ActionResult Main()
        {
            return ThemeableView();
        }

        public ActionResult Title()
        {
            string titleName = _sysInfoProvider.LoadSystemInfo().SystemName;
            Right currentRight = getCurrentPageRight();
            
            if (currentRight != null)
            {
                titleName = currentRight.RightName;
            }
            return Content(titleName);
        }
        public ActionResult oldTop()
        {
            int userID = Convert.ToInt32(_authManager.CurrentUser.UserID);
            var rights = _adminSysProvider.GetAllUserDisplayRight(userID)
                .Where(p => p.ShowInMenu == true)
                .OrderByDescending(p => p.Sort)
                .ThenByDescending(p => p.RightID)
                .ToList();

            //填充url
            var topMenus = rights.Where(r => r.ParentID == 0).ToList();
            setMenuPageUrl(topMenus, rights);


            ViewBag.User = _authManager.CurrentUser;
            ViewBag.modules = topMenus;

            return ThemeableView("Top", _sysInfoProvider.LoadSystemInfo());
        }

        /// <summary>
        /// 設定左上角LOGO
        /// </summary>
        private string SetTopLogo()
        {
            //抓取登入者組織名稱，也許可以在 AuthManager 存起來，但暫時不宜去改動FW
            int orgID = GetCurrentAdminUserOrgID();

            string topLogText = string.Empty;

            if(orgID <= 0)
            {
                topLogText = DefaultOrgName;
            }
            else
            {
                topLogText = GetOrgNameByCache(orgID);
            }

            return topLogText;
        }


        private string GetOrgNameByCache(int orgID)
        {
            try
            {
                int caheSeconds = 300;
                string cacheKey = $"OrgData_{orgID}";

                string queryOrgName = MyCacheHelper.GetCacheOrQueryValue<string>(_cache, cacheKey, () =>
                {
                    Organization organization = APIHelper.GetAPIResult<Organization>($"/Organization/GetByOrgById?id={orgID}");
                    return organization.Name;

                }, caheSeconds);

                return queryOrgName;

            }
            catch
            {

            }

            return DefaultOrgName;
            
        }






        public ActionResult Top()
        {
            ViewBag.User = _authManager.CurrentUser;
            ViewBag.Logo = SetTopLogo();
            ViewBag.PlatformIDs = System.Web.HttpContext.Current?.Session["adminPlatformIDs"];

            return ThemeableView("Top", _sysInfoProvider.LoadSystemInfo());
        }

        public ActionResult MenuMobile()
        {
            int userID = Convert.ToInt32(_authManager.CurrentUser.UserID);
            var urs = _adminSysProvider.GetAllUserDisplayRight(userID);

            var topMenus = urs
                .Where(p => p.ShowInMenu == true && p.ParentID == 0)
                .OrderByDescending(p => p.Sort)
                .ThenByDescending(p => p.RightID)
                .ToList();

            //填充url
            setMenuPageUrl(topMenus, urs);
            var tepMenusIds = topMenus.Select(m => m.RightID);
            ViewBag.modules = topMenus;

            List<Right> rs = new List<Right>();

            if (!String.IsNullOrWhiteSpace(Request.QueryString[SYSTEMPANEL]))
            {
                rs = urs
               .Where(p => p.ShowInMenu == true && tepMenusIds.Contains(p.ParentID))
               .OrderByDescending(p => p.Sort)
               .ThenByDescending(p => p.RightID)
               .ToList();

                string iconcls = rs.FirstOrDefault().IconCls;

                var sps = _adminSysProvider.GetQuickAction(userID);
                sps.ForEach(sp =>
                {
                    var r = new Right();
                    r.RightName = sp.Name;
                    r.PageUrl = sp.Url;
                    r.IconCls = iconcls;

                    rs.Add(r);
                });
            }
            else
            {
                rs = urs.Where(p => p.ShowInMenu == true && tepMenusIds.Contains(p.ParentID))
                              .OrderByDescending(p => p.Sort)
                              .ThenByDescending(p => p.RightID)
                              .ToList();
            }

            //填充页面地址
            setMenuPageUrl(rs, urs);

            //设置系统面板标识
            setSystemPanel(rs);

            return ThemeableView("MenuMobile", rs);
        }


        public ActionResult Menu()
        {
            int userID = Convert.ToInt32(_authManager.CurrentUser.UserID);
            var urs = _adminSysProvider.GetAllUserDisplayRight(userID);
            var allRights = urs
                .Where(p => p.ShowInMenu == true)
                .OrderByDescending(p => p.Sort)
                .ThenByDescending(p => p.RightID)
                .ToList();
            ViewBag.allRights = allRights;
            var currentRootRight = getRootRight();
            ViewBag.currentRootRight = currentRootRight;
            ViewBag.currentPageRight = getCurrentPageRight();

            foreach (var rootRight in allRights.Where(p => p.ParentID == 0))
            {
                var menuRights = allRights.Where(p => p.ParentID == rootRight.RightID).ToList();
                setMenuPageUrl(menuRights, allRights);
            }

            return ThemeableView("Menu", allRights);
        }

        public ActionResult oldMenu()
        {
            int userID = Convert.ToInt32(_authManager.CurrentUser.UserID);
            var urs = _adminSysProvider.GetAllUserDisplayRight(userID);
            var allRights = urs
                .Where(p => p.ShowInMenu == true)
                .OrderByDescending(p => p.Sort)
                .ThenByDescending(p => p.RightID)
                .ToList();
            ViewBag.allRights = allRights;
            var currentRootRight = getRootRight();
            ViewBag.currentRootRight = currentRootRight;
            ViewBag.currentPageRight = getCurrentPageRight();
            if (currentRootRight == null)
            {
                return Content("");
            }

            List<Right> rs = new List<Right>();

            if (!String.IsNullOrWhiteSpace(Request.QueryString[SYSTEMPANEL]))
            {
                rs = urs
               .Where(p => p.ShowInMenu == true && p.ParentID == currentRootRight.RightID)
               .OrderByDescending(p => p.Sort)
               .ThenByDescending(p => p.RightID)
               .ToList();

                string iconcls = rs.FirstOrDefault().IconCls;

                var sps = _adminSysProvider.GetQuickAction(userID);
                sps.ForEach(sp =>
                {
                    var r = new Right();
                    r.RightName = sp.Name;
                    r.PageUrl = sp.Url;
                    r.IconCls = iconcls;

                    rs.Add(r);
                });
            }
            else
            {
                rs = urs.Where(p => p.ShowInMenu == true && p.ParentID == currentRootRight.RightID)
                              .OrderByDescending(p => p.Sort)
                              .ThenByDescending(p => p.RightID)
                              .ToList();
            }

            //填充页面地址
            setMenuPageUrl(rs, urs);

            //设置系统面板标识
            setSystemPanel(rs);

            return ThemeableView("Menu", rs);
        }

        [HttpGet]
        public ActionResult Logon(string message)
        {
            ViewBag.message = message;

            //開發方便用
            SetDefaultLoginNameWhenLocalDev();

            return ThemeableView("Logon", _sysInfoProvider.LoadSystemInfo());
        }

        [HttpPost]
        public ActionResult Logon(string loginName, string password, string vcode, string phoneCode = null, string scretKey = null)
        {

            string message = "";
            var user = _adminSysProvider.ValidateUserInfo(loginName, password, ref message);
            if (user == null)
            {
                return RedirectToAction("Logon", new { message = message });
            }
            var _scretKey = user.SecretKey;

            //联运无须判断
            if (user.UserType != 10)
            {
                if (bool.Parse(ConfigurationManager.AppSettings["NeedVerification"]))
                {
                    #region 金鑰判斷

                    if (_scretKey == 0)
                    {
                        TempData["UserID"] = user.UserID;
                        TempData["LoninName"] = user.LoginName;
                        return RedirectToAction("SettingScretKey");
                    }

                    var tfa = new TwoFactorAuthenticator();
                    var result = tfa.ValidateTwoFactorPIN(_scretKey.ToString(), scretKey, new TimeSpan(0, 0, 60));

                    if (!result)
                    {
                        return RedirectToAction("Logon", new { message = "金钥验证码不正确" });
                    }

                    #endregion
                }
            }
            var authUser = new AuthUser()
            {
                LoginName = loginName,
                UserID = user.UserID.ToString(),
                SecretKey = _scretKey,
                UserType = user.UserType,
                OrgID = user.OrgID
            };

            _authManager.Signin(authUser);
            Session["FW.Core.AdminSys.WebUI.LoginIP"] = IPHelper.GetIP();
            user.LastLogonTime = DateTime.Now;
            user.LastLogonIPAddress = IPHelper.GetIP();
            _adminSysProvider.UpdateUserLoginInfo(user);
            Event.LoginFinshed();
            string url = Event.GetLoginUrl();
            if (string.IsNullOrWhiteSpace(url))
            {
                return RedirectToAction("Index", new { systempanel = 1 });
            }
            else
            {
                return Redirect(url);
            }

        }

        [HttpGet]
        public ActionResult SettingScretKey()
        {

            var _userID = TempData["UserID"];
            if (_userID == null)
            {
                return RedirectToAction("Logon");
            }

            var _loginName = TempData["LoninName"].ToString();
            var _secretKey = new Random().Next(10000000, 999999999);
            var _codeTag = ConfigurationManager.AppSettings["QRCodeTag"];
            var _tfa = new TwoFactorAuthenticator();
            var _setupInfo = _tfa.GenerateSetupCode(_codeTag, _loginName, _secretKey.ToString(), 300, 300);

            ViewBag.UserID = _userID;
            ViewBag.SecretKey = _secretKey;
            ViewBag.QrCodeUrl = _setupInfo.QrCodeSetupImageUrl;
            return ThemeableView();
        }

        [HttpPost]
        public ActionResult saveQRCode(int userID, string secretKey)
        {
            string message = null;
            _adminSysProvider.ModifyUserInfo(Convert.ToInt32(userID), null, null, null, null, null, null, Convert.ToInt32(secretKey), ref message);

            return RedirectToAction("Logon");
        }

        [HttpGet]
        public ActionResult Logout()
        {
            _authManager.Signout();
            Session.Clear();
            Session.Abandon(); //20180416 alan add        
            return Content("<script>window.parent.location.href='" + Url.Action("Logon") + "';</script>");
        }

        [HttpGet]
        public void LastLogon()
        {
            var loginName = CurrentUser.LoginName;
            _adminSysProvider.UpdateLastLogon(loginName);

        }

        [ChildActionOnly]
        public ActionResult Nav(int pageRightID = 0)
        {
            int userID = Convert.ToInt32(_authManager.CurrentUser.UserID);
            var rights = _adminSysProvider.GetAllUserRight(userID);

            if (pageRightID == 0)
            {
                var r = getCurrentPageRight();
                if (r == null)
                {
                    return Content("");
                }

                pageRightID = r.RightID;
            }

            var parentRights = new List<Right>();
            getParentRight(pageRightID, rights, parentRights);
            parentRights.Reverse();


            return ThemeableView("Nav", parentRights);

        }

        private void getParentRight(int pageRightID, List<Right> rights, List<Right> parentRights)
        {
            var pageRight = rights.Where(r => r.RightID == pageRightID).FirstOrDefault();
            parentRights.Add(pageRight);
            if (pageRight == null || pageRight.ParentID == 0)
            {
                return;
            }

            int parentRightID = pageRight.ParentID;

            getParentRight(parentRightID, rights, parentRights);

        }

        [ChildActionOnly]
        public ActionResult RightTab()
        {
            var prs = GetParentsRight();
            if (prs == null || prs.Count == 0)
            {
                return Content("");
            }

            if (prs[0].ParentID != 0)
            {
                prs.Reverse();
            }
            var parentRightID = prs.ElementAt(1).RightID;

            int userID = Convert.ToInt32(_authManager.CurrentUser.UserID);
            var rights = _adminSysProvider.GetAllUserRight(userID).Where(r => r.ParentID == parentRightID).ToList();
            if (rights != null && rights.Count > 0)
            {
                rights = rights.Where(r => r.ShowInMenu == true)
                    .OrderByDescending(r => r.Sort)
                    .ThenByDescending(r => r.RightID)
                    .ToList();
            }

            ViewBag.currentRight = getCurrentPageRight(true);

            //设置系统面板标识
            setSystemPanel(rights);

            return ThemeableView("RightTab", rights);
        }

        [ChildActionOnly]
        public ActionResult MenuActive()
        {
            var prs = GetParentsRight();
            if (prs == null || prs.Count == 0)
            {
                return Content("");
            }
            return ThemeableView("MenuActive", prs);
        }

        [ChildActionOnly]
        public ActionResult MenuActiveMobile()
        {
            var prs = GetParentsRight();
            if (prs == null || prs.Count == 0)
            {
                return Content("");
            }
            var r = getCurrentPageRight();
            ViewBag.CurrentPageRight = r;
            return ThemeableView("MenuActiveMobile", prs);
        }

        [NonAction]
        private Right getCurrentPageRight(bool truly = false)
        {
            int? frid = null;
            string fromright = Request.QueryString["fromright"];
            if (!String.IsNullOrWhiteSpace(fromright))
            {
                if (new Regex("^\\d+$").IsMatch(fromright))
                {
                    frid = Convert.ToInt32(fromright);
                }
            }

            if (truly || frid == null)
            {
                int userID = Convert.ToInt32(_authManager.CurrentUser.UserID);
                string rightToken = [REDACTED];
                var rt = _adminSysProvider.GetAllUserRight(userID).Where(r => String.Compare(r.RightToken, rightToken, true) == 0).FirstOrDefault();
                return rt;
            }

            return _adminSysProvider.GetRightByID(frid.Value);

        }

        [NonAction]
        private string getCurrentRightToken()
        {
            return _rightTokenBuilder.CreateRightToken(string.Empty).ToString();
        }

        /// <summary>
        /// 获取根级权限
        /// </summary>
        /// <returns></returns>
        [NonAction]
        private Right getRootRight()
        {
            if (Request.QueryString.AllKeys.Contains(SYSTEMPANEL))
            {
                var token = [REDACTED]"_System__");
                return _adminSysProvider.GetRightByToken(token);
            }

            var cr = getCurrentPageRight();
            if (cr == null)
            {
                return null;
            }

            int userID = Convert.ToInt32(_authManager.CurrentUser.UserID);
            var rs = _adminSysProvider.GetAllUserRight(userID);
            Right rootRight = null;
            var parentRight = rs.Where(r => r.RightID == cr.ParentID).FirstOrDefault();
            while (parentRight != null && parentRight.ParentID != 0)
            {
                parentRight = rs.Where(r => r.RightID == parentRight.ParentID).FirstOrDefault();
            }

            if (parentRight != null && parentRight.ParentID == 0)
            {
                rootRight = parentRight;
            }

            return rootRight;
        }

        /// <summary>
        /// 获取所有父级权限
        /// </summary>
        /// <returns></returns>
        [NonAction]
        private List<Right> GetParentsRight()
        {
            var r = getCurrentPageRight();
            if (r == null)
            {
                return null;
            }

            int userID = Convert.ToInt32(_authManager.CurrentUser.UserID);
            var rights = _adminSysProvider.GetAllUserRight(userID);
            var parentRights = new List<Right>();
            getParentRight(r.RightID, rights, parentRights);
            parentRights.Reverse();

            return parentRights;
        }


        /// <summary>
        /// 设置菜单项页面地址
        /// </summary>
        /// <param name="menuRights"></param>
        /// <param name="allRights"></param>
        [NonAction]
        private void setMenuPageUrl(List<Right> menuRights, List<Right> allRights)
        {
            foreach (var r in menuRights)
            {
                if (String.IsNullOrWhiteSpace(r.PageUrl))
                {
                    var tr = r;

                    var crs = allRights.Where(cr => cr.ParentID == tr.RightID).ToList();
                    while (String.IsNullOrWhiteSpace(r.PageUrl) && crs.Count > 0)
                    {
                        tr = crs.Where(cr => !String.IsNullOrWhiteSpace(cr.PageUrl)).FirstOrDefault();
                        if (tr == null)
                        {
                            tr = crs.FirstOrDefault();
                        }

                        if (!string.IsNullOrWhiteSpace(tr.PageUrl))
                        {
                            r.PageUrl = tr.PageUrl;
                            break;
                        }

                        crs = allRights.Where(cr => cr.ParentID == tr.RightID).ToList();
                    }
                }
            }
        }

        /// <summary>
        /// 设置系统面板标识
        /// </summary>
        /// <param name="rs"></param>
        [NonAction]
        private void setSystemPanel(List<Right> rs)
        {
            foreach (var r in rs)
            {
                if (!String.IsNullOrWhiteSpace(r.PageUrl))
                {
                    if (!String.IsNullOrWhiteSpace(Request.QueryString[SYSTEMPANEL]) && r.PageUrl.ToLower().IndexOf(SYSTEMPANEL) == -1)
                    {
                        if (r.PageUrl.IndexOf("?") == -1)
                        {
                            r.PageUrl += "?";
                        }
                        else
                        {
                            r.PageUrl += "&";
                        }

                        r.PageUrl += (SYSTEMPANEL + "=1");
                    }
                }
            }
        }

        private void SetSession(string key, object value, int? timeout)
        {
            int time = timeout ?? Session.Timeout;
            DateTime endTime = DateTime.Now + new TimeSpan(0, time, 0);
            var tuple = Tuple.Create(key, value, endTime);
            Session[key] = tuple;
        }


        private object GetSingleSession(string key)
        {
            if (Session[key] == null) { return null; }

            var tuple = Session[key] as Tuple<string, object, DateTime>;
            var diff = DateTime.Compare(tuple.Item3, DateTime.Now);
            object result = null;
            if (diff > 0) result = tuple.Item2;//有效時間截止之前正常返回值
            else Session.Remove(key);//超過有效時間清空Session
            return result;
        }
        

        private void SetDefaultLoginNameWhenLocalDev()
        {
            if (!IsLocalhost(9230))
                return;

            //若是開發人員電腦 自動登入
            SetDefaultLoginInDevelopComputer();
        }



        private static bool IsLocalhost(int? port = null)
        {
            string keyword = string.Format("http://localhost:{0}", (port.HasValue ? port.Value.ToString() : string.Empty));

            var request = System.Web.HttpContext.Current.Request;

            if (request == null)
                return false;

            return (request.Url.AbsoluteUri.Contains(keyword));
        }



        /// <summary>
        /// 若是開發人員電腦 自動登入
        /// </summary>
        /// <returns></returns>
        private void SetDefaultLoginInDevelopComputer()
        {
            string currentPCName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            string auto = System.Web.HttpContext.Current.Request.QueryString["auto"];

            var computerNames = System.Configuration.ConfigurationManager.AppSettings["AutoLogin_ComputerNames"];

            if (string.IsNullOrWhiteSpace(computerNames))
                return;

            if (!string.IsNullOrWhiteSpace(auto))
                return;

            var split = computerNames.Split(',').Where(z=> !string.IsNullOrWhiteSpace(z)); 

            //如果config設定的電腦名稱有符合 自動登入
            foreach(var compName in split)
            {
                string account = System.Configuration.ConfigurationManager.AppSettings["AutoLogin_Account"];
                string password = [REDACTED]"AutoLogin_Password"];
                
                if (IsComputerNameEqual(currentPCName , compName))
                {
                    ViewBag.DefaultAccount = account;
                    ViewBag.DefaultPassword = [REDACTED];
                }
            }
        }


        private bool IsComputerNameEqual(string currentPCName, string targetPCName)
        {
            currentPCName = currentPCName.Replace(@"\" , "");
            targetPCName = targetPCName.Replace(@"\" , "");
            return currentPCName == targetPCName;
        }








    }
}