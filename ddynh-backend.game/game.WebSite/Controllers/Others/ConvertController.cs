using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using game.Entity.Money;
using game.Infrastructure;
using game.WebSite.PaymentHelper;
using static game.WebSite.PayConfig;

namespace game.WebSite.Controllers
{
    public class ConvertController : Controller
    {
        private string[] ips;
        private string convertSignKey;
        string logGuid = Guid.NewGuid().ToString().Substring(0, 5);

        public ConvertController()
        {

            convertSignKey = ConfigurationManager.AppSettings["ConvertSignKey"];
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

            var list = SiteHelper.GetAPIResult<List<PullConvertLog>>(string.Format("/Money/PullConvertLog?flag={0}&number={1}", flag, number));
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
                APIResult result = SiteHelper.GetAPIResult<APIResult>(string.Format("/Money/ModifyConvertLogStatusRefused?id={0}&status={1}&updateUser={2}", id, flag, "system"));

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

            return Json(new ConvertResult(1, status), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult WithdrawCallback(string orderId, string teamOrderId, int orderAmount, int realAmount, string status, string time, string sign)
        {
            var state = tungyi.WithdrawCallback(logGuid, orderId, teamOrderId, orderAmount, realAmount, status, time, sign);

            if (state == PayState.SignError)
            {
                return Content("SignError");
            }
            else if (state == PayState.Success)
            {
                return Content("SUCCESS");
            }
            else
            {
                return Content("Error");
            }
        }

        [HttpPost]
        public ActionResult CkWithdrawCallback()
        {
            var state = CK.WithdrawCallback(logGuid);

            if (state == PayState.SignError)
            {
                return Content("SignError");
            }
            else if (state == PayState.Success)
            {
                return Content("success");
            }
            else
            {
                return Content("Error");
            }
        }

        /// <summary>
        /// 風云聚合代付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DSDFWithdrawCallback()
        {
            var state = DSDFPay.WithdrawCallback(logGuid);

            if (state == PayState.SignError)
            {
                return Content("true");
            }
            else if (state == PayState.Success)
            {
                return Content("true");
            }
            else
            {
                return Content("true");
            }
        }

        /// <summary>
        /// 利鑫代付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult LXWithdrawCallback()
        {
            var state = LXPay.WithdrawCallback(logGuid);

            if (state == PayState.Success)
            {
                return Content("success");
            }
            else
            {
                return Content("success");
            }
        }

        /// <summary>
        /// MTPAY 聚寶盆代付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MTWithdrawCallback()
        {
            var state = MTPay.WithdrawCallback(logGuid);

            if (state == PayState.Success)
            {
                return Content("OK");
            }
            else
            {
                return Content("OK");
            }
        }

        /// <summary>
        /// MSZPAY 碼上代付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MSZWithdrawCallback()
        {
            var state = MSZPay.WithdrawCallback(logGuid);

            if (state == PayState.Success)
            {
                return Content("SUCCESS");
            }
            else
            {
                return Content("SUCCESS");
            }
        }

        /// <summary>
        /// 檢查簽章與來源IP
        /// </summary>
        /// <returns></returns>
        private ActionResult Check()
        {
            //if (ips != null && ips.Length > 0 && !ips.Contains(FW.Core.Helper.IPHelper.GetIP()))
            //{
            //    return Json(new ConvertResult(99, FW.Core.Helper.IPHelper.GetIP()), JsonRequestBehavior.AllowGet);
            //}
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

    public class ConvertResult
    {
        public ConvertResult(int result, object data = null)
        {
            this.result = result;
            this.data = data;

        }

        /// <summary>
        /// 結果代碼
        /// </summary>
        public int result
        {
            get;
            set;
        }


        /// <summary>
        /// 扩展信息
        /// </summary>
        public object data { get; set; }
    }
}