using game.Entity.Game;
using game.Infrastructure;
using System;
using example.invalid;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace game.WebSite.Controllers
{
    public class UploadController : Controller
    {
        int MaxSizeBytes = 3 * 1024 * 1024; // max 3Mb


        public ActionResult PostImage()
        {
            //var r = new List<ViewDataUploadFilesResult>();

            foreach (string file in Request.Files)
            {
                HttpPostedFileBase f = Request.Files[file] as HttpPostedFileBase;
                if (f.ContentLength == 0 || f.ContentLength > MaxSizeBytes)
                    return Content("InvalidFileSize");

                if (!f.FileName.EndsWith("jpg") && !f.FileName.EndsWith("png"))
                    return Content("InvalidContentType");

                string savedFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Upload", "QuestionImage", Path.GetFileName(f.FileName));
                f.SaveAs(savedFileName);
            }
            return new EmptyResult();
        }

        [HttpPost]
        public ActionResult NewOrEditAdvertisement(FormCollection formCollection, HttpPostedFileBase file)
        {
            int id;
            var currentFileName = formCollection["Picture"];
            var uploadURL = formCollection["UploadUrl"];
            var dbUploadPath = string.Format("{0}/{1}/{2}", uploadURL, "Upload", "AdImage");
            var savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Upload", "AdImage");
            string fileName = null;
            string savedFileName = null;
            APIResult result = null;

            if (file != null)
            {
                fileName = string.Format("{0}_{1}", DateTime.Now.ToString("yyyyMMddHHmm"), file.FileName);
                savedFileName = Path.Combine(savePath, fileName);
            }

            var model = new Advertisement()
            {
                PlatformID = int.Parse(formCollection["PlatformID"]),
                Link = formCollection["Link"].Trim(),
                Behavior = (Behavior)Enum.Parse(typeof(Behavior), formCollection["Behavior"]),
                Status = (Status)Enum.Parse(typeof(Status), formCollection["Status"]),
                Picture = string.Format("{0}/{1}", dbUploadPath, fileName)
            };

            #region 更新

            if (int.TryParse(formCollection["Id"], out id))
            {
                //有新檔案要更新
                if (file != null && file.ContentLength != 0)
                {
                    var deleteFile = currentFileName.Replace(dbUploadPath, savePath).Replace("/", "\\");
                    this.deleteFile(deleteFile);

                    file.SaveAs(savedFileName);
                }
                else
                {
                    model.Picture = currentFileName;
                }

                model.Id = id;
                result = SiteHelper.PostAPIResult<Advertisement>("Area/UpdateAdvertisement", model);
                return Json(new { success = result.Result, msg = result.Message });
            }

            #endregion

            #region 新增

            //找不到資料夾新增
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            file.SaveAs(savedFileName);
            result = SiteHelper.PostAPIResult<Advertisement>("Area/AddAdvertisement", model);

            return Json(new { success = result.Result, msg = result.Message });

            #endregion
        }


        public ActionResult InsertAdvertisement(FormCollection formCollection, HttpPostedFileBase file)
        {
            var uploadURL = formCollection["UploadUrl"];
            var dbUploadPath = string.Format("{0}/{1}/{2}", uploadURL, "Upload", "AdImage");
            var savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Upload", "AdImage");
            string fileName = null;
            string savedFileName = null;

            if (file != null)
            {
                fileName = string.Format("{0}_{1}", DateTime.Now.ToString("yyyyMMddHHmm"), file.FileName);
                savedFileName = Path.Combine(savePath, fileName);
            }

            var platformID = int.Parse(formCollection["PlatformID"]);
            var areaID = int.Parse(formCollection["AreaID"]);
            var link = formCollection["Link"];

            var model = new Advertisement()
            {
                PlatformID = platformID,
                AreaID = areaID,
                Link = link.Trim(),
                Behavior = (Behavior)Enum.Parse(typeof(Behavior), formCollection["Behavior"]),
                Status = (Status)Enum.Parse(typeof(Status), formCollection["Status"]),
                Picture = string.Format("{0}/{1}", dbUploadPath, fileName)
            };

            #region 新增

            //找不到資料夾新增
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            file.SaveAs(savedFileName);
            var result = SiteHelper.PostAPIResult<Advertisement>("Area/AddAdvertisement", model);

            return Json(new { success = result.Result, msg = result.Message });

            #endregion
        }


        public ActionResult UpdateAdvertisement(FormCollection formCollection, HttpPostedFileBase file)
        {
            int id;
            var currentFileName = formCollection["Picture"];
            var uploadURL = formCollection["UploadUrl"];
            var dbUploadPath = string.Format("{0}/{1}/{2}", uploadURL, "Upload", "AdImage");
            var savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Upload", "AdImage");
            string fileName = null;
            string savedFileName = null;
            APIResult result = null;

            if (file != null)
            {
                fileName = string.Format("{0}_{1}", DateTime.Now.ToString("yyyyMMddHHmm"), file.FileName);
                savedFileName = Path.Combine(savePath, fileName);
            }

            var platformID = int.Parse(formCollection["PlatformID"]);
            var areaID = int.Parse(formCollection["AreaID"]);
            var link = formCollection["Link"];

            var model = new Advertisement()
            {
                PlatformID = int.Parse(formCollection["PlatformID"]),
                Link = formCollection["Link"].Trim(),
                Behavior = (Behavior)Enum.Parse(typeof(Behavior), formCollection["Behavior"]),
                Status = (Status)Enum.Parse(typeof(Status), formCollection["Status"]),
                Picture = string.Format("{0}/{1}", dbUploadPath, fileName)
            };


            if (file != null && file.ContentLength != 0)
            {
                var deleteFile = currentFileName.Replace(dbUploadPath, savePath).Replace("/", "\\");
                this.deleteFile(deleteFile);

                file.SaveAs(savedFileName);
            }
            else
            {
                model.Picture = currentFileName;
            }

            if (int.TryParse(formCollection["Id"], out id))
            {
                //有新檔案要更新
                if (file != null && file.ContentLength != 0)
                {
                    var deleteFile = currentFileName.Replace(dbUploadPath, savePath).Replace("/", "\\");
                    this.deleteFile(deleteFile);

                    file.SaveAs(savedFileName);
                }
                else
                {
                    model.Picture = currentFileName;
                }

                model.Id = id;
                result = SiteHelper.PostAPIResult<Advertisement>("Area/UpdateAdvertisement", model);

            }
            return Json(new { success = result.Result, msg = result.Message });

        }

        [HttpPost]
        public ActionResult DeleteAdvertisement(int adId, string picture, string uploadURL)
        {
            var savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Upload", "AdImage");
            var dbUploadPath = string.Format("{0}/{1}/{2}", uploadURL, "Upload", "AdImage");
            var deleteFile = picture.Replace(dbUploadPath, savePath).Replace("/", "\\");

            this.deleteFile(deleteFile);

            var result = SiteHelper.GetAPIResult<APIResult>(string.Format("Area/DeleteAdvertisement?id={0}", adId));

            return Json(new { success = result.Result, msg = result.Message }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult InsertCampaign(FormCollection formCollection, HttpPostedFileBase file)
        {
            var uploadURL = formCollection["UploadUrl"];
            var dbUploadPath = string.Format("{0}/{1}/{2}", uploadURL, "Upload", "CampaignImage");
            var savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Upload", "CampaignImage");
            string fileName = null;
            string savedFileName = null;

            if (file != null)
            {
                fileName = string.Format("{0}_{1}", DateTime.Now.ToString("yyyyMMddHHmm"), file.FileName);
                savedFileName = Path.Combine(savePath, fileName);
            }

            var platformID = int.Parse(formCollection["PlatformID"]);
            var areaID = int.Parse(formCollection["AreaID"]);
            var seqID = int.Parse(formCollection["SeqID"]);
            var link = formCollection["Link"];
            var title = formCollection["Title"];
            var starttime = formCollection["StartTime"];
            var endtime = formCollection["EndTime"];

            var model = new Campaign()
            {
                PlatformID = platformID,
                AreaID = areaID,
                SeqID = seqID,
                Link = link.Trim(),
                Title = title.Trim(),
                StartTime = Convert.ToDateTime(starttime),
                EndTime = Convert.ToDateTime(endtime),
                CampaignTag = (CampaignTag)Enum.Parse(typeof(CampaignTag), formCollection["CampaignTag"]),
                CampaignBehavior = (CampaignBehavior)Enum.Parse(typeof(CampaignBehavior), formCollection["CampaignBehavior"]),
                CampaignStatus = (CampaignStatus)Enum.Parse(typeof(CampaignStatus), formCollection["CampaignStatus"]),
                Picture = string.Format("{0}/{1}", dbUploadPath, fileName)
            };

            #region 新增

            //找不到資料夾新增
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            file.SaveAs(savedFileName);
            var result = SiteHelper.PostAPIResult<Campaign>("Area/AddCampaign", model);

            return Json(new { success = result.Result, msg = result.Message });

            #endregion
        }

        public ActionResult UpdateCampaign(FormCollection formCollection, HttpPostedFileBase file)
        {
            int id;
            var currentFileName = formCollection["Picture"];
            var uploadURL = formCollection["UploadUrl"];
            var dbUploadPath = string.Format("{0}/{1}/{2}", uploadURL, "Upload", "CampaignImage");
            var savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Upload", "CampaignImage");
            string fileName = null;
            string savedFileName = null;
            APIResult result = null;

            if (file != null)
            {
                fileName = string.Format("{0}_{1}", DateTime.Now.ToString("yyyyMMddHHmm"), file.FileName);
                savedFileName = Path.Combine(savePath, fileName);
            }

            var platformID = int.Parse(formCollection["PlatformID"]);
            var areaID = int.Parse(formCollection["AreaID"]);
            var seqID = int.Parse(formCollection["SeqID"]);
            var link = formCollection["Link"];
            var title = formCollection["Title"];
            var starttime = formCollection["StartTime"];
            var endtime = formCollection["EndTime"];

            var model = new Campaign()
            {
                PlatformID = platformID,
                AreaID = areaID,
                Link = link.Trim(),
                Title = title.Trim(),
                SeqID = seqID,
                StartTime = Convert.ToDateTime(starttime),
                EndTime = Convert.ToDateTime(endtime),
                CampaignTag = (CampaignTag)Enum.Parse(typeof(CampaignTag), formCollection["CampaignTag"]),
                CampaignBehavior = (CampaignBehavior)Enum.Parse(typeof(CampaignBehavior), formCollection["CampaignBehavior"]),
                CampaignStatus = (CampaignStatus)Enum.Parse(typeof(CampaignStatus), formCollection["CampaignStatus"]),
                Picture = string.Format("{0}/{1}", dbUploadPath, fileName)
            };


            if (file != null && file.ContentLength != 0)
            {
                var deleteFile = currentFileName.Replace(dbUploadPath, savePath).Replace("/", "\\");
                this.deleteFile(deleteFile);

                file.SaveAs(savedFileName);
            }
            else
            {
                model.Picture = currentFileName;
            }

            if (int.TryParse(formCollection["Id"], out id))
            {
                //有新檔案要更新
                if (file != null && file.ContentLength != 0)
                {
                    var deleteFile = currentFileName.Replace(dbUploadPath, savePath).Replace("/", "\\");
                    this.deleteFile(deleteFile);

                    file.SaveAs(savedFileName);
                }
                else
                {
                    model.Picture = currentFileName;
                }

                model.Id = id;
                result = SiteHelper.PostAPIResult<Campaign>("Area/UpdateCampaign", model);

            }
            return Json(new { success = result.Result, msg = result.Message });

        }

        [HttpPost]
        public ActionResult DeleteCampaign(int adId, string picture, string uploadURL)
        {
            var savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Upload", "CampaignImage");
            var dbUploadPath = string.Format("{0}/{1}/{2}", uploadURL, "Upload", "CampaignImage");
            var deleteFile = picture.Replace(dbUploadPath, savePath).Replace("/", "\\");

            this.deleteFile(deleteFile);

            var result = SiteHelper.GetAPIResult<APIResult>(string.Format("Area/DeleteCampaign?id={0}", adId));

            return Json(new { success = result.Result, msg = result.Message }, JsonRequestBehavior.AllowGet);
        }




        private void deleteFile(string path)
        {
            if (example.invalid.File.Exists(path)) { example.invalid.File.Delete(path); }
        }


        public ActionResult AdminUploadFile()
        {
            int MaxSizeBytes = 3 * 1024 * 1024; // max 3Mb
            foreach (string file in Request.Files)
            {
                HttpPostedFileBase f = Request.Files[file] as HttpPostedFileBase;

                if (f.ContentLength > MaxSizeBytes)
                    return Content("您的图片大小已超出，请小于3兆( " + f.ContentLength + " )");

                if (!f.FileName.EndsWith("jpg") && !f.FileName.EndsWith("jpeg") && !f.FileName.EndsWith("png"))
                    return Content("图片类型请使用jpg,jpeg,png ( " + f.FileName + " )");

                var guid = GetRandomStringByGuid(21);

                var updateFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Upload", "QuestionImage", DateTime.Now.ToString("yyyyMMdd"));

                if (!Directory.Exists(updateFile))
                {
                    Directory.CreateDirectory(updateFile);
                }

                string savedFileName = Path.Combine(updateFile, Path.GetFileName(guid + Path.GetExtension(Request.Files[0].FileName)));
                string savedFileName2 = Path.Combine(updateFile, Path.GetFileName("ORIG-" + guid + Path.GetExtension(Request.Files[0].FileName)));

                f.SaveAs(savedFileName2);

                var thumbImage = new WebImage(f.InputStream);
                thumbImage.Resize(144, 256, true, true);
                thumbImage.Save(savedFileName);

                var path = $"/Upload/QuestionImage/{DateTime.Now.ToString("yyyyMMdd")}/{guid}{Path.GetExtension(Request.Files[0].FileName)}";
                return Json(new
                {
                    imageUrl = path
                });
            }
            return Json(new
            {
                status = 200
            });
        }

        private static string GetRandomStringByGuid(int length)
        {
            var str = Guid.NewGuid().ToString().Replace("-", "");
            return str.Substring(0, length);
        }
    }
}