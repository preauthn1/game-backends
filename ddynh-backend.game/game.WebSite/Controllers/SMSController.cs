using game.Infrastructure;
using Newtonsoft.Json.Linq;
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

        public ActionResult Test()
        {
            try
            {
                var sendResult = "";
                var SMSdata = SMS.GetSMS("1");
                var IsSuccess = SendMessage_haoservice(SMSdata.VCode, SMSdata.Tel, SMSdata.UserID, ref sendResult);

                return Content(IsSuccess.ToString());
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
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
                        IsSuccess = SendMessage_submail(SMSdata.VCode, SMSdata.Tel, SMSdata.UserID, ref sendResult);
                        break;
                    case "ucpaas":
                        IsSuccess = SendMessage_ucpaas(SMSdata.VCode, SMSdata.Tel, SMSdata.UserID, ref sendResult);
                        break;
                    case "haoservice":
                        IsSuccess = SendMessage_haoservice(SMSdata.VCode, SMSdata.Tel, SMSdata.UserID, ref sendResult);
                        break;
                    case "吉信通":
                        IsSuccess = SendMessage_winic(SMSdata.VCode, SMSdata.Tel, SMSdata.UserID, ref sendResult);
                        break;
                    case "nuage":
                        IsSuccess = SendMessage_nuage(SMSdata.VCode, SMSdata.Tel, SMSdata.UserID, ref sendResult);
                        break;
                    case "onecode":
                        IsSuccess = SendMessage_onecode(SMSdata.VCode, SMSdata.Tel, SMSdata.UserID, ref sendResult);
                        break;
                    case "smsgo":
                        IsSuccess = SendMessage_smsgo(SMSdata.VCode, SMSdata.Tel, SMSdata.UserID, ref sendResult);
                        break;
                    case "ezlink":
                        IsSuccess = SendMessage_ezlink(SMSdata.VCode, SMSdata.Tel, SMSdata.UserID, ref sendResult);
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
        private bool SendMessage_submail(string vCode, string tel, int userId, ref string returnMessage)
        {
            string venderName = "submail";
            try
            {
                //取得URL
                var request = (HttpWebRequest)WebRequest.Create(SMS.SubmailAPI);

                //取得各信息商配置
                var venderInfo = SMS.GetVenderInfo(userId, venderName);
                //取得傳送訊息
                var postData = SMS.GetPostData_submail(vCode, tel, venderInfo);
                log(guid, $"傳送內容: data={postData}", logfilename);
                var data = Encoding.ASCII.GetBytes(postData);
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
                /**
                 * {
                    "status":"success"
                    "send_id":"093c0a7df143c087d6cba9cdf0cf3738"
                    "fee":1,
                    "sms_credits":14197
                    }
                 */
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
        private bool SendMessage_ucpaas(string vCode, string tel, int userId, ref string returnMessage)
        {
            string venderName = "ucpaas";
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(SMS.UCPaasAPI);
                //取得各信息商配置
                var venderInfo = SMS.GetVenderInfo(userId, venderName);
                //取得傳送訊息
                var postData = SMS.GetPostData_ucpaas(vCode, tel, venderInfo);
                var data = Encoding.ASCII.GetBytes(postData);

                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = data.Length;

                ServicePointManager.ServerCertificateValidationCallback += (s, cert, chain, sslPolicyErrors) => true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                SiteHelper.Log(string.Format("ucpaas use security protocol"), logfilename);
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                var json_serializer = new JavaScriptSerializer();
                var routes_list = (IDictionary<string, object>)json_serializer.DeserializeObject(responseString);
                /**
                 *  参数	    类型	描述	                            示例
                 *  code	    String	状态码	                            000000
                 *  msg	        String	对应状态码的信息	                例如：“发送成功”，“手机号码不合法”
                 *  count	    String	计费条数	                        1
                 *  create_date	String	创建时间,格式YYYY-MM-DD hh:mm:ss	2017-08-28 19:08:28
                 *  smsid	    String	短信唯一id	                        f96f79240e372587e9284cd580d8f953
                 *  mobile	    String	手机号码	                        18088888888
                 *  uid	        String	请求时透传的uid	                    2d92c6132139467b989d087c84a365d8
                 */
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

        /// <summary>
        /// 发送短信 ucpaas
        /// </summary>
        /// <param name="vCode"></param>
        /// <param name="tel"></param>
        /// <returns></returns>
        private bool SendMessage_haoservice(string vCode, string tel, int userId, ref string returnMessage)
        {
            var smsName = "haoservice";
            try
            {
                log(guid, $"SMS请求 [vendor]{smsName} [vCode] {vCode}, tel:{tel}", logfilename);
                //取得各信息商配置
                var venderInfo = SMS.GetVenderInfo(userId, smsName);
                //use HTTP GET
                var targetUri = new Uri(SMS.HaoServiceAPI + SMS.GetPostData_haoservice(vCode, tel, venderInfo));
                log(guid, $"SMS请求[method]{SMS.HaoService_Method} [uri]{targetUri}", logfilename);
                var request = (HttpWebRequest)WebRequest.Create(targetUri);

                request.Method = SMS.HaoService_Method;
                request.ContentType = "application/json";

                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                //response string
                var json_serializer = new JavaScriptSerializer();
                var routes_list = (IDictionary<string, object>)json_serializer.DeserializeObject(responseString);
                /*
                 * 服务级错误码参照(error_code)：
                 * 	201701	 错误的手机号码
                 *  201702	 错误的短信模板ID
                 *  201703	 网络错误,请重试
                 *  201704	 发送失败
                 *  201705	 参数错误
                 *  201706	 短信长度超过限制
                 *  201707	 错误的消息Id
                 *  201708	 模板参数没有全部生效,短信内容不能包含特殊字符#,请检查参数重试
                 *  201709	 发送内容和模板不匹配
                 *  201710	 有效号码不足
                 *  201711	 变量内容超过限定字符
                 *  
                 *  系统级错误码参照(error_code)：
                 *  0	     成功
                 *  10001	 错误的请求KEY
                 *  10002	 该KEY无请求权限
                 *  10003	 KEY过期
                 *  10004	 错误的SDK KEY
                 *  10005	 应用未审核，请提交认证
                 *  10007	 未知的请求源，（服务器没有获取到IP地址）
                 *  10008	 被禁止的IP
                 *  10009	 被禁止的KEY
                 *  10011	 当前IP请求超过限制
                 *  10012	 当前Key请求超过限制
                 *  10013	 测试KEY超过请求限制
                 *  10020	 接口维护
                 *  10021	 接口停用
                 *  10022	 appKey按需剩余请求次数为零
                 *  10023	 请求IP无效
                 *  10024	 网络错误
                 *  10025	 没有查询到结果
                 *  10026	 当前请求频率过高超过权限限制
                 *  10027	 账号违规被冻结
                 *  10028	 传递参数错误
                 *  10029	 系统内部异常，请重试
                 *  10030	 校验值sign错误
                 *  10031	 套餐产品编号不存在
                 *  10032	 虚拟账号余额不足
                 *  10033	 提交的订单号不能重复
                 *  10034	 通道请求超时
                 *  10035	 相同参数访问频率过高
                 *  
                {
                    "error_code": 201701,
                    "reason": "错误的手机号码",
                    "result": null
                }
                {
                    "error_code": 0,
                    "reason": "成功",
                    "result": "11377940"
                }
                 */
                var status = routes_list["error_code"].ToString();
                if (status == "0")
                {
                    var result = routes_list["result"].ToString();
                    //回傳回應內容
                    returnMessage = string.Format("status={0},sendid={1}", status, result);

                    log(guid, string.Format("短信请求成功:status={0}&tel={1}&vender={2}&result={3}", status, tel, smsName, result), logfilename);
                    return true;
                }
                else
                {
                    var msg = routes_list["reason"].ToString();
                    var code = routes_list["error_code"].ToString();
                    //回傳回應內容
                    returnMessage = string.Format("status={0},code={1},msg={2}", status, code, msg);

                    log(guid, string.Format("短信请求失败:status={0}&tel={1}&vender={2}&code={3}&msg={4}", status, tel, smsName, code, msg), logfilename);
                    return false;
                }
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
                SiteHelper.Log(guid, string.Format("短信寄送错误 [vendor] {0} [tel] {1} [vCode] {2} [Exception] {3}", smsName, tel, vCode, ex), logfilename);
                return false;
            }
        }

        /// <summary>
        /// 发送短信 ucpaas
        /// </summary>
        /// <param name="vCode"></param>
        /// <param name="tel"></param>
        /// <returns></returns>
        private bool SendMessage_winic(string vCode, string tel, int userId, ref string returnMessage)
        {
            var smsName = "winic";
            try
            {
                return false;
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
                SiteHelper.Log(guid, string.Format("短信寄送错误 [vendor] {0} [tel] {1} [vCode] {2} [Exception] {3}", smsName, tel, vCode, ex), logfilename);
                return false;
            }
        }

        /// <summary>
        /// 发送短信 nuage
        /// </summary>
        private bool SendMessage_nuage(string vCode, string tel, int userId, ref string returnMessage)
        {
            string smsName = "nuage";
            try
            {
                log(guid, $"SMS请求 [vendor]{smsName} [vCode] {vCode}, tel:{tel}", logfilename);
                //取得各信息商配置
                var venderInfo = SMS.GetVenderInfo(userId, smsName);

                //use HTTP GET
                var targetUri = new Uri(SMS.Nuage_Url + "?" + SMS.GetPostData_nuage(vCode, tel, venderInfo));
                log(guid, $"SMS请求 [method]{SMS.HaoService_Method} [uri]{targetUri}", logfilename);
                var request = (HttpWebRequest)WebRequest.Create(targetUri);

                request.Method = "GET";
                request.ContentType = "application/x-www-form-urlencoded";

                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                //response string
                var json_serializer = new JavaScriptSerializer();
                var routes_list = (IDictionary<string, object>)json_serializer.DeserializeObject(responseString);
                /*
                 *  错误码参照(error_code)：
                 *  AccessDenied    Access Denied => 跟簡訊商確認資訊是否正確
                 *  receiverNeed    request 缺少 receiver
                 *  nameNeed        request 缺少 name
                 *  pwdNeed         request 缺少 pwd
                 *  validcodeNeed   request 缺少 validcode
                 *  checksumNeed    request 缺少 checksum
                 *  checksumError   request 缺少 receiver
                 *  {
                    {
                        "IsSuccess": true,
                        "Message": "成功"
                    }
                    {
                        "IsSuccess": false,
                        "Message": "AccessDenied"
                    }
                 */
                var status = routes_list["IsSuccess"].ToString().ToLower();
                var msg = routes_list["Message"].ToString();
                //回傳回應內容
                returnMessage = $"status={status},msg={msg}";
                if (status == "true")
                {
                    log(guid, $"短信请求成功 [status]{status} [tel]{tel} [vender]{smsName} [result]{msg}", logfilename);
                    return true;
                }
                else
                {
                    log(guid, $"短信请求失败 [status]{status} [tel]{tel} [vender]{smsName} [result]{msg}", logfilename);
                    return false;
                }
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
                SiteHelper.Log($"短信寄送错误 [tel]{tel} [vender]{smsName} [msg]{ex}", logfilename);
                return false;
            }
        }


        /// <summary>
        /// 发送短信 onecode
        /// </summary>
        private bool SendMessage_onecode(string vCode, string tel, int userId, ref string returnMessage)
        {
            string smsName = "onecode";
            try
            {
                log(guid, $"SMS请求 [vendor]{smsName} [vCode] {vCode}, tel:{tel}", logfilename);
                //取得各信息商配置
                var venderInfo = SMS.GetVenderInfo(userId, smsName);

                //use HTTP GET
                var targetUri = new Uri(SMS.OneCodAPI + "?" + SMS.GetPostData_OneCode(vCode, tel, venderInfo));
                log(guid, $"SMS请求 [method]{SMS.OneCode_Method} [uri]{targetUri}", logfilename);
                var request = (HttpWebRequest)WebRequest.Create(targetUri);

                request.Method = "GET";
                request.ContentType = "application/x-www-form-urlencoded";

                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                //response string
                var json_serializer = new JavaScriptSerializer();
                var res = (IDictionary<string, object>)json_serializer.DeserializeObject(responseString);
                /*
                 *  错误码参照(error_code)：
                 *  000 成功
                 *  001 參數錯誤
                 *  002 預約時間錯誤
                 *  003 預約時間已過期
                 *  004 發送名稱錯誤
                 *  005 發送名稱過長
                 *  006 發送名稱非正確店號
                 *  101 帳號不存在
                 *  102 帳號已停用
                 *  103 Secret 錯誤
                 *  104 IP 位址不允許
                 *  201 無任何點數
                 *  202 號碼錯誤或所屬國家未開通
                 *  203 該國家點數不足
                 *  204 無正確收件者
                 *  205 收件者超過 250 筆
                 */
                var status = res["stats"].ToString().ToLower(); //發送成功或失敗; True/False
                var error_code = res["error_code"].ToString();
                var msg = res["error_msg"].ToString();        //發送結果說明，請參考 error_code
                var data = res["data"].ToString();              //發送成功時提供
                //回傳回應內容
                returnMessage = $"status={status},msg={msg}";
                if (status == "true")
                {
                    log(guid, $"短信请求成功 [status]{status} [tel]{tel} [vender]{smsName} [result]{msg}", logfilename);
                    return true;
                }
                else
                {
                    log(guid, $"短信请求失败 [status]{status} [tel]{tel} [vender]{smsName} [result]{msg}", logfilename);
                    return false;
                }
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
                SiteHelper.Log($"短信寄送错误 [tel]{tel} [vender]{smsName} [msg]{ex}", logfilename);
                return false;
            }
        }


        /// <summary>
        /// 发送短信 SMSGo
        /// </summary>
        private bool SendMessage_smsgo(string vCode, string tel, int userId, ref string returnMessage)
        {
            string smsName = "smsgo";
            try
            {
                log(guid, $"SMS请求 [vendor]{smsName} [vCode] {vCode}, tel:{tel}", logfilename);
                //取得各信息商配置
                var venderInfo = SMS.GetVenderInfo(userId, smsName);

                //use HTTP GET
                var targetUri = new Uri(SMS.SmsGoAPI + "?" + SMS.GetPostData_SmsGo(vCode, tel, venderInfo));
                log(guid, $"SMS请求 [method]{SMS.OneCode_Method} [uri]{targetUri}", logfilename);
                var request = (HttpWebRequest)WebRequest.Create(targetUri);

                request.Method = "GET";
                request.ContentType = "application/x-www-form-urlencoded";

                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                log(guid, $"SMS response [responseString]{responseString}", logfilename);

                //response string
                JObject json = JObject.Parse(responseString);
                /*
                { "result":{ "msgid":1504280910499665,"statuscode":"0","statusstr":"OK","point":1} }
                 *  错误码参照(error_code)：
                 *  -1 CGI string error or Unknow
                 *  -2 授權錯誤(帳號/密碼錯誤/IP錯誤)
                 *  -3 Priority錯誤(User設定太高的優先權)
                 *  -4 A Number違反規則
                 *  -5 B Number違反規則
                 *  -6 Closed User
                 *  -7 Invalid Encoding
                 *  -8 點數不夠
                 *  -9 Invalid msisdn amount                 *  -10 Send Fail
                 *   response [responseString]{"result":{"msgid":2008111217380804,"statuscode":"0","statusstr":"OK","point":3}}
                 */
                JObject result = JObject.Parse(json["result"].ToString());
                var status = result["statuscode"].ToString(); //發送成功或失敗; 0 or -N
                var msg = result["statusstr"].ToString();        //發送結果說明，請參考 error_code
                var data = result["msgid"].ToString();              //msgid
                //回傳回應內容
                returnMessage = $"status={status},msg={msg}";
                if (status == "0")
                {
                    log(guid, $"短信请求成功 [status]{status} [tel]{tel} [vender]{smsName} [result]{msg}", logfilename);
                    return true;
                }
                else
                {
                    log(guid, $"短信请求失败 [status]{status} [tel]{tel} [vender]{smsName} [result]{msg}", logfilename);
                    return false;
                }
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
                SiteHelper.Log($"短信寄送错误 [tel]{tel} [vender]{smsName} [msg]{ex}", logfilename);
                return false;
            }
        }


        public bool SendMessage_ezlink(string vCode, string tel, int userId, ref string returnMessage)
        {
            string smsName = "ezlink";
            try
            {
                log(guid, $"SMS请求 [vendor]{smsName} [vCode] {vCode}, tel:{tel}", logfilename);
                //取得各信息商配置
                var venderInfo = SMS.GetVenderInfo(userId, smsName);
                JObject json = JObject.Parse(venderInfo.SMSInfo);
                if(json["sign"] == null)
                {
                    log(guid, $"短信请求失败, SMSInfo not set url", logfilename);
                    return false;
                }
                var apiUrl = json["url"].ToString();
                //use HTTP GET

                var targetUri = new Uri(apiUrl + "?" + SMS.GetPostData_EzLink(vCode, tel, venderInfo));
                log(guid, $"SMS请求 [method]{apiUrl} [uri]{targetUri}", logfilename);
                var request = (HttpWebRequest)WebRequest.Create(targetUri);

                request.Method = "GET";
                request.ContentType = "application/x-www-form-urlencoded";

                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                log(guid, $"SMS response [responseString]{responseString}", logfilename);

                //response string
                /*
                 * 0 参数错误 缺少必要参数或者参数值有误,比如:号码长度不符、预定时间格式不对 1 成功 请求成功,系统将保存发送数据并计费 2 无此账号 系统中没有对应的合作方账号(ID)
                 * 3 账号未激活 合作方账号(ID)对应的账号没有激活 4 余额不足 要发送的短信数量超出了当前账号的剩余值 5 非法请求 发起请求的服务器 IP 没有在系统中备案 
                 * 6 号码无效 没有一个符合规则的号码 7 预约超时 预定发送短信的时间超出允许范围 8 群发限制 账号不支持同时发送多个号码 
                 */
                returnMessage = responseString;
                if (responseString == "1")
                {
                    log(guid, $"短信请求成功 [responseString]{responseString} [tel]{tel} [vender]{smsName} [result]{responseString}", logfilename);
                    return true;
                }
                else
                {
                    log(guid, $"短信请求失败 [responseString]{responseString} [tel]{tel} [vender]{smsName} [result]{responseString}", logfilename);
                    return false;
                }
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
                log(guid,$"短信寄送错误 [tel]{tel} [vender]{smsName} [msg]{ex}", logfilename);
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