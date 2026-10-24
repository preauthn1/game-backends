using System;
using example.invalid;
using System.Drawing;
using ZXing;
using ZXing.QrCode;

namespace game.WebSite
{
    public static class QRCodeService
    {
        /// <summary>
        /// 创建支付订单
        /// </summary>
        public static string Create(string qrcodeString, int height = 300, int width = 300)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = height,
                    Width = width,
                    Margin = 1
                }
            };
            Bitmap m_Bitmap = writer.Write(qrcodeString);
            MemoryStream ms = new MemoryStream();
            m_Bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            byte[] buff = ms.ToArray();
            return $"<img src='data:image/jpeg;base64,{Convert.ToBase64String(buff)}'style='margin: 0 auto; display: block;' />";
        }
    }
}