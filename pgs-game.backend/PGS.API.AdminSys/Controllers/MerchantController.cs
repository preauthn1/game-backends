using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using System.Linq;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.Paging;
using target.Web.Modules.Filter;
using target.Models.Enums;
using target.Utility.Extensions;
using target.Models.Models;
using target.Models.CoreLibs;
using target.Utility.Helper;
using target.BL.Services;
using target.Models;
using target.Utility;
using target.Utility.IOC;

namespace target.API.AdminSys.Controllers
{
    [AdminTokenrAuthorizeFilter]
    public class MerchantController : BaseController
    {
        public IMerchantService MerchantService { get; set; }

        public IMerchantCustomizedSettingService MerchantCustomizedSettingService { get; set; }

        public IUsersService UsersService { get; set; }

        public IAdminSysComplexService AdminSysComplexService { get; set; }

        public IMerchantCreditService MerchantCreditService { get; set; }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingMerchant_Search)]
        [Route("merchants")]
        [HttpGet]
        public PagedList<MerchantViewModel> GetMerchantPagingByFilter([FromQuery] QueryMerchantApiRequestParameterModel filter)
        {
            var query = MerchantService.GetByPagingFilter(filter);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingMerchant_Search)]
        [Route("merchants/{merchantID}")]
        [HttpGet]
        public MerchantViewModel GetByID(int merchantID)
        {
            var query = MerchantService.GetByID(merchantID);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingMerchant_Search)]
        [Route("merchants/copy-info/{merchantID}")]
        [Route("merchants/copy-preview-info/{merchantID}")]
        [HttpGet]
        public MerchantCopyInfoPreviewViewModel GetCopyApiKeyPreviewInfo(int merchantID)
        {
            int userID = GetCurrentUserID();

            var query = MerchantService.GetCopyPreviewInfo(userID, merchantID);

            if (query.IsDepositCreditAfterProcess && example.invalid.WalletMode == (int)MerchantWalletModeEnum.TransferMode)
            {
                MerchantCreditService.UpdateCreditAmount(userID, new MerchantCreditUpdateRequestModel
                {
                    MerchantID = merchantID,
                    PlusType = PlusOrMinusEnum.Plus.ToInteger(),
                    Memo = "預覽商戶自動加值",
                    UpdateAmount = 900000000
                });
            }

            return query;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Agent_MerchantCredit_Search, AdminSysPermissionKey.SystemSettingMerchant_Search)]
        [Route("merchants/{merchantID}/merchant-status-and-hierarchy-preview")]
        [HttpGet]
        public MerchantStatusAndHierarchyPreviewInfoModel PreviewMerchantStatusAndHierarchyInfo(int merchantID)
        {
            int userID = GetCurrentUserID();
            var query = MerchantService.GetMerchantStatusAndHierarchyPreviewInfo(userID, merchantID);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingMerchant_Create)]
        [Route("merchants")]
        [HttpPost]
        public void Create(MerchantCreateRequestModel model)
        {
            int userID = GetCurrentUserID();
            MerchantService.Create(userID, model);
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingMerchant_Update)]
        [Route("merchants")]
        [HttpPut]
        public void Update(MerchantUpdateRequestModel model)
        {
            int userID = GetCurrentUserID();
            MerchantService.Update(userID, model);
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingMerchant_Update)]
        [Route("merchants/{targetID}")]
        [HttpDelete]
        public void Delete(int targetID, [FromBody] LazyCommonRequestModel model)
        {
            int userID = GetCurrentUserID();
            MerchantService.Delete(userID, targetID, model?.Memo);
        }


        [Route("merchants/create-available-settings")]
        [HttpGet]
        public List<SystemConfigSettingViewModel> GetAvailableMerchantCustomizedSettingsWhenCreateMerchant()
        {
            int userID = GetCurrentUserID();
            var result = MerchantCustomizedSettingService.GetAvailableMerchantCustomizedSettingsWhenCreateMerchant(userID);
            return result;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Agent_MerchantCredit_TriggerSwValidateTest)]
        [Route("merchants/sw_test")]
        [HttpPost]
        public object TriggerSwTest([FromBody] BackendDefineButtonActionRequestModel model)
        {
            model.RuleFor(x => x.ExtraParameter).Rule_ModelNotNull();
            model.MaxTimesPerHour = 10;
            model.IsOperateByCSOrAM = true;

            var param = model.ExtraParameter;
            param.PlayerAccount = param.Text;

            param.RuleFor(x => x.PlayerAccount).Rule_NotEmptyText();

            var service = IOCHelper.Resolve<IBackendButtonsService>();

            int userID = GetCurrentUserID();
            BackendButtonResultModel result = service.ExecuteDefineAction(userID, model);

            if (result.FileResult != null)
            {
                string brandName = SettingHelper.PromoteBrandCodeName;
                string fileName = string.Format("{0}.xlsx", result.FileResult.FileName);
                HttpContext.Response.Headers.Add(SystemConstant.PgsFileHeaderBase64FileName, fileName.Base64Encode());
                return File(result.FileResult.FileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName); // returns a FileStreamResult
            }
            else
            {
                return result;
            }
        }

        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingMerchant , AdminSysPermissionKey.Agent_MerchantCredit)]
        [Route("v2/merchants/{merchantID}/advance-setting")]
        [HttpGet]
        public SystemConfigSettingPageViewModel GetMerchantAdvanceSettings_v2(int merchantID, [FromQuery] MerchantCustomizedSettingQueryParameter extraParam)
        {
            int userID = GetCurrentUserID();
            var result = MerchantCustomizedSettingService.GetMerchantAdvanceSettingsBySpecificKey(userID, merchantID, extraParam);
            return result;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingMerchant, AdminSysPermissionKey.Agent_MerchantCredit)]
        [Route("v2/merchants/advance-setting")]
        [HttpPost]
        public void UpdateMerchantAdvanceSettings_v2(SystemConfigSettingUpdateRequestModel model)
        {
            int userID = GetCurrentUserID();
            MerchantCustomizedSettingService.UpdateSettings(userID, model);
            return;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.System_Merchant_CustomizedSettingList)]
        [Route("v2/merchants/advance-setting/batch-update")]
        [HttpPost]
        public CRUDResultModel BatchUpdateMerchantAdvanceSettings(MerchantBatchUpdateSettingRequestModel model)
        {
            int userID = GetCurrentUserID();
            var result = MerchantCustomizedSettingService.BatchUpdateMerchantAdvanceSettings(userID, model);
            return result;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingMerchant_Create)]
        [Route("merchants/compare")]
        [HttpPost]
        public MerchantCompareResultViewModel CompareTwoMerchants(MerchantCompareRequestModel model)
        {
            int userID = GetCurrentUserID();
            var result = MerchantService.CompareTwoMerchants(userID , model);
            return result;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingMerchant_Create)]
        [Route("merchants/list-all-settings")]
        [HttpGet]
        public MerchantAdvanceListAllViewModel ListAllMerchantCustomizedSettings([FromQuery] MerchantAdvanceListRequestModel filter)
        {
            int userID = GetCurrentUserID();
            var result = MerchantService.ListAllMerchantCustomizedSettings(userID, filter);
            return result;
        }


        /// <summary>
        /// 技術端手動使用 (特定品牌有N+個貨幣商戶要創建時使用)
        /// </summary>
        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingMerchant_Create)]
        [Route("merchants/batch-copy-by-each-parents-and-prefixName")]
        [HttpPost]
        public string BatchCreateNewMerchantByEachParentAndPrefixName([FromBody] MerchantBatchCopyAndCreateRequestModel model)
        {
            int userID = GetCurrentUserID();
            string resultMsg = AdminSysComplexService.BatchCreateNewMerchantByEachParentAndPrefixName(userID, model);
            return resultMsg;
        }


        /// <summary>
        /// 給代理用 創建dev商戶
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Agent_MerchantCredit_CreateDevMerchant)]
        [Route("merchants/dev-merchant-create")]
        [HttpPost]
        public CRUDResultModel CreateDevMerchantByAM([FromBody] LazyCommonRequestModel model)
        {
            int userID = GetCurrentUserID();
            var result = AdminSysComplexService.CreateDevMerchantByAM(userID, model);
            return result;
        }

        /// <summary>
        /// 只有內部人員會呼叫此API 等同於 商戶管理>複製商戶
        /// </summary>
        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingMerchant_Create)]
        [Route("merchants/batch-copy")]
        [HttpPost]
        public CRUDResultModel CopyAndBatchCreateNewMerchant([FromBody] MerchantBatchCopyAndCreateRequestModel model)
        {
            int userID = GetCurrentUserID();
            var result = AdminSysComplexService.CopyAndBatchCreateNewMerchant(userID, model);
            return result;
        }

        /// <summary>
        /// 代理操作-創建子代裡
        /// </summary>
        [PostHashHeaderValidateFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingMerchant_Create, AdminSysPermissionKey.Agent_MerchantCredit_CreateChildAgent)]
        [Route("merchants/copy-merchant-by-agent")]
        [HttpPost]
        public void CopyMerchantByAgent([FromBody] MerchantCopyByAgentRequestModel model)
        {
            int userID = GetCurrentUserID();
            AdminSysComplexService.CopyMerchantByAgent(userID, model);
        }


        /// <summary>
        /// 代理 或 商戶管理頁 都會進到這支API
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Agent_MerchantCredit_EditStatus)]
        [Route("merchants/edit-merchant-status-by-agent")]
        [HttpPut]
        public void UpdateMerchantStatusByAgent([FromBody] LazyCommonRequestModel model)
        {
            int userID = GetCurrentUserID();
            MerchantService.UpdateStatusByAgent(userID, model);
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_SystemConfigSetting)]
        [Route("merchants/extra-operate-by-type")]
        [HttpPost]
        public void DoExtraOperateForMerchant([FromBody] LazyCommonRequestModel model)
        {
            int userID = GetCurrentUserID();
            MerchantService.DoExtraOperateForMerchant(userID, model);
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingMerchant_Search)]
        [Route("merchants/parse-merchantIDs-by-rawText")]
        [HttpPost]
        public MerchantParseViewModel ParseMerchantIDsByRawText([FromBody] LazyCommonRequestModel model)
        {
            model.RuleFor(x => x.RawText).Rule_NotEmptyText();

            List<int> merchantIDs = AppHelper.AnalyzeIDsFromRawText(model.RawText);

            return new MerchantParseViewModel
            {
                TotalCount = merchantIDs.Count(),
                MerchantIDs = merchantIDs
            };
        }

    }
}
