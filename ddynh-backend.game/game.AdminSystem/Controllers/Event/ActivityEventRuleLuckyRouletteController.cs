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

namespace game.AdminSystem.Controllers.Event
{
    public partial class ActivityEventRuleController : BaseController
    {
        private string _uploadAP_Doamin = System.Configuration.ConfigurationManager.AppSettings["UploadApi_Internal_IP"];
        private string _imageUrlDoamin = System.Configuration.ConfigurationManager.AppSettings["ImageUrl"];

        private ImageUploadResultViewModel UploadFileToRemoteServer(HttpPostedFileBase uploadFile)
        {
            string extension = Path.GetExtension(uploadFile.FileName).Replace(".", "");

            string fileName = $"{Guid.NewGuid()}.{extension}";

            string contentType = $"image/{extension}";

            string apiUrl = $"{_uploadAP_Doamin}/Upload/PostImageToMarketingFolder".ReplaceDuplicateUrlChar();

            //请求website的某支API 上传图片
            string resultText = ServerToServerUploadHelper.UploadFileToRemoteServer(apiUrl, fileName, contentType, uploadFile.InputStream);

            ImageUploadResultViewModel uploadResult = resultText.TryJsonConvertToModel<ImageUploadResultViewModel>();

            if (uploadResult == null)
            {
                return new ImageUploadResultViewModel
                {
                    IsSuccess = false,
                    DebugInfo = resultText
                };
            }

            return uploadResult;
        }

