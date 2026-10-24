using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using game.Infrastructure;
using game.Entity.Money;

namespace game.WebSite.Controllers
{

    //聯運專用
    public class UnionConvertController : Controller
    {
        private string[] ips;
        private string convertSignKey;

        public UnionConvertController()
        {

            convertSignKey = ConfigurationManager.AppSettings["Union_ConvertSignKey"];
        }

        
        /// <summary>
        /// 获取记录列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ExchangeList(string flag, string number)
        {
            var c = Check();
            if (c != null) return c;

            //聯運
            var list = SiteHelper.GetAPIResult<List<PullConvertLog>>(string.Format("/Money/UnionPullConvertLog?flag={0}&number={1}", flag, number ));
            return Json(new ConvertResult(1, list), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 更新打款結果
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status">3=打款成功</param>
        /// <param name="transID"></param>
        /// <param name="transData"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ExchangeEditStatus(int id, int flag)
        {
            var c = Check();
            if (c != null) return c;

            int returnCode = 1;
            string returnMessage = "";
            
            //打款成功
            if (flag == 3)
            {

                APIResult2 result = SiteHelper.GetAPIResult<APIResult2>(string.Format("/Money/FinishedConvertLog?id={0}&status={1}&realTaxAmount={2}&transID={3}&transData={4}&updateUser={5}",
                    id,
                    flag,
                    0,
                    "",
                    "",
                    "system"));


                // 訂單不存在, 修改失敗
                if (result.ResultCode == -1) returnCode = -4;
                // 必须审核通过才可打款
                if (result.ResultCode == -2) returnCode = -2;
                // 訂單更新失敗
                if (result.ResultCode == -3) returnCode = -3;
                
                returnMessage = result.Message;
            }

            //打款失敗
            else if (flag == 4 || flag == 5 || flag == 6 || flag == 7 || flag == 8 || flag == 9)
            {
                APIResult result = SiteHelper.GetAPIResult<APIResult>(string.Format("/Money/ModifyConvertLogStatusRefused?id={0}&status={1}&updateUser={2}", id, flag,"system"));
                
                // 訂單更新失敗
                if (!result.Result) returnCode = -4;
                
                returnMessage = result.Message;

            }
            else
            {
                returnCode = 0;
                returnMessage = "状态不正确";
            }

            return Json(new ConvertResult(returnCode, returnMessage), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 取得單筆兌換狀態
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="transID"></param>
        /// <param name="transData"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ExchangelistCheckidStatus(int id)
        {
            var c = Check();
            if (c != null) return c;

            var status = SiteHelper.GetAPIResult<int>(string.Format("/Money/GetPullConvertLogSatusById?id={0}", id));

            return Json(new ConvertResult(1,status), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 檢查簽章與來源IP
        /// </summary>
        /// <returns></returns>
        private ActionResult Check()
        {
            if (!CheckSign(Request, convertSignKey))
            {
                return Json(new ConvertResult(-1, "签章错误"), JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        /// <summary>
        /// 检查签名是否正确
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private bool CheckSign(HttpRequestBase request, string key)
        {
            List<string> kv = new List<string>();
            System.Collections.Specialized.NameValueCollection paras;
            if (request.HttpMethod.ToLower() == "get")
            {
                paras = request.QueryString;
            }
            else
            {
                paras = request.Form;
            }

            string sign = "";
            foreach (var k in paras.AllKeys) //.OrderBy(m => m.ToLower())
            {
                if (k.ToLower() == "sign")
                {
                    sign = paras[k];
                }
                else
                {
                    kv.Add(string.Format("{0}={1}", k, paras[k]));
                }
            }
            string text = string.Join("&", kv) + "&key=" + key;
            string c_sign = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(text, "md5");
            return c_sign.ToLower() == sign.ToLower();
        }
        
    }

    
}