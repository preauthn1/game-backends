using System;
using example.invalid;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;
using game.Entity.Payment;
using game.Entity.HttpClient;
using game.Infrastructure;
using Newtonsoft.Json;
using FW.Core.Helper;
using FW.Core.Helper.Enum;
using NPOI.XSSF.UserModel;
using Newtonsoft.Json.Linq;
using game.Entity.ViewModels;
using game.Entity;
using game.AdminSystem.Models.ViewModels;
using FW.Core.AdminSys.WebUI;
using game.Infrastructure.Extensions;

namespace game.AdminSystem.Controllers.Money
{
    public class PaymentPlatformSettingController : BaseController
    {
        [HttpGet]
        public ActionResult UpdatePriority(int platformID , int gwID , int typeEnumID , int payTypeID)
        {
            var queryPlatforms = SiteHelper.GetPlatformIDs();

            CheckPlatformPermissionExistOrRedirect(platformID);
            
            var queryAll = SiteHelper.GetAPIResult<List<PayTypeManagementAmountListModel>>($"/Payment/GetPayTypeManagementAmountList?platformID={platformID}");

            var queryExistOne = queryAll.FirstOrDefault(x => x.GWID == gwID && x.PayTypeID == payTypeID);

            var querySamePayTypes = queryAll.Where(x => x.TypeID == typeEnumID &&  x.IsRange == queryExistOne.IsRange).ToList();
            
            var viewModel = new PaymentPlatformSettingViewModel
            {
                PlatformID = platformID,
                SameTypePaymentGateways = querySamePayTypes,
                CurrentEditPaymentGateway = queryExistOne
            };

            return View(viewModel);
        }



        [HttpGet]
        public ActionResult UpdateAllPriority(int platformID, int gwID)
        {
            if (CheckCurrentPageAccessPermissionIsNotValid())
            {
                return PartialView(No_Permission_Page_View);
            }
            
            var queryPlatforms = SiteHelper.GetPlatformIDs();

            CheckPlatformPermissionExistOrRedirect(platformID);

            var queryAll = SiteHelper.GetAPIResult<List<PayTypeManagementAmountListModel>>($"/Payment/GetPayTypeManagementAmountList?platformID={platformID}");

            var querySamwGW = queryAll.Where(z => z.GWID == gwID).ToList();

            var viewModel = new PaymentPlatformSettingUpdateAllViewModel
            {
                PlatformID = platformID,
                GWID = gwID,
                GWName = querySamwGW.FirstOrDefault()?.GatewayName ?? string.Empty,
                SameTypePaymentGateways = querySamwGW,
            };

            return PartialView("UpdateAllPriority", viewModel);
        }



        [HttpPost]
        [NoAuthorizationAttribute]
        public ActionResult UpdatePriority(UpdatePlatformPaymentPriorityViewModel model)
        {
            var postModel = new UpdatePlatformSettingPriorityRequestModel
            {
                PayTypeID = model.PayTypeID,
                PlatformID = model.PlatformID,
                Priority = model.Priority
            };

            var result = SiteHelper.PostGameApi<APIResult>($"/PaymentOthers/UpdatePlatformSettingPriority" , postModel);
            return Alert(result.ResultObject.Message);
        }


        [HttpPost]
        [NoAuthorizationAttribute]
        public ActionResult UpdateAllPriority(UpdatePlatformPaymentPriorityViewModel model)
        {
            if (model == null)
                return Alert("失敗，參數不足");

            var queryAll = SiteHelper.GetAPIResult<List<PayTypeManagementAmountListModel>>($"/Payment/GetPayTypeManagementAmountList?platformID={model.PlatformID}");

            var querySamwGW = queryAll.Where(z => z.GWID == model.GWID).ToList();
            
            if (querySamwGW.IsEmptyOrNull())
                return Alert("无资料可供更新");

            List<APIResult> list = new List<APIResult>();

            //這功能應該不常使用 就暫時迴圈呼叫吧
            foreach(var item in querySamwGW)
            {
                var postModel = new UpdatePlatformSettingPriorityRequestModel
                {
                    PayTypeID = item.PayTypeID,
                    PlatformID = model.PlatformID,
                    Priority = model.Priority
                };

                var result = SiteHelper.PostGameApi<APIResult>($"/PaymentOthers/UpdatePlatformSettingPriority", postModel)?.ResultObject;
                list.Add(result);
            }

            var successCount = list.Where(x => x.Result).Count();
            var failCount = list.Where(x => !x.Result).Count();

            string msg = $"更新結束, 共{successCount}笔资料更新成功。";

            if(failCount > 0)
            {
                msg += $"共{failCount}笔资料更新失败";
            }

            return Alert(msg);
        }

    }
}

