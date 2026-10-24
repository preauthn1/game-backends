using FW.Core.Engine;
using FW.Core.Helper;
using FW.Core.Repository;
using game.AdminSystem.Models;
using game.Entity.AdminSys;
using game.Infrastructure;
using game.Infrastructure.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace game.AdminSystem.Controllers
{
    public class BaseController : FW.Core.Web.Mvc.BaseController
    {

        protected string No_Permission_Page_View = "NoPermission";

        protected string No_Permission_Page_Url = "/ErrorRedirect/GoToLogin";


        public BaseController()
        {
            InstanceAllServiceImplement(this);
        }


        protected void InstanceAllServiceImplement<T>(T controller) where T : class
        {
            var allService = this.GetType()
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.PropertyType.IsInterface)
                .Where(x => x.Name.ToLower().Contains("service"))
                .ToList();

            foreach (var item in allService)
            {
                var instanceValue = IOCHelper.Resolve(item.PropertyType);
                item.SetValue(this, instanceValue);
            }


            var allRep = this.GetType()
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.Name.ToLower().Contains("repository") || x.Name.ToLower().Contains("rep"))
                .ToList();
            
            foreach (PropertyInfo item in allRep)
            {
                if (item.PropertyType.BaseType != null)
                {
                    if (item.PropertyType.BaseType.IsGenericType &&
                       item.PropertyType.BaseType.GetGenericTypeDefinition() == typeof(IDapperRepository<>))
                    {
                        var instanceValue = Activator.CreateInstance(item.PropertyType);
                        item.SetValue(this, instanceValue);
                    }
                }
            }

        }


        /// <summary>
        /// uniform jquery 這套件很煩 導致checkbox處理變複雜
        /// </summary>
        protected void DisableUniformCheckbox()
        {
            ViewBag.isDisableUniformCheckbox = true;
        }

        /// <summary>
        /// 根据ID/登录名称/昵称进行查询
        /// </summary>
        /// <returns></returns>
        public string GetFindData()
        {
            string userIDType = Request["userIDType"];
            string TextIdOrName = Request["TextIdOrName"];
            if (!string.IsNullOrWhiteSpace(userIDType) && !string.IsNullOrWhiteSpace(TextIdOrName))
                return userIDType + "=" + TextIdOrName;
            else
                return string.Empty;
        }


        protected bool CheckCurrentPageAccessPermissionIsNotValid()
        {
            return !SiteService.CheckCurrentPageAccess();
        }

        /// <summary>
        /// 取得當前用戶UserId
        /// </summary>
        protected int GetCurrentUserID()
        {
            var userId = SiteService.GetAdminUserID();
            return userId;
        }

        /// <summary>
        /// 取得當前用戶UserName
        /// </summary>
        protected string GetCurrentUserName()
        {
            var userName = SiteService.GetAdminUserName();
            return userName;
        }



        /// <summary>
        /// 取得當前用戶UserName
        /// </summary>
        [NonAction]
        protected ActionResult AlertOrRedirectConfirm(ApiRequestResultModel<CRUDResultModel> apiResult , string targetActionName)
        {
            if (apiResult.ResultObject == null || apiResult.Exception != null || !apiResult.ResultObject.IsSuccess)
                return Alert(apiResult.ResultObject.Message);

            return ConfirmReturnList(targetActionName);
        }



        /// <summary>
        /// 取得當前用戶UserName
        /// </summary>
        [NonAction]
        protected ActionResult AlertOrRedirectConfirm(CRUDResultModel apiResult, string targetActionName)
        {
            if (apiResult == null || apiResult.IsSuccess)
                return Alert(apiResult.Message);

            return ConfirmReturnList(targetActionName);
        }




        [NonAction]
        protected JsonResult ConvertToDataTableJsonResult<T>(int? draw , PagedList<T> pagingObject)
        {
            var resultModel = new JQueryDataTableResultViewModel
            {
                draw = draw,
                success = true,
                recordsTotal = pagingObject.TotalCount,
                recordsFiltered = pagingObject.TotalCount,
                data = pagingObject.PageData
            };

            return Json(resultModel, JsonRequestBehavior.AllowGet);
        }


        protected void CheckPlatformPermissionExistOrRedirect(int targetPlatform)
        {
            bool isExist = IsPlatformPermissionExist(targetPlatform);
            
            if(!isExist)
            {
                //要注意這邊雖然最後會redirect 但中間的 查詢/編輯 還是會執行到。請注意
                System.Web.HttpContext.Current.Response.Redirect(No_Permission_Page_Url);
            }
        }

        protected void CheckAreaPermissionExistOrRedirect(int targetAreaId)
        {
            bool isExist = IsAreaPermissionExist(targetAreaId);

            if (!isExist)
            {
                //要注意這邊雖然最後會redirect 但中間的 查詢/編輯 還是會執行到。請注意
                System.Web.HttpContext.Current.Response.Redirect(No_Permission_Page_Url);
            }
        }


        protected bool IsPlatformPermissionExist(int targetPlatform)
        {
            var queryPlatforms = SiteHelper.GetPlatformIDs();
            return queryPlatforms.Any(x => x == targetPlatform);
        }


        protected bool IsAreaPermissionExist(int targetAreaId)
        {
            var queryAreas = DataService.GetAllArea();
            return queryAreas.Any(x => x.AreaID == targetAreaId);
        }

        protected RedirectResult Go_No_Permission_Page()
        {
            return Redirect("~" + No_Permission_Page_Url);
        }

        /// <summary>
        /// 获取当前登录用户的UserID   -AdminSys数据库
        /// </summary>
        /// <returns></returns>
        public static int GetAdminUserID()
        {
            return Convert.ToInt32(FW.Core.Engine.EngineContext.Current.Resolve<FW.Core.Auth.AuthManager>().CurrentUser.UserID);
        }


        /// <summary>
        /// 取得當前用戶的組織ID
        /// </summary>
        protected int GetCurrentAdminUserOrgID()
        {
            var user = FW.Core.Engine.EngineContext.Current.Resolve<FW.Core.Auth.AuthManager>().CurrentUser;

            if (user == null)
                return 0;

            return user.OrgID;
        }
        

        /// <summary>
        /// 取得當前用戶的組織ID
        /// </summary>
        protected string GetCurrentAdminUserOrgIdOrEmptyText()
        {
            int orgId = GetCurrentAdminUserOrgID();
            return FilterOrganizationIdOrEmpty(orgId);
        }


        /// <summary>
        /// 如果是預設系統的orgId，會顯示全部數據 而非單一的org數據
        /// </summary>
        protected string FilterOrganizationIdOrEmpty(int tragetOrgId)
        {
            int defaultSystemOrgId = SettingHelper.DefaultAdminOrganizationId;

            if (defaultSystemOrgId == tragetOrgId)
                return string.Empty;

            return tragetOrgId.ToString();
        }


        /// <summary>
        /// 判斷當前用戶 是否為系統管理員的組織
        /// </summary>
        protected bool IsCurrentUserBelongWithSystemOrganization()
        {
            int orgId = GetCurrentAdminUserOrgID();
            return IsSystemAdminOrganization(orgId);
        }


        /// <summary>
        /// 判斷是否為系統管理員的組織
        /// </summary>
        private bool IsSystemAdminOrganization(int tragetOrgId)
        {
            return tragetOrgId == SettingHelper.DefaultAdminOrganizationId;
        }

        protected void CheckIsOwnPermissionForOrg(int tragetOrgId)
        {
            var currentOrgId = GetCurrentAdminUserOrgID();

            //如果是系統管理員的組織 都允許
            if (IsSystemAdminOrganization(currentOrgId))
                return;

            if (currentOrgId != tragetOrgId)
            {
                System.Web.HttpContext.Current.Response.Redirect(No_Permission_Page_Url);
            }

            return;
        }



    }
}