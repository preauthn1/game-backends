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
using game.AdminSystem.Models.ViewModels;
using game.Entity.Event;
using System.Web;
using example.invalid;
using game.Infrastructure.Utility.ThirdParty;
using game.Entity.Game;

namespace game.AdminSystem.Controllers.FinanceSetting
{
    public class UnionpayBankCodeController : BaseController
    {

        public ActionResult List()
        {          
            return View();
        }

        /// <summary>
        /// 新增/編輯
        /// </summary>
        public ActionResult Update(int targetID = 0)
        {
            bool isUpdate = targetID != 0;

            var updateModel = new UnionpayBankCodeUpdateViewModel();

            if (isUpdate)
            {
                var query = SiteHelper.GetAPIResult<UnionpayBankCodeEntity>($"UnionpayBankCode/GetByID?id={targetID}");

                if (query == null)
                    return Json("查无资料", JsonRequestBehavior.AllowGet);

                updateModel = query.ReturnInjecteObjectNullable<UnionpayBankCodeUpdateViewModel>();

            }

            updateModel.IsUpdatePage = isUpdate;
            return View(updateModel);
        }



        [HttpPost]
        public ActionResult Update(UnionpayBankCodeUpdateViewModel model)
        {
            bool isUpdate = model.Id > 0;

            var updateModel = new UnionpayBankCodeEntity();
            var postDataModel = model.ReturnInjecteObjectNullable<UnionpayBankCodeEntity>();
            postDataModel.UpdateBy = GetCurrentUserName();
            postDataModel.Show = Convert.ToBoolean(model.Status);

            if (isUpdate)
            {
                var result = SiteHelper.PostGameApi<CRUDResultModel>($"UnionpayBankCode/Edit" , postDataModel);
                return AlertOrRedirectConfirm(result , "List");
            }
            else
            {
                var result = SiteHelper.PostGameApi<CRUDResultModel>($"UnionpayBankCode/Create", postDataModel);
                return AlertOrRedirectConfirm(result, "List");
            }
        }


        [NoAuthorization]
        public ActionResult GetList(QueryUnionpayBankCodeJQueryRequestModel filter)
        {
            string url = $"UnionpayBankCode/GetPagingByFilter?{SiteHelper.GeneratePageQueryString(filter)}&{SiteHelper.GenerateQueryStringParameter(filter)}";

            var query = SiteHelper.GetAPIResult<PagedList<UnionpayBankCodeEntity>>(url);

            PagedList<UnionpayBankCodeDisplayViewModel> pagingViewModel = ConvertToViewModels(query);
            return ConvertToDataTableJsonResult(filter.Draw , pagingViewModel);
        }
        

        public PagedList<UnionpayBankCodeDisplayViewModel> ConvertToViewModels(PagedList<UnionpayBankCodeEntity> queryApis)
        {
            var convert = queryApis.ReturnInjecteObject<PagedList<UnionpayBankCodeDisplayViewModel>>();

            convert.PageData = queryApis.PageData.Select(z =>
            {
                var convertModel = z.ReturnInjecteObject<UnionpayBankCodeDisplayViewModel>();
                convertModel.IsShowText = z.Show ? "v" : "";
                convertModel.CreateTimeText = z.CreateTime.ToString("yyyy-MM-dd HH:mm:ss");
                convertModel.UpdateTimeText = z.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss");
                return convertModel;
            })
            .ToList();

            return convert;
        }


    }
}