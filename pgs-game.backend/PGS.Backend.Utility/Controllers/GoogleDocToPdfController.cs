using Lck.GoogleApi;
using Microsoft.AspNetCore.Mvc;
using target.Models.Enums;
using target.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid;
using example.invalid.Http;
using example.invalid.Http.Headers;
using System.Web;

namespace target.Backend.Utility.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class GoogleDocToPdfController : ControllerBase
    {

        /// <summary>
        /// 執行API 就會轉換檔案       
        /// 將 target API 文件 (google docs) 取代變數關鍵字、並轉成對應的四個檔案名
        /// 檔案是直接寫在某個開發者電腦常用的檔案路徑 C:\Gitlab_DDYNH\gameicons\cs-download\{fileName}.pdf 
        /// </summary>
        [Route("google_doc_to_pdf")]
        [HttpGet, HttpPost]
        public IActionResult ConvertExcelToJson([FromQuery] string langs = "en,tw,cn,ms", bool isReplaceCharacter = true)
        {
            var begin = DateTime.UtcNow;

            GoogleDocUtility.ReadAndReplaceDocFile(PGSDocumentTypeEnum.TransferAPI_EN, PromoteBrandTypeEnum.target);
            GoogleDocUtility.ReadAndReplaceDocFile(PGSDocumentTypeEnum.TransferAPI_CN, PromoteBrandTypeEnum.target);
            GoogleDocUtility.ReadAndReplaceDocFile(PGSDocumentTypeEnum.TransferAPI_EN, PromoteBrandTypeEnum.BWG);
            GoogleDocUtility.ReadAndReplaceDocFile(PGSDocumentTypeEnum.TransferAPI_CN, PromoteBrandTypeEnum.BWG);

            GoogleDocUtility.ReadAndReplaceDocFile(PGSDocumentTypeEnum.SeamlessAPI_EN, PromoteBrandTypeEnum.target);
            GoogleDocUtility.ReadAndReplaceDocFile(PGSDocumentTypeEnum.SeamlessAPI_CN, PromoteBrandTypeEnum.target);
            GoogleDocUtility.ReadAndReplaceDocFile(PGSDocumentTypeEnum.SeamlessAPI_EN, PromoteBrandTypeEnum.BWG);
            GoogleDocUtility.ReadAndReplaceDocFile(PGSDocumentTypeEnum.SeamlessAPI_CN, PromoteBrandTypeEnum.BWG);

            int totalSeconds = (int)(DateTime.UtcNow - begin).TotalSeconds;

            return Ok($"處理完成，總花費時間 {totalSeconds} 秒");
        }


    }

}
