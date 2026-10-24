using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZXing.QrCode;
using ZXing.Common;
using ZXing;
using System.Drawing;
using example.invalid;
using example.invalid;
using game.Infrastructure;
using Newtonsoft.Json;

namespace game.WebSite.Controllers
{
    public class ShareController : Controller
    {
        // GET: Share
        public ActionResult Index(string pid, string aid)
        {
            try
            {
                var api = SiteHelper.GetAPIResult<Entity.Area.Area>(string.Format("/Area/GetAreaById?areaID={0}", aid));
                string URL = api.ShareURL;

                if (!String.IsNullOrEmpty(URL))
                {
                    var writer = new BarcodeWriter
                    {
                        Format = BarcodeFormat.QR_CODE,
                        Options = new QrCodeEncodingOptions
                        {
                            Height = 300,
                            Width = 300,
                            Margin = 1
                        }
                    };
                    Bitmap m_Bitmap = writer.Write(URL);

                    MemoryStream ms = new MemoryStream();
                    m_Bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                    byte[] buff = ms.ToArray();

                    return File(buff, "image/jpeg");
                }
            }
            catch (Exception e)
            {
                return new EmptyResult();
            }

            return new EmptyResult();
        }

        public ActionResult ShareURL(string pid, string aid)
        {
            try
            {
                var _api = SiteHelper.GetAPIResult<Entity.Area.Area>(string.Format("/Area/GetAreaById?areaID={0}", aid));
                string _URL = _api.ShareURL;

                if (string.IsNullOrEmpty(_URL))
                {
                    return new EmptyResult();
                }

                return Content(HttpUtility.UrlEncode(_URL));

            }
            catch (Exception e)
            {
                return new EmptyResult();
            }
        }
    }
}