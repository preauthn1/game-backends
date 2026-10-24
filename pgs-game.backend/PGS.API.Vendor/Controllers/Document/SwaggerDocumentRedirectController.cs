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
using target.Models;
using target.Models.Models;

namespace target.API.Vendor.Controllers
{

    [ApiExplorerSettings(IgnoreApi = true)]
    public class SwaggerDocumentRedirectController : BaseController
    {


        private static string urlOfTransferMode = "/swagger/index.html";
        private static string urlOfParameter = "/swagger/index.html?urls.primaryName=";
        private static string urlOfSingleWalletMode = "/swagger/index.html?urls.primaryName=target.API.SeamlessMode";

        // api-document/transfer/EN
        // api-document/transfer/CN
        // api-document/singleWallet/EN
        // api-document/singleWallet/CN

        /// <summary>
        /// 透過中間一層網址導頁 (英文)
        /// </summary>
        [Route("api-document/transfer/EN")]
        [ApiConfig(IsLogApiEnable = false)]
        [HttpGet]
        public IActionResult RedirectTo_TransferDocument_EN()
        {
            Response.Cookies.Append(SystemConstant.HeaderKey_Lang, LanguageEnum.en_us.ToString());     
            return Redirect(urlOfTransferMode);
        }

        /// <summary>
        /// 透過中間一層網址導頁 (英文)
        /// </summary>
        [Route("api-document/transfer/CN")]
        [ApiConfig(IsLogApiEnable = false)]
        [HttpGet]
        public IActionResult RedirectTo_TransferDocument_CN()
        {
            Response.Cookies.Append(SystemConstant.HeaderKey_Lang, LanguageEnum.zh_cn.ToString());
            return Redirect(urlOfTransferMode);
        }

        /// <summary>
        /// 透過中間一層網址導頁 (英文)
        /// </summary>
        [Route("api-document/singleWallet/EN")]
        [ApiConfig(IsLogApiEnable = false)]
        [HttpGet]
        public IActionResult RedirectTo_SingleWalletDocument_EN()
        {
            Response.Cookies.Append(SystemConstant.HeaderKey_Lang, LanguageEnum.en_us.ToString());
            return Redirect(urlOfSingleWalletMode);
        }


        /// <summary>
        /// 透過中間一層網址導頁 (中文)
        /// </summary>
        [Route("api-document/singleWallet/CN")]
        [ApiConfig(IsLogApiEnable = false)]
        [HttpGet]
        public IActionResult RedirectTo_SingleWalletDocument_CN()
        {
            Response.Cookies.Append(SystemConstant.HeaderKey_Lang, LanguageEnum.zh_cn.ToString());
            return Redirect(urlOfSingleWalletMode);
        } 
        
        /// <summary>          
        /// 特殊客製API 
        /// </summary>
        [Route("api-document/customized-api/Richmond")]
        [ApiConfig(IsLogApiEnable = false)]
        [HttpGet]
        public IActionResult RedirectTo_CustomizedApi_Richmond()
        {
            Response.Cookies.Append(SystemConstant.HeaderKey_Lang, LanguageEnum.en_us.ToString());
            return Redirect(urlOfParameter + SystemConstant.SwaggerGroupName_Richmond);
        }



        /// <summary>          
        /// freespin API 
        /// </summary>
        [Route("api-document/freespin/EN")]
        [ApiConfig(IsLogApiEnable = false)]
        [HttpGet]
        public IActionResult RedirectTo_CustomizedApi_Freespin_EN()
        {
            Response.Cookies.Append(SystemConstant.HeaderKey_Lang, LanguageEnum.en_us.ToString());
            return Redirect(urlOfParameter + SystemConstant.SwaggerGroupName_Freespin);
        }


        /// <summary>          
        /// freespin API 
        /// </summary>
        [Route("api-document/freespin/CN")]
        [ApiConfig(IsLogApiEnable = false)]
        [HttpGet]
        public IActionResult RedirectTo_CustomizedApi_Freespin_CN()
        {
            Response.Cookies.Append(SystemConstant.HeaderKey_Lang, LanguageEnum.zh_cn.ToString());
            return Redirect(urlOfParameter + SystemConstant.SwaggerGroupName_Freespin);
        }


        [Route("api-document/page/{type}/{lang}")]
        [ApiConfig(IsLogApiEnable = false)]
        [HttpGet]
        public IActionResult RedirectTo_CustomizedApi_By_Type(string type , string lang)
        {
            if(lang == "CN")
            {
                Response.Cookies.Append(SystemConstant.HeaderKey_Lang, LanguageEnum.zh_cn.ToString());
            }
            else
            {
                Response.Cookies.Append(SystemConstant.HeaderKey_Lang, LanguageEnum.en_us.ToString());
            }

            //type 需要對應其中一種 (可能還會陸續新增)
            List<string> listOfTypes = SystemConstant.SwaggerSpecialDocumentGroups;

            string targetType = "";

            foreach (var eachType in listOfTypes)
            {
                if(eachType.ToLower() == type.ToLower())
                {
                    targetType = eachType;
                    break;
                }
            }

            if(string.IsNullOrWhiteSpace(targetType))
            {
                throw new TerproException(ApiErrorCode.NotFoundPageOrAPiUrl, new ErrorDetailModel("type not found"));
            }

            return Redirect(urlOfParameter + targetType);
        }



    }
}
