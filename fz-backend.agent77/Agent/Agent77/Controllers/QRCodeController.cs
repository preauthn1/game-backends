using System;
using System.Collections.Generic;
using System.Drawing;
using example.invalid;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZXing;
using ZXing.QrCode;

namespace Agent77.Controllers
{
    public class QRCodeController : Controller
    {
        public ActionResult GetLiveQRCode(string url)
        {
            try
            {
                if (!string.IsNullOrEmpty(url))
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
                    Bitmap m_Bitmap = writer.Write(url);

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
    }
}