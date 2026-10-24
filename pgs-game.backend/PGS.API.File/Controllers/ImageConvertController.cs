using Microsoft.AspNetCore.Mvc;
using target.Models.Enums;
using target.Web.Modules.Filter;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using example.invalid;
using example.invalid;

namespace target.API.File.Controllers
{
    public class ImageConvertController : BaseController
    {

		//[ApiConfig(ApiCategory = ApiConfigCategoryEnum.DebugApi)]
		//[Route("image/convert-to-base64/remote-url")]
		//[HttpGet, HttpPost]
		//public object ConvertToBase64StringFromRemoteImageUrl(string url = "")
  //      {
		//	try
  //          {
		//		string base64Result = string.Empty;

		//		using (WebClient client = new WebClient())
		//		{
		//			byte[] data = client.DownloadData(url);

		//			using (MemoryStream mem = new MemoryStream(data))
		//			{
		//				using (var myImage = Image.FromStream(mem))
		//				{
		//					base64Result = ImageToBase64(myImage, ImageFormat.Png);
		//				}
		//			}
		//		}

		//		return base64Result;
		//	}
		//	catch(Exception ex)
  //          {
		//		return ex.ToString();
  //          }
  //      }



		//private string ImageToBase64(Image image, ImageFormat format)
		//{
		//	using (MemoryStream ms = new MemoryStream())
		//	{
		//		// Convert Image to byte[]
		//		image.Save(ms, format);
		//		byte[] imageBytes = ms.ToArray();

		//		// Convert byte[] to base 64 string
		//		string base64String = Convert.ToBase64String(imageBytes);
		//		return base64String;
		//	}
		//}

	}
}
