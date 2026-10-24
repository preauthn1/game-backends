using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Text;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace game.WebSite.Controllers
{
    public class SMSController : Controller
    {
        private string[] SMS_IPS = null;
        private string logfilename = SMS.Logname;
        private string guid = Guid.NewGuid().ToString().Substring(0, 5);

        public SMSController()
        {
            var allowIPS = ConfigurationManager.AppSettings["SMS_AllowIPS"];

            if (allowIPS != null && allowIPS.Length > 0)
            {
                SMS_IPS = allowIPS.Split(',');
            }

        }


        [HttpGet]
        public string ip()
        {
            return SiteHelper.GetIP();
        }


        /// <summary>
        /// 获取codeID
        /// </summary>
        /// <returns></returns>
        ///
        [HttpPost]
        public ActionResult Send(string codeID)
        {
            string userip = SiteHelper.GetIP();

            if (SMS_IPS != null && !SMS_IPS.Contains(userip))
            {
                log(guid, string.Format("简讯请求: codeID={0}&data={1}&ip={2}", codeID, "非法访问", userip), logfilename);
                return Json(new APIResult(false, "非法访问"), JsonRequestBehavior.AllowGet);
            }
            else
            {
                log(guid, string.Format("简讯请求: codeID={0} ip={1}", codeID, userip), logfilename);
            }

            //取得簡訊資料
            var SMSdata = SMS.GetSMS(codeID);
            log(guid, string.Format("取得简讯内容: codeID={0}&data={1}", codeID, Newtonsoft.Json.JsonConvert.SerializeObject(SMSdata)), logfilename);


            var IsSuccess = false;
            var sendResult = "";
            if (SMSdata != null)
            {

                switch (SMSdata.Vender.ToLower())
                {
                    case "submail":
                        IsSuccess = SendMessage_submail(SMSdata.VCode, SMSdata.Tel, ref sendResult);
                        break;
                    case "ucpaas":
                        IsSuccess = SendMessage_ucpaas(SMSdata.VCode, SMSdata.Tel, ref sendResult);
                        break;
                }

                //更新簡訊發送結果
                SMS.UpdateSendResult(codeID, IsSuccess, sendResult);


            }
            return Json(new APIResult(true, "获取成功", SMSdata), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 发送短信 submail
        /// </summary>
        /// <param name="vCode"></param>
        /// <param name="tel"></param>
        /// <returns></returns>
        private bool SendMessage_submail(string vCode, string tel, ref string returnMessage)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(SMS.SubmailAPI);
                var data = Encoding.ASCII.GetBytes(SMS.GetPostData_submail(vCode, tel));

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                var json_serializer = new JavaScriptSerializer();
                var routes_list = (IDictionary<string, object>)json_serializer.DeserializeObject(responseString);
                var status = routes_list["status"].ToString();

                if (status == "success")
                {
                    var send_id = routes_list["send_id"].ToString();
                    var fee = routes_list["fee"].ToString();
                    var sms_credits = routes_list["sms_credits"].ToString();
                    //回傳回應內容
                    returnMessage = string.Format("status={0},sendid={1}", status, send_id);

                    log(guid, string.Format("短信请求成功:status={0}&tel={1}&vender={2}&send_id={3}&fee={4}&sms_credits={5}", status, tel, "submail", send_id, fee, sms_credits), logfilename);
                    return true;
                }
                else
                {
                    var msg = routes_list["msg"].ToString();
                    var code = routes_list["code"].ToString();
                    //回傳回應內容
                    returnMessage = string.Format("status={0},code={1},msg={2}", status, code, msg);

                    log(guid, string.Format("短信请求失败:status={0}&tel={1}&vender={2}&code={3}&msg={4}", status, tel, "submail", code, msg), logfilename);
                    return false;
                }
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
                log(guid, string.Format("短信请求错误:tel={0}&vender={1}&msg={2}", tel, "submail", ex), logfilename);
                return false;
            }

        }

        /// <summary>
        /// 发送短信 ucpaas
        /// </summary>
        /// <param name="vCode"></param>
        /// <param name="tel"></param>
        /// <returns></returns>
        private bool SendMessage_ucpaas(string vCode, string tel, ref string returnMessage)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(SMS.UCPaasAPI);
                var data = Encoding.ASCII.GetBytes(SMS.GetPostData_ucpaas(vCode, tel));

                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                var json_serializer = new JavaScriptSerializer();
                var routes_list = (IDictionary<string, object>)json_serializer.DeserializeObject(responseString);
                var status = routes_list["code"].ToString();

                if (status == "000000")
                {
                    var code = routes_list["code"].ToString();
                    var msg = routes_list["msg"].ToString();
                    var smsid = routes_list["smsid"].ToString();
                    //回傳回應內容
                    returnMessage = string.Format("status={0},code={1},msg={2},smsid={3}", status, code, msg, smsid);

                    log(guid, string.Format("短信请求成功:status={0}&tel={1}&vender={2}&smsid={3}&code={4}&msg={5}", status, tel, "ucpaas", smsid, code, msg), logfilename);
                    return true;
                }
                else
                {
                    var msg = routes_list["msg"].ToString();
                    var code = routes_list["code"].ToString();
                    //回傳回應內容
                    returnMessage = string.Format("status={0},code={1},msg={2}", status, code, msg);

                    log(guid, string.Format("短信请求失败:status={0}&tel={1}&vender={2}&code={3}&msg={4}", status, tel, "ucpaas", code, msg), logfilename);
                    return false;
                }
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
                SiteHelper.Log(string.Format("短信寄送错误:tel={0}&vender={1}&msg={2}", tel, "ucpaas", ex), logfilename);
                return false;
            }

        }

       

        //包裝paylog方式
        private static void log(string guid, string message, string filelog)
        {
            SiteHelper.Log(string.Format("{0} {1} {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm"), guid, message), filelog);
        }


    }
}