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
    public class PromotionEventController : BaseController
    {
        private string _uploadAP_Doamin = System.Configuration.ConfigurationManager.AppSettings["UploadApi_Internal_IP"];
        private string _imageUrlDoamin = System.Configuration.ConfigurationManager.AppSettings["ImageUrl"];


        public ActionResult EventList()
        {          
            return View();
        }

        
        /// <summary>
        /// 新增/編輯
        /// </summary>
        /// <returns></returns>
        public ActionResult Update(int eventId = 0)
        {
            bool isUpdate = eventId != 0;

            var updateModel = new PromotionEventUpdateModel();

            if (isUpdate)
            {
                var query = SiteHelper.GetAPIResult<PromotionEventEntity>($"PromotionEvent/GetEventById?id={eventId}");

                if (query == null)
                    return Json("查无资料", JsonRequestBehavior.AllowGet);

                updateModel = query.ReturnInjecteObjectNullable<PromotionEventUpdateModel>();

                //检查查看权限
                CheckPlatformPermissionExistOrRedirect(query.PlatformID);
            }

            updateModel.IsUpdatePage = isUpdate;
            return View(updateModel);
        }


        public ActionResult UpdateEventItem(int eventId , PromotionEventPositionTypeEnum type)
        {
            bool isUpdate = eventId != 0;

            var queryEvent = SiteHelper.GetAPIResult<PromotionEventEntity>($"PromotionEvent/GetEventById?id={eventId}");

            if (queryEvent == null)
                return Json("查无资料", JsonRequestBehavior.AllowGet);

            var query = SiteHelper.GetAPIResult<PromotionEventItemEntity>($"PromotionEvent/GetItemByUniqueKey?eventID={eventId}&type={type}");

            if (query == null)
                return Json("查无资料", JsonRequestBehavior.AllowGet);
                       
            ViewBag.TypeName = type.GetEnumDescription();


            var matchFirstArea = DataService.GetMyAreas().FirstOrDefault();
            var queryGameList = SiteHelper.GetAPIResult<List<AreaGameList>>($"Area/GetAreaGameList?areaID={matchFirstArea?.AreaID}");
            ViewBag.GameList = queryGameList;

            var viewModel = query.ReturnInjecteObject<PromotionEventItemUpdateModel>();
            viewModel.EventName = queryEvent.Title;

            return View(viewModel);
        }



        public ActionResult UpdateAreaMapping(int eventId)
        {
            var queryEvent = SiteHelper.GetAPIResult<PromotionEventEntity>($"PromotionEvent/GetEventById?id={eventId}");

            if (queryEvent == null)
                return Json("查无资料", JsonRequestBehavior.AllowGet);

            //检查查看权限
            CheckPlatformPermissionExistOrRedirect(queryEvent.PlatformID);


            var queryAreas = SiteHelper.GetAPIResult<List<Area>>($"Area/GetArea?platformId={queryEvent.PlatformID}");

            var queryExistSetting = SiteHelper.GetAPIResult<List<PromotionEventAreaMappingEntity>>($"PromotionEvent/GetAreaMappingByEventID?eventID={eventId}");

            var viewModels = new UpdateEventAreaMappingViewModel
            {
                CurrentSettings = queryExistSetting,
                CurrentPlatformAreas = queryAreas,
                EventID = eventId,
                EventName = queryEvent.Title
            };

            return View(viewModels);
        }


        [HttpPost]
        public ActionResult Update(PromotionEventUpdateModel model)
        {
            bool isUpdate = model.EventId != 0;

            var updateModel = new PromotionEventUpdateModel();

            var postDataModel = model.ReturnInjecteObjectNullable<PromotionEventEntity>();
            postDataModel.UpdateBy = GetCurrentUserName();

            if (isUpdate)
            {
                var result = SiteHelper.PostGameApi<CRUDResultModel>($"PromotionEvent/Update" , postDataModel);
                return AlertOrRedirectConfirm(result , "EventList");
            }
            else
            {
                var result = SiteHelper.PostGameApi<CRUDResultModel>($"PromotionEvent/Create", postDataModel);
                return AlertOrRedirectConfirm(result, "EventList");
            }
        }


        public ActionResult Delete(int eventId)
        {
            var query = SiteHelper.GetAPIResult<PromotionEventEntity>($"PromotionEvent/GetEventById?id={eventId}");

            if (query == null)
                return Alert("查無資料");

            //检查查看权限
            if(!IsPlatformPermissionExist(query.PlatformID))
            {
                return Go_No_Permission_Page();
            }

            CheckAreaPermissionExistOrRedirect(query.PlatformID);

            var update = new UpdateEditorRequestModel
            {
                TargetIds = new List<int> { eventId },
                UpdateBy = GetCurrentUserName()
            };

            var result = SiteHelper.PutGameApi<PromotionEventEntity>($"PromotionEvent/SetInvisible" , update);
            return Alert("删除成功！");
        }


        [HttpPost]
        public ActionResult AjaxDeleteMultiple(string eventIds)
        {
            if (string.IsNullOrWhiteSpace(eventIds))
                return Json(new CRUDResultModel("eventIds 参数未填写"), JsonRequestBehavior.AllowGet);

            List<int> eventIDs = eventIds.SeperateToListIntOrDefaultValue();
            
            var update = new UpdateEditorRequestModel
            {
                TargetIds = eventIDs,
                UpdateBy = GetCurrentUserName()
            };

            var result = SiteHelper.PutGameApi<PromotionEventEntity>($"PromotionEvent/SetInvisible", update);

            string msg = $"成功！共删除 {eventIDs.Count()} 笔资料";
            return Json(new CRUDResultModel(true , msg), JsonRequestBehavior.AllowGet);
        }


        [ValidateInput(false)]
        [HttpPost]
        public ActionResult UpdateEventItem(PromotionEventItemUpdateModel model)
        {
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

            PromotionEventItemEntity updateModel = model.ReturnInjecteObject<PromotionEventItemEntity>();

            //如果有選擇上傳圖片，才會覆蓋檔案
            if (uploadResult != null)
            {
                updateModel.ImageUrlDomain = _imageUrlDoamin.ReplaceLastWordIfMatch('/');
                updateModel.ImageUrl = uploadResult.RelativePath;
            }

            var result = SiteHelper.PostGameApi<CRUDResultModel>($"PromotionEvent/UpdateEventItem", updateModel);

            if (result == null ||
               !result.ResultObject.IsSuccess)
            {
                return Alert(result.ResultObject.Message);
            }

            //更新編輯者、編輯時間 (因為是記在另一張table)
            SiteHelper.PostGameApi<CRUDResultModel>($"PromotionEvent/UpdateEditor", new UpdateEditorRequestModel
            {
                TargetId = model.EventId,
                UpdateBy = GetCurrentUserName()
            });

            return ConfirmReturnList("EventList");
        }



        [HttpPost]
        [NoAuthorization]
        public ActionResult UpdateAreaMapping(int eventId , string selectAreaIDs)
        {
            var queryEvent = SiteHelper.GetAPIResult<PromotionEventEntity>($"PromotionEvent/GetEventById?id={eventId}");

            if (queryEvent == null)
                return Json("查无资料", JsonRequestBehavior.AllowGet);

            //检查查看权限
            CheckPlatformPermissionExistOrRedirect(queryEvent.PlatformID);
            
            List<int> areaIDs = selectAreaIDs.SeperateToListIntOrDefaultValue();
            var postModel = new UpdatePromotionEventAreaMappingRequestModel
            {
                AreaIDs = areaIDs,
                EventID = eventId
            };
            
            string url = $"PromotionEvent/UpdateAreaMapping";
            var apiResult = SiteHelper.PostGameApi<CRUDResultModel>(url, postModel);


            if (apiResult.ResultObject.IsSuccess)
                return Alert(apiResult.ResultObject.Message);

            return ConfirmReturnList("EventList");
        }



        [HttpPost]
        public JsonResult UpdateImageToRemote()
        {
            ImageUploadResultViewModel uploadResult = null;

            var jsonResult = new CKEditorUploadResultViewModel
            {
                uploaded = false,
                url = string.Empty
            };
            
            foreach (string file in Request.Files)
            {
                HttpPostedFileBase f = Request.Files[file] as HttpPostedFileBase;

                if (f.ContentLength == 0)
                    continue;

                uploadResult = UploadFileToRemoteServer(f);

                if (!uploadResult.IsSuccess || string.IsNullOrWhiteSpace(uploadResult.RelativePath))
                {
                    return Json(jsonResult, JsonRequestBehavior.AllowGet);
                }
            }

            jsonResult.uploaded = true;
            jsonResult.url = _imageUrlDoamin.ReplaceLastWordIfMatch('/') + uploadResult.RelativePath;
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }


        [NoAuthorization]
        public ActionResult AjaxGetEventList(QueryPromotionEventRequestModel model)
        {
            var myPlatforms = DataService.GetMyPlatforms().Select(x => x.Id).ToList();

            //如果當前使用者沒有所屬平台 看不到東西
            if(myPlatforms.IsEmptyOrNull())            
                return ConvertToDataTableJsonResult(model.Draw, new PagedList<PromotionEventListPageViewModel>());

            string filterPlatformIds = string.Join(",", myPlatforms);

            if (model.PlatformID.HasValue)
                filterPlatformIds = model.PlatformID.Value.ToString();

            string url = $"PromotionEvent/GetFullInfoPagingByFilter?Keyword={model.Keyword.UrlEncode()}&PlatformIDsText={filterPlatformIds}&status={model.Status}&AreaIDsText={model.AreaID}&{SiteHelper.GeneratePageQueryString(model)}";

            var query = SiteHelper.GetAPIResult<PagedList<PromotionEventFullInfoViewModel>>(url);

            PagedList<PromotionEventListPageViewModel> pagingViewModel = ConvertToViewModels(query);
            return ConvertToDataTableJsonResult(model.Draw , pagingViewModel);
        }
        

        public PagedList<PromotionEventListPageViewModel> ConvertToViewModels(PagedList<PromotionEventFullInfoViewModel> queryApis)
        {
            var queryPlatforms = DataService.GetAllPlatform();
            var convert = queryApis.ReturnInjecteObject<PagedList<PromotionEventListPageViewModel>>();

            convert.PageData = queryApis.PageData.Select(z =>
            {
                var convertModel = z.ReturnInjecteObject<PromotionEventListPageViewModel>();

                convertModel.PlatformName = queryPlatforms.FirstOrDefault(x => x.Id == z.PlatformID)?.Name;
                convertModel.StatusText = ((NormalActiveStatusEnum)z.Status).GetEnumDescription();
                convertModel.EventStartTime = z.EventStartTime.ToString("yyyy-MM-dd HH:mm:ss");
                convertModel.EventEndTime = z.EventEndTime.ToString("yyyy-MM-dd HH:mm:ss");
                convertModel.CreateTime = z.CreateTime.ToString("yyyy-MM-dd HH:mm:ss");
                convertModel.UpdateTime = z.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss");
                convertModel.IsHomePageVisible = z.EventItems.Any(x => x.Type == (int)PromotionEventPositionTypeEnum.HomePage && x.Status == (int)NormalActiveStatusEnum.Enable) ? "v" : "";
                convertModel.IsAnnounementPageVisible = z.EventItems.Any(x => x.Type == (int)PromotionEventPositionTypeEnum.MailPage && x.Status == (int)NormalActiveStatusEnum.Enable) ? "v" : "";

                return convertModel;
            })
            .ToList();

            return convert;
        }


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



    }
}