using System;
using System.Collections.Generic;
using example.invalid;
using example.invalid;
using System.Text;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Agent.Helper;
using Service.Service.AgentClub;

namespace Agent.Areas.agentclub.Controllers
{
    public class SMSController : BaseController
    {
        private SMSService _service = new SMSService();
        private AgentService _AgentService = new AgentService();

        [HttpPost]
        public ActionResult SendVerificationCode()
        {
            try
            {
                //取得簡訊資料
                var vCode = HelpExtensions.CreateRandomCode(5);
                var user = _AgentService.GetAgentUser(AdminID);
                var tel = user.Tel;

                var IsSuccess = false;
                var sendResult = string.Empty;
                var msg = string.Empty;
                var result = false;

                IsSuccess = SendMessage_submail(vCode, tel, ref sendResult);

                if (IsSuccess)
                {
                    result = _service.CresteSMS(vCode, ref msg);
                }
                else
                {
                    msg = "驗證碼發送失敗";
                }

                return Json(new
                {
                    msg,
                    result,
                    status = result ? 20001 : 10010
                });
            }
            catch (Exception ex)
            {
                return OnError(ex.Message);
            }
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
                var request = (HttpWebRequest)WebRequest.Create(_service.SubmailAPI);
                var data = Encoding.ASCII.GetBytes(_service.GetPostData_submail(vCode, tel));

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

                    return true;
                }
                else
                {
                    var msg = routes_list["msg"].ToString();
                    var code = routes_list["code"].ToString();
                    //回傳回應內容
                    returnMessage = string.Format("status={0},code={1},msg={2}", status, code, msg);

                    return false;
                }
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
                return false;
            }
        }
    }
}