        [NoAuthorization]
        [HttpPost]
        public ActionResult UpdateAwardImage(FormCollection form)
        {
            var awardID = Convert.ToInt32(form["awardID"].ToString());
            var eventSettingID = Convert.ToInt32(form["eventSettingID"].ToString());

            ImageUploadResultViewModel uploadResult = null;

            foreach (string file in Request.Files)
            {
                HttpPostedFileBase f = Request.Files[file] as HttpPostedFileBase;

                if (f.ContentLength == 0)
                    continue;

                uploadResult = UploadFileToRemoteServer(f);

                if (!uploadResult.IsSuccess || string.IsNullOrWhiteSpace(uploadResult.RelativePath))
                {
                    return Alert($"上传失败 ({uploadResult.DebugInfo})");
                }
            }

            //如果有選擇上傳圖片，才會覆蓋檔案

            var ImageUrlDomain = string.Empty;
            var ImageUrl = string.Empty;
            if (uploadResult != null)
            {
                ImageUrlDomain = _imageUrlDoamin.ReplaceLastWordIfMatch('/');
                ImageUrl = uploadResult.RelativePath;
            }

            string apiUrl = "ActivityEventRules/UpdateAwardImage";
            var awardItems = new List<EventSettingAwardItemEntity>();
            awardItems.Add(
                new EventSettingAwardItemEntity
                {
                    Id = awardID,
                    EventSettingID = eventSettingID,
                    ImageUrlDomain = ImageUrlDomain.ToString(),
                    ImageUrl = ImageUrl.ToString(),
                }
            );
            var postBody = new ModifyEventLuckyRouletteRequestModel
            {
                EventSettingID = eventSettingID,
                UpdateBy = GetCurrentUserName(),
                AwardItems = awardItems
            };

            var result = SiteHelper.PostGameApi<CRUDResultModel>(apiUrl, postBody);
            return Json(result.ResultObject, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        /// <summary>
        /// 幸運輪盤
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult LuckyRoulette(int id, EventSettingEntity queryEvent, List<EventSettingRuleItemEntity> queryEventRules)
        {
            string urlAward = $"/ActivityEventRules/GetEventAwardBySettingID?settingID={id}";
            string urlLotteryConditions = $"/ActivityEventRules/GetEventLotteryConditionsBySettingID?settingID={id}";

            var queryEventAward = SiteHelper.GetAPIResult<List<EventSettingAwardItemEntity>>(urlAward);
            var queryEventLotteryConditions = SiteHelper.GetAPIResult<List<EventSettingLotteryConditionsItemEntity>>(urlLotteryConditions);

            var setting = new EventSettingViewModel
            {
                 Memo = queryEvent.Memo,
                 Tips = queryEvent.Tips
            };

            List<EventSettingRuleViewModel> ruleItems = queryEventRules.Select(z =>
            {
                var ruleType = (EventRuleTypeEnum)z.RuleType;
                bool isToCashAttr = ruleType.IsAttributeExist<ToCashAttribute>();
                EventRuleTypeEnum ruleTypeEnum = (EventRuleTypeEnum)z.RuleType;

                var convertModel = new EventSettingRuleViewModel
                {
                    BonusGroup = z.BonusGroup,
                    TableExistID = z.Id,
                    DisplayName = ruleType.GetEnumDescription() + ruleType.GetDescription(),
                    RuleType = ruleType,
                    Value = ToCashOrNot(ruleTypeEnum, z.RuleValue).ToString(),
                    ValueType = GetValueFormat(z.RuleType),
                    IsDisabled = IsDisabledColumns(queryEvent.EventType, ruleTypeEnum)
                };

                if (convertModel.ValueType == ValueFormatRestrictionEnum.Boolean)
                    convertModel.Value = (z.RuleValue == 1 ? true : false);

                return convertModel;
            })
            .ToList();

            List<EventSettingAwardViewModel> awardItems = queryEventAward.Select(x => new EventSettingAwardViewModel
            {
                DisplayName = x.AwardName,
                AwardType = x.AwardType,
                Amount = x.AwardAmount / SystemConst.CashAmountRate,
                AwardLimit = x.AwardLimit,
                AwardRate = x.AwardRate,
                TableExistID = x.Id,
                ImageUrlDomain = x.ImageUrlDomain,
                ImageUrl = x.ImageUrl,
                AwardUsed = x.AwardUsed
            })
            .ToList();

            List<EventSettingLotteryConditionsViewModel> lotteryConditionsItems = queryEventLotteryConditions.Select(y => new EventSettingLotteryConditionsViewModel
            {
                TableExistID = y.Id,
                LotteryAmount = y.LotteryAmount / SystemConst.CashAmountRate,
                LotteryCount = y.LotteryCount
            })
            .ToList();

            NormalActiveStatusEnum statusEnum = (NormalActiveStatusEnum)queryEvent.Status;

            var viewModel = new EventSettingLuckyRouletteSettingUpdateModel
            {
                IsNotAllowEdit = queryEvent.Status == (int)NormalActiveStatusEnum.PermanentInvisible,
                EventSettingID = queryEvent.ID,
                EventName = queryEvent.EventName,
                EventStatusHintText = statusEnum == NormalActiveStatusEnum.Enable ? "" : statusEnum.GetEnumDescription(),
                EventType = (ApplyEventTypeEnum)queryEvent.EventType,
                EventTypeName = ((ApplyEventTypeEnum)queryEvent.EventType).GetEnumDescription(),
                Memo = queryEvent.Memo,
                Tips = queryEvent.Tips,
                RuleItems = ruleItems,
                AwardItems = awardItems,
                LotteryConditionsItems = lotteryConditionsItems
            };

            return View("EditLuckyRoulette", viewModel);
        }

        [HttpPost]
        [NoAuthorization]
        public ActionResult EditLuckyRoulette(EventSettingLuckyRouletteSettingUpdateModel model)
        {
            string apiUrl = "ActivityEventRules/UpdateEventLuckyRouletteRules";

            var postBody = new ModifyEventLuckyRouletteRequestModel
            {
                EventSettingID = model.EventSettingID,
                UpdateBy = GetCurrentUserName(),
                Memo = model.Memo,
                Tips = model.Tips,
                AwardItems = model.AwardItems.Select(x => new EventSettingAwardItemEntity
                {
                    Id = x.TableExistID,
                    EventSettingID = model.EventSettingID,
                    AwardName = x.DisplayName,
                    AwardAmount = x.Amount * SystemConst.CashAmountRate,
                    AwardType = x.AwardType,
                    AwardRate = x.AwardRate,
                    AwardLimit = x.AwardLimit,
                    ImageUrlDomain = x.ImageUrlDomain,
                    ImageUrl = x.ImageUrl
                })
                .ToList(),

                LotteryItems = model.LotteryConditionsItems.Select(z => new EventSettingLotteryConditionsItemEntity
                {
                    Id = z.TableExistID,
                    EventSettingID = model.EventSettingID,
                    LotteryAmount = z.LotteryAmount * SystemConst.CashAmountRate,
                    LotteryCount = z.LotteryCount
                })
                .ToList(),

                RuleItems = model.RuleItems.Select(y => new EventSettingRuleItemEntity
                {
                    BonusGroup = y.BonusGroup,
                    Id = y.TableExistID,
                    EventSettingID = model.EventSettingID,
                    RuleType = (int)y.RuleType,
                    RuleValue = IsToCashFormat(y.RuleType) ? Int64.Parse(y.Value.ToString()) * SystemConst.CashAmountRate :
                                GetValueFormat((int)y.RuleType) == ValueFormatRestrictionEnum.Boolean ? BooleanTextToInt(y.Value) : Int64.Parse(y.Value.ToString()),
                })
                .ToList()
            };

            var result = SiteHelper.PostGameApi<CRUDResultModel>(apiUrl, postBody);
            return Json(result.ResultObject, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [NoAuthorization]
        public ActionResult AddAwardRow(EventSettingAwardItemEntity model)
        {
            string apiUrl = "ActivityEventRules/AddAwardRow";
            if (string.IsNullOrEmpty(model.AwardName)) model.AwardName = "金币";
            var result = SiteHelper.PostGameApi<CRUDResultModel>(apiUrl, model);
            return Json(result.ResultObject, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [NoAuthorization]
        public ActionResult DeleteAwardRow(int id)
        {
            string apiUrl = $"ActivityEventRules/DeleteAwardRow?awardID={id}";
            var result = SiteHelper.GetAPIResult<APIResult>(apiUrl);
            return Json(new AjaxResult()
            {
                success = result == null ? false : true,
                data = result
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [NoAuthorization]
        public ActionResult AddLotteryRow(EventSettingLotteryConditionsItemEntity model)
        {
            string apiUrl = "ActivityEventRules/AddLotteryRow";
            var result = SiteHelper.PostGameApi<CRUDResultModel>(apiUrl, model);
            return Json(result.ResultObject, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [NoAuthorization]
        public ActionResult DeleteLotteryRow(int id)
        {
            string apiUrl = $"ActivityEventRules/DeleteLotteryRow?lotteryID={id}";
            var result = SiteHelper.GetAPIResult<APIResult>(apiUrl);
            return Json(new AjaxResult()
            {
                success = result == null ? false : true,
                data = result
            }, JsonRequestBehavior.AllowGet);
        }

    }
}