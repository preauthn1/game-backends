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
using game.Entity.Payment;
using game.Entity.Pay;

namespace game.AdminSystem.Controllers.FinanceSetting
{
    public class PayGatewayController : BaseController
    {

        /// <summary>
        /// 新增/編輯
        /// </summary>
        public ActionResult Update(int targetID = 0)
        {
            bool isUpdate = targetID != 0;

            var updateViewModel = new PayGatewayAdminUpdateViewModel();

            if (isUpdate)
            {
                var query = SiteHelper.GetAPIResult<PayGatewayEntity>($"PayGateway/GetByID?id={targetID}");

                var queryConvertTypeSetting = SiteHelper.GetAPIResult<List<PayGatewayConvertTypeMappingEntity>>($"PayGatewayConvertTypeMapping/GetByFilter?GWIDsText={targetID}");

                if (query == null)
                    return Json("查无资料", JsonRequestBehavior.AllowGet);

                updateViewModel = query.ReturnInjecteObjectNullable<PayGatewayAdminUpdateViewModel>();
                updateViewModel.IsSupportt_Alipay = queryConvertTypeSetting.Any(z => z.ConvertType == (int)ConvertTypeEnum.Alipay);
                updateViewModel.IsSupportt_Unionpay = queryConvertTypeSetting.Any(z => z.ConvertType == (int)ConvertTypeEnum.Unionpay);
            }

            updateViewModel.IsUpdatePage = isUpdate;
            return View(updateViewModel);
        }



        [HttpPost]
        public ActionResult Update(PayGatewayAdminUpdateViewModel model)
        {
            bool isUpdate = model.ID != 0;

            var updateModel = new PayGatewayEntity();

            var postDataModel = model.ReturnInjecteObjectNullable<PayGatewayEntity>();
            postDataModel.UpdateBy = GetCurrentUserName();

            if (isUpdate)
            {
                //Step 1. 编辑基本资讯
                var result1 = SiteHelper.PostGameApi<CRUDResultModel>($"PayGateway/Edit", postDataModel);

                if(!result1.ResultObject.IsSuccess)
                    return Alert(result1.ResultObject.Message);

                PayGatewayConvertTypeMappingUpdateRequestModel updateMappingModel = InstanceUpdateMappingModel(model);

                //Step 2. 编辑mappingConvertType
                var result2 = SiteHelper.PostGameApi<CRUDResultModel>($"PayGatewayConvertTypeMapping/EditAndDelete", updateMappingModel);
                return Alert(result2.ResultObject.Message);
            }
            else
            {
                var result = SiteHelper.PostGameApi<CRUDResultModel>($"PayGateway/Create", postDataModel);
                return Alert(result.ResultObject.Message);
            }
        }


        /// <summary>
        /// 修改可支援银行
        /// </summary>
        public ActionResult UpdateBanks(int GWID)
        {
            DisableUniformCheckbox();

            var queryGW = SiteHelper.GetAPIResult<PayGatewayEntity>($"/PayGateway/GetByID?id={GWID}");

            if (queryGW == null)
                return Alert("查无资料");

            string url = $"ConvertGatewayBankSetting/GetByFilter?GWIDsText={GWID}";

            var querySettings = SiteHelper.GetAPIResult<List<ConvertGatewayBankSettingEntity>>(url);

            var queryAllBanks = SiteHelper.GetAPIResult<List<UnionpayBankCodeEntity>>($"UnionpayBankCode/GetByFilter");

            var convertToSelected = queryAllBanks.Select(z =>
            {
                var matchSetting = querySettings.FirstOrDefault(x => x.UnionpayBankCodeID == z.Id);
                var convertViewModel = new ConvertGatewayUpdateBankSettingsItemsViewModel
                {
                    BankName = z.BankName,
                    ConvertGatewayID = GWID,
                    UnionpayBankCodeID = z.Id,
                };

                if (matchSetting != null)
                {
                    convertViewModel.IsChecked = matchSetting.Status == (int)NormalActiveStatusEnum.Enable;
                    convertViewModel.ThirdPartyBankCode = matchSetting.ThirdPartyBankCode;
                }

                return convertViewModel;
            })
            .OrderBy(x => x.BankName)
            .ToList();

            var updateModel = new ConvertGatewayUpdateBankSettingsViewModel
            {
                GWID = GWID,
                GWName = queryGW.GatewayName,
                SettingItems = convertToSelected
            };

            return View(updateModel);
        }



        /// <summary>
        /// 修改可支援银行
        /// </summary>
        [NoAuthorization]
        [HttpPost]
        public ActionResult UpdateBanks(ConvertGatewayUpdateBankSettingsViewModel model)
        {
            var updateModel = new ConvertGatewayBankSettingViewModel
            {
                GWID = model.GWID,
                SettingItems = model.SettingItems.Select(x => new ConvertGatewayBankSettingEntity
                {
                    Status = (int)(x.IsChecked ? NormalActiveStatusEnum.Enable : NormalActiveStatusEnum.Disable),
                    ConvertGatewayID = x.ConvertGatewayID,
                    UnionpayBankCodeID = x.UnionpayBankCodeID,
                    ThirdPartyBankCode = x.ThirdPartyBankCode
                })
                .ToList()
            };

            var result = SiteHelper.PostGameApi<CRUDResultModel>($"ConvertGatewayBankSetting/Edit", updateModel);
            return Json(result.ResultObject, JsonRequestBehavior.AllowGet);
        }


        private PayGatewayConvertTypeMappingUpdateRequestModel InstanceUpdateMappingModel(PayGatewayAdminUpdateViewModel model)
        {

            var updateMappingModel = new PayGatewayConvertTypeMappingUpdateRequestModel
            {
                GWID = model.ID,
                Items = new List<PayGatewayConvertTypeMappingEntity>()
            };


            if (model.IsSupportt_Alipay_CheckText == "on")
            {
                updateMappingModel.Items.Add(new PayGatewayConvertTypeMappingEntity
                {
                    ConvertType = ConvertTypeEnum.Alipay.ToInteger(),
                    GWID = model.ID
                });
            }

            if (model.IsSupportt_Unionpay_CheckText == "on")
            {
                updateMappingModel.Items.Add(new PayGatewayConvertTypeMappingEntity
                {
                    ConvertType = ConvertTypeEnum.Unionpay.ToInteger(),
                    GWID = model.ID
                });
            }

            return updateMappingModel;
        }




    }
}