using game.Entity.Area;
using game.Entity.Pay;
using game.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    public class tungyi
    {

        public class PayAmountInfo
        {
            public int typeID { get; set; }
            public int gwid { get; set; }
            public string gatewayName { get; set; }
            public string typeName { get; set; }
            public string currencyCode { get; set; }
            public int payType { get; set; }
            public string tradeCode { get; set; }
            public bool isMustBeConverted { get; set; }
            public string amounts { get; set; }
            public List<PayPoints> amountsPoint { get; set; }
            public int minAmount { get; set; }
            public int maxAmount { get; set; }
            public int status { get; set; }            
            public int popular { get; set; }
            public int sort { get; set; }
            public int isRange { get; set; }
            public int priority { get; set; }
        }

        public class Data
        {
            public List<PayAmountInfo> payAmountInfo { get; set; }
        }

        public class Amount
        {
            public int code { get; set; }
            public string msg { get; set; }
            public Data data { get; set; }
            public string sign { get; set; }
        }

        public class PayPoints
        {
            [JsonProperty("id")]
            public Int64 ID { get; set; }
            [JsonProperty("amount")]
            public Int64 Amount { get; set; }
            [JsonProperty("pointAmount")]
            public Int64 PointAmount { get; set; }
            [JsonProperty("bouns")]
            public Int64 Bonus { get; set; }
            [JsonProperty("payTypeID")]
            public int PayTypeID { get; set; }
        }


        public static string post(CDPayOrder order, string guid, string tradeCode, string appid, ref string outMessage)
        {
            //如果transID = 0, 表示曾经跟统一取得跳转位置, 不再重复进行
            if (order.TransID == "0")
            {
                return string.Empty;
            }

            var tungyi_mchid = string.Empty;
            var tungyi_Key = string.Empty;
            var teamOrderId = order.Id.ToString();
            var orderAmount = (order.Amount * 100).ToString();
            var payType = tradeCode;
            var playerId = order.UserID.ToString();
            var playerIp = order.IPAddress;
            var deviceType = order.UserInfo.HDType.ToString();
            var deviceId = order.UserInfo.HDType.ToString();
            var vipGrade = order.UserInfo.GradeID.ToString();
            var description = productName;
            var phone = string.Empty;
            var alipayAccount = order.UserInfo.AlipayAccount;
            var bankId = order.UserInfo.UnionpayAccount;
            var seqId = GetTimeStamp();

            switch (order.GWID)
            {
                case (int)PayGatewayType.Tungyi:
                    tungyi_mchid = Tungyi_mchid;
                    tungyi_Key = Tungyi_Key;
                    break;
                case (int)PayGatewayType.TungyiHonda:
                    tungyi_mchid = TungyiHonda_mchid;
                    tungyi_Key = TungyiHonda_Key;
                    break;
                case (int)PayGatewayType.TungyiDF:
                    tungyi_mchid = TungyiDF_mchid;
                    tungyi_Key = TungyiDF_Key;
                    break;
            }

            Dictionary<string, string> paras = new Dictionary<string, string>();

            paras.Add("accountId", tungyi_mchid);
            paras.Add("alipayAccount", alipayAccount);
            paras.Add("appid", appid);
            paras.Add("bankId", bankId);
            paras.Add("description", description);
            paras.Add("deviceId", deviceId);
            paras.Add("deviceType", deviceType);
            paras.Add("orderAmount", orderAmount);
            paras.Add("payType", payType);
            paras.Add("phone", phone);
            paras.Add("playerId", playerId);
            paras.Add("playerIp", playerIp);
            paras.Add("seqId", seqId);
            paras.Add("teamOrderId", teamOrderId);
            paras.Add("vipGrade", vipGrade);
            paras.Add("key", tungyi_Key);

            string pay_md5sign = Md5(string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value))), Encoding.UTF8);

            paras.Remove("key");
            paras.Add("sign", pay_md5sign);

            var data = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            var responseString = string.Empty;


            SiteHelper.Log(guid, string.Format("支付请求 [data] {0}", data), Tungyi_logFile);

            Stream instream = null;
            StreamReader sr = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;
            Encoding encoding = Encoding.UTF8;
            try
            {
                // 设置参数
                request = WebRequest.Create(Tungyi_PayURL + data) as HttpWebRequest;
                CookieContainer cookieContainer = new CookieContainer();
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.Method = "GET";
                request.ContentType = "application/x-www-form-urlencoded";
                //发送请求并获取相应回应数据
                response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                instream = response.GetResponseStream();
                sr = new StreamReader(instream, encoding);
                //返回结果网页（html）代码
                string content = sr.ReadToEnd();
                string err = string.Empty;
                responseString = content;
                SiteHelper.Log(guid, string.Format("支付请求结果 [response] {0}", responseString), Tungyi_logFile);
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                SiteHelper.Log(guid, string.Format("支付请求结果 [Exception] {0}", err), Tungyi_logFile);
            }

            SiteHelper.Log(guid, string.Format("支付请求结果 [JObject.Parse] {0}", responseString), Tungyi_logFile);
            JObject json = JObject.Parse(responseString);

            //充值失败
            if (json["data"] == null)
            {
                var code = json["code"];
                var msg = json["msg"];

                switch (code.ToString())
                {
                    case "10000":
                        outMessage = "参数错误";
                        break;
                    case "10001":
                        outMessage = "内部返回码不为整型";
                        break;
                    case "10002":
                        outMessage = "内部返回数据不为数组";
                        break;
                    case "10003":
                        outMessage = "签名错误";
                        break;
                    case "10004":
                        outMessage = "渠道class没有找到";
                        break;
                    case "10005":
                        outMessage = "DB访问错误";
                        break;
                    case "10006":
                        outMessage = "渠道类返回结果不合法";
                        break;
                    case "10007":
                        outMessage = "项目组订单号已存在";
                        break;
                    case "10008":
                        outMessage = "渠道类异常";
                        break;
                    case "10009":
                        outMessage = "黑名单用户";
                        break;
                    case "10010":
                        outMessage = "获取项目组信息失败";
                        break;
                    case "10011":
                        outMessage = "调用第三方渠道创建订单失败";
                        break;
                    case "10012":
                        outMessage = "未知支付类型";
                        break;
                    case "10013":
                        outMessage = "获取支付渠道ID失败";
                        break;
                    case "10014":
                        outMessage = "获取支付渠道信息失败";
                        break;
                    case "10015":
                        outMessage = "项目组没有分配的渠道";
                        break;
                    case "10016":
                        outMessage = "第三方返回错误";
                        break;
                    case "10017":
                        outMessage = "解密失败";
                        break;
                    case "10018":
                        outMessage = "提现失败";
                        break;
                    case "10019":
                        outMessage = "项目组订单不存在";
                        break;
                    case "20000":
                        outMessage = "系统内部错误";
                        break;
                    default:
                        outMessage = "充值异常";
                        break;
                }

                //记录错误
                SiteHelper.Log(guid, $"充值失败，返回码 Code={code}，Message={msg}", Tungyi_logFile);

                //更新数据
                //如果是10007, 表示訂單重複發送,则不對已經存在的訂單關閉
                //如果是10005, 表示統一訂單沒有正確寫入, 但FZ仍視為正常單子
                //如果返回碼非10007 or 10005,表示订单错误
                if (code.ToString() == "10007" || code.ToString() == "10005")
                {
                    //do nothing , 仍視為正常的訂單
                }
                else
                {
                    Service.CDFailOrder(order.Id, "0", $"充值失败，返回码 Code={code}，Message={outMessage}");
                }

                return string.Empty;
            }

            //更新transid null --> 0, 表示已经成功呼叫统一
            Service.CDModifyTransID(order.Id, "0");

            //取得跳转位置
            var pay_params = json["data"]["content"] != null ? json["data"]["content"].ToString() : string.Empty;

            SiteHelper.Log(guid, $"支付请求结果 [pay_params] {pay_params}", Tungyi_logFile);
            return pay_params;
        }
        
        public static string getamountlist(string guid)
        {

            //預設先從cache取得
            var cache = SiteHelper.GetCache("amountlist");
            if (cache != null)
            {
                //SiteHelper.Log(guid, string.Format("取得支付金额接口 {0}", "Cache"), Tungyi_logFile);
                return cache.ToString();
            }
            

            Dictionary<string, string> paras = new Dictionary<string, string>();

            paras.Add("accountId", Tungyi_mchid);
            paras.Add("alipayAccount", string.Empty);
            paras.Add("bankId", string.Empty);
            paras.Add("phone", string.Empty);
            paras.Add("seqId", GetTimeStamp());
            paras.Add("key", Tungyi_Key);

            string pay_md5sign = Md5(string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value))), Encoding.UTF8);

            paras.Remove("key");
            paras.Add("sign", pay_md5sign);

            var data = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            var responseString = string.Empty;

            Stream instream = null;
            StreamReader sr = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;
            Encoding encoding = Encoding.UTF8;
            // 准备请求...
            try
            {
                // 设置参数
                request = WebRequest.Create(GetAmountList + data) as HttpWebRequest;
                CookieContainer cookieContainer = new CookieContainer();
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.Method = "GET";
                request.ContentType = "application/json";
                //发送请求并获取相应回应数据
                response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                instream = response.GetResponseStream();
                sr = new StreamReader(instream, encoding);
                //返回结果网页（html）代码
                string content = sr.ReadToEnd();
                string err = string.Empty;

                responseString = content;

                var amountList = JsonConvert.DeserializeObject<Amount>(responseString);  
                var payAmountInfo = amountList.data.payAmountInfo.OrderBy(e => e.payType).ToList();

                Data payAmount = new Data()
                {
                    payAmountInfo = payAmountInfo
                };

                var result = new Amount()
                {
                    code = amountList.code,
                    msg = amountList.msg,
                    data = payAmount,
                    sign = amountList.sign
                };

                responseString = new JavaScriptSerializer().Serialize(result);
                //設定cache, 強迫15秒過期
                SiteHelper.SetCache("amountlist", responseString, DateTime.Now.AddSeconds(15));
                SiteHelper.Log(guid, string.Format("取得支付金额接口 {0}", "Query"), Tungyi_logFile);
            }
            catch (Exception ex)
            {
                SiteHelper.Log(guid, string.Format("取得支付金额接口 [error] {0}", ex), Tungyi_logFile);
            }

            return responseString;
        }

        public static string getamountlistByHonda(string guid)
        {

            //預設先從cache取得
            var cache = SiteHelper.GetCache("amountlistHonda");
            if (cache != null)
            {
                //SiteHelper.Log(guid, string.Format("取得支付金额接口 {0}", "Cache"), Tungyi_logFile);
                return cache.ToString();
            }


            Dictionary<string, string> paras = new Dictionary<string, string>();

            paras.Add("accountId", TungyiHonda_mchid);
            paras.Add("alipayAccount", string.Empty);
            paras.Add("bankId", string.Empty);
            paras.Add("phone", string.Empty);
            paras.Add("seqId", GetTimeStamp());
            paras.Add("key", TungyiHonda_Key);

            string pay_md5sign = Md5(string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value))), Encoding.UTF8);

            paras.Remove("key");
            paras.Add("sign", pay_md5sign);

            var data = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            var responseString = string.Empty;

            Stream instream = null;
            StreamReader sr = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;
            Encoding encoding = Encoding.UTF8;
            // 准备请求...
            try
            {
                // 设置参数
                request = WebRequest.Create(GetAmountList + data) as HttpWebRequest;
                CookieContainer cookieContainer = new CookieContainer();
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.Method = "GET";
                request.ContentType = "application/json";
                //发送请求并获取相应回应数据
                response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                instream = response.GetResponseStream();
                sr = new StreamReader(instream, encoding);
                //返回结果网页（html）代码
                string content = sr.ReadToEnd();
                string err = string.Empty;

                responseString = content;

                var amountList = JsonConvert.DeserializeObject<Amount>(responseString);
                var payAmountInfo = amountList.data.payAmountInfo.OrderBy(e => e.payType).ToList();

                Data payAmount = new Data()
                {
                    payAmountInfo = payAmountInfo
                };

                var result = new Amount()
                {
                    code = amountList.code,
                    msg = amountList.msg,
                    data = payAmount,
                    sign = amountList.sign
                };

                responseString = new JavaScriptSerializer().Serialize(result);
                //設定cache, 強迫15秒過期
                SiteHelper.SetCache("amountlistHonda", responseString, DateTime.Now.AddSeconds(15));
                SiteHelper.Log(guid, string.Format("取得支付金额接口 {0}", "Query"), TungyiHonda_logFile);
            }
            catch (Exception ex)
            {
                SiteHelper.Log(guid, string.Format("取得支付金额接口 [error] {0}", ex), TungyiHonda_logFile);
            }

            return responseString;
        }

        public static string getamountlistByDF(string guid)
        {

            //預設先從cache取得
            var cache = SiteHelper.GetCache("amountlistDF");
            if (cache != null)
            {
                //SiteHelper.Log(guid, string.Format("取得支付金额接口 {0}", "Cache"), Tungyi_logFile);
                return cache.ToString();
            }


            Dictionary<string, string> paras = new Dictionary<string, string>();

            paras.Add("accountId", TungyiDF_mchid);
            paras.Add("alipayAccount", string.Empty);
            paras.Add("bankId", string.Empty);
            paras.Add("phone", string.Empty);
            paras.Add("seqId", GetTimeStamp());
            paras.Add("key", TungyiDF_Key);

            string pay_md5sign = Md5(string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value))), Encoding.UTF8);

            paras.Remove("key");
            paras.Add("sign", pay_md5sign);

            var data = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            var responseString = string.Empty;

            Stream instream = null;
            StreamReader sr = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;
            Encoding encoding = Encoding.UTF8;
            // 准备请求...
            try
            {
                // 设置参数
                request = WebRequest.Create(GetAmountList + data) as HttpWebRequest;
                CookieContainer cookieContainer = new CookieContainer();
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.Method = "GET";
                request.ContentType = "application/json";
                //发送请求并获取相应回应数据
                response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                instream = response.GetResponseStream();
                sr = new StreamReader(instream, encoding);
                //返回结果网页（html）代码
                string content = sr.ReadToEnd();
                string err = string.Empty;

                responseString = content;

                var amountList = JsonConvert.DeserializeObject<Amount>(responseString);
                var payAmountInfo = amountList.data.payAmountInfo.OrderBy(e => e.payType).ToList();

                Data payAmount = new Data()
                {
                    payAmountInfo = payAmountInfo
                };

                var result = new Amount()
                {
                    code = amountList.code,
                    msg = amountList.msg,
                    data = payAmount,
                    sign = amountList.sign
                };

                responseString = new JavaScriptSerializer().Serialize(result);
                //設定cache, 強迫15秒過期
                SiteHelper.SetCache("amountlistDF", responseString, DateTime.Now.AddSeconds(15));
                SiteHelper.Log(guid, string.Format("取得支付金额接口 {0}", "Query"), TungyiDF_logFile);
            }
            catch (Exception ex)
            {
                SiteHelper.Log(guid, string.Format("取得支付金额接口 [error] {0}", ex), TungyiDF_logFile);
            }

            return responseString;
        }

        public static PayState call(string logGuid)
        {
            try
            {
                var requestType = HttpContext.Current.Request.RequestType;

                if (requestType == "GET")
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", System.Web.HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + System.Web.HttpContext.Current.Request.QueryString[key]))), Tungyi_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", System.Web.HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + System.Web.HttpContext.Current.Request.Form[key]))), Tungyi_logFile);
                }

               
                var playerId = HttpContext.Current.Request["playerId"];
                var orderId = HttpContext.Current.Request["orderId"];
                var teamOrderId = HttpContext.Current.Request["teamOrderId"];
                var orderAmount = HttpContext.Current.Request["orderAmount"];
                var realAmount = HttpContext.Current.Request["realAmount"];

                /*
                    订单状态:0失败,1成功,2金额不匹配(即创建订单时金额为10元,但是收到回调时可能只有9.9,一般为渠道侧为了风控故意减去了几分钱,这种情况也可视为成功)
                    */
                var status = HttpContext.Current.Request["status"];
                var time = HttpContext.Current.Request["time"];
                var sign = HttpContext.Current.Request["sign"];

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(teamOrderId));
                var tungyi_Key = string.Empty;

                switch (order.GWID)
                {
                    case (int)PayGatewayType.Tungyi:                      
                        tungyi_Key = Tungyi_Key;
                        break;
                    case (int)PayGatewayType.TungyiHonda:
                        tungyi_Key = TungyiHonda_Key;
                        break;
                    case (int)PayGatewayType.TungyiDF:
                        tungyi_Key = TungyiDF_Key;
                        break;
                }

                if (!CheckSign(logGuid, tungyi_Key))
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), Tungyi_logFile);
                    return PayState.Error;
                }

                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("訂單狀態已經完成,無須金幣充值"), Tungyi_logFile);
                    return PayState.Success;
                }

                //支付失败
                if (status == "0")
                {
                    SiteHelper.Log(logGuid, string.Format("支付失败"), Tungyi_logFile);
                    Service.CDFailOrder(order.Id, orderId, string.Format("支付失败 playerId:{0} , 统一訂單:{1} , orderAmount:{2}, realAmount:{3} ,status={4}", playerId, orderId, orderAmount, realAmount, status));
                    return PayState.Error;
                }

                //進行金幣充值
                string msg = "";
                if (status == "1" && orderAmount == realAmount && order.Amount == (long.Parse(realAmount) / 100))
                {
                    var data = string.Format("完成支付: playerId:{0} , 统一訂單:{1}, orderAmount:{2}, realAmount:{3}, status={4}", playerId, orderId, orderAmount, realAmount, status);

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, orderId, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, string.Format("金幣充值成功 orderid={0} amount={1}", order.Id, order.Amount), Tungyi_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, string.Format("金幣充值错误 {0}", msg), Tungyi_logFile);
                        return PayState.Error;
                    }
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付無效"), Tungyi_logFile);
                    Service.CDFailOrder(order.Id, orderId, string.Format("支付無效 playerId:{0} , 统一訂單:{1} , orderAmount:{2}, realAmount:{3} ,status={4}", playerId, orderId, orderAmount, realAmount, status));
                    return PayState.Error;
                }                
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调异常错误 {0}", ex.ToString()), Tungyi_logFile);
                return PayState.Error;
            }
        }

        //久聊訂單回調
        public static PayState AgentCall(string logGuid)
        {
            try
            {
                var requestType = HttpContext.Current.Request.RequestType;

                if (requestType == "GET")
                {
                    SiteHelper.Log(logGuid, string.Format("統一代理订单結果通知 [GetData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), Tungyi_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("統一代理订单結果通知 [PostData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), Tungyi_logFile);
                }

                
                var playerId = HttpContext.Current.Request["playerId"];
                var orderId = HttpContext.Current.Request["orderId"]; //支付中心訂單
                var teamOrderId = HttpContext.Current.Request["teamOrderId"]; //項目組訂單
                var orderAmount = HttpContext.Current.Request["orderAmount"];
                var payType = HttpContext.Current.Request["payType"]; //订单类型3：转账,4：收银台

                /*
                订单状态:0失败,1成功,2金额不匹配
                */
                var status = HttpContext.Current.Request["status"];
                var time = HttpContext.Current.Request["time"];
                var sign = HttpContext.Current.Request["sign"];

                var order = Service.CDGetPayOrderByID(Convert.ToInt32(teamOrderId));
                var tungyi_Key = order.GWID == (int)PayGatewayType.Tungyi ? Tungyi_Key : TungyiHonda_Key;

                if (!CheckSign(logGuid, tungyi_Key))
                {
                    SiteHelper.Log(logGuid, string.Format("統一代理支付回调非法签名"), Tungyi_logFile);
                    return PayState.Error;
                }

                //進行金幣充值
                string msg = "";
                string data = "";
                int finishStatus = 1; //待支付  Waiting = 1, 已支付 Paied = 2, 支付失败 fail = 3 
                if (status == "1")
                {
                    data = $"統一代理支付完成 playerId={playerId} payType={payType} orderId={orderId} orderAmount={orderAmount}, status={status}";
                    finishStatus = 2;
                }
                //支付失败
                else if (status == "0")
                {
                    data = $"統一代理支付失敗 playerId={playerId} payType={payType} orderId={orderId} orderAmount={orderAmount}, status={status}";
                    finishStatus = 3;
                }
                else
                {
                    data = $"統一代理支付無效 playerId={playerId} payType={payType} orderId={orderId} orderAmount={orderAmount}, status={status}";
                    finishStatus = 3;
                }

                //紀錄結果
                SiteHelper.Log(logGuid, data, Tungyi_logFile);


                //有两种支付方式 ：
                //第一种是收银台[payType = 4]，
                //这种订单是你们先生成订单号，然后再到久聊下单，最后支付中心回调你们的时候会携带这订单号，你们根据这种订单号做判断
                //第二种是转帐[payType = 3]，
                //这种你们没有生成订单，是由支付中心生成的订单，所以回调的时候只有支付中心的订单号，没有项目组订单号，这种情况你们直接给用户加金币就行了，不需要判断订单是否存在
                if (Convert.ToInt32(payType) == 3)
                    teamOrderId = "0";

                //2018-10-09 支付中心改傳orderid當作項目組訂單號
                if (Convert.ToInt32(payType) == 4)
                    teamOrderId = orderId;

                if (Service.CDFinishThirdPartyOrder(Convert.ToInt32(payType), Convert.ToInt32(teamOrderId), playerId, orderId, Convert.ToInt64(orderAmount), SystemHelper.UserIPAddress, data, finishStatus.ToString(), ref msg) == true)
                {
                    SiteHelper.Log(logGuid, string.Format("統一代理金幣充值成功 orderid={0} amount={1}", orderId, orderAmount), Tungyi_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("統一代理金幣充值错误 {0}", msg), Tungyi_logFile);
                }

                return PayState.Success;
                
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("統一代理支付回调异常错误 {0}", ex.ToString()), Tungyi_logFile);
                return PayState.Error;
            }
        }

        /// <summary>
        /// 统一打款回调
        /// </summary>
        /// <param name="logGuid"></param>
        /// <param name="orderId"></param>
        /// <param name="teamOrderId"></param>
        /// <param name="orderAmount"></param>
        /// <param name="realAmount"></param>
        /// <param name="status"></param>
        /// <param name="time"></param>
        /// <param name="sign"></param>
        /// <returns></returns>
        public static PayState WithdrawCallback(string logGuid, string orderId, string teamOrderId, int orderAmount, int realAmount, string status, string time, string sign, string key)
        {
            try
            {
                var msg = string.Empty;

                if (!CheckSign(logGuid, key))
                {
                    SiteHelper.Log(logGuid, string.Format("統一提现异步回调非法签名"), Tungyi_logFile);
                    return PayState.SignError;
                }

                int intUpdStatus = -1;
                if (status == "success") intUpdStatus = 3;
                else if (status == "pending") intUpdStatus = 99;
                else if(status == "fail") intUpdStatus = 100;

                if(intUpdStatus == -1)
                {
                    SiteHelper.Log(logGuid, $"統一提现狀態無效 {status}", Tungyi_logFile);
                    return PayState.Error;
                }

                var res = Service.WithdrawCallback(orderId, intUpdStatus, teamOrderId, status);
                if (res.Result)
                {
                    SiteHelper.Log(logGuid, $"統一提现回调成功 orderid={orderId}&teamOrderId={teamOrderId}&orderAmount={orderAmount}&realAmount={realAmount}&status={status}&time={time}", Tungyi_logFile);
                    return PayState.Success;
                }
                else
                {
                    SiteHelper.Log(logGuid, $"統一提现回调错误 {res.Message}", Tungyi_logFile);
                    return PayState.Error;
                }
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("統一提现异步回调异常错误 {0}", ex.ToString()), Tungyi_logFile);
                return PayState.Error;
            }
        }

        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        private static string Md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }

        private static bool CheckSign(string guid, string tungyi_Key)
        {

            var requestType = HttpContext.Current.Request.RequestType;
            Dictionary<string, string> parameters = null;

            if (requestType == "GET")
            {
                parameters = HttpContext.Current.Request.QueryString.AllKeys.Cast<string>()
             .ToDictionary(k => k, v => HttpContext.Current.Request[v]).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            }
            else
            {
                parameters = HttpContext.Current.Request.Form.AllKeys.Cast<string>()
               .ToDictionary(k => k, v => HttpContext.Current.Request[v]).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            }

            parameters.Remove("sign");

            var sign = HttpContext.Current.Request["sign"];
            var str = string.Join("&", parameters.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            str = str + string.Format("&key={0}", tungyi_Key);
            var cSign = Md5(str, Encoding.UTF8);

            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str), Tungyi_logFile);
            return sign == cSign;
        }

        public static string amountlist(string guid, int platformID)
        {
            var responseString = string.Empty;

            try
            {              
                var gwid = 0;
                var data = new List<PayAmountInfo>();
                var listAmount = new Amount() { data = new Data() };                    
                var list = SiteHelper.GetAPIResult<List<PayGateway>>($"/Pay/GetAmountList?platformID={platformID}");                

                //是否開啟統一支付
                if (TungyiEnable == "1")
                {
                    if (list.Any(x => x.GWID == (int)PayGatewayType.Tungyi))
                    {
                        //統一支付
                        listAmount = JsonConvert.DeserializeObject<Amount>(getamountlist(guid));
                        gwid = (int)PayGatewayType.Tungyi;
                    }
                    else if (list.Any(x => x.GWID == (int)PayGatewayType.TungyiHonda))
                    {
                        //轰达統一
                        listAmount = JsonConvert.DeserializeObject<Amount>(getamountlistByHonda(guid));
                        gwid = (int)PayGatewayType.TungyiHonda;
                    }                    
                    else if (list.Any(x => x.GWID == (int)PayGatewayType.TungyiDF))
                    {
                        //巔峰統一
                        listAmount = JsonConvert.DeserializeObject<Amount>(getamountlistByDF(guid));
                        gwid = (int)PayGatewayType.TungyiDF;
                    }

                    foreach (var i in listAmount.data.payAmountInfo)
                    {
                        var payGateway = list.FirstOrDefault(x => x.GWID == gwid && x.TradeCode == i.payType.ToString());
                        if (payGateway != null)
                        {
                            i.gwid = payGateway.GWID;
                            i.popular = payGateway.Popular;
                            i.sort = payGateway.Sort;
                            i.typeID = payGateway.TypeID;
                            data.Add(i);
                        }
                    }
                }

                foreach (var i in list.Where(x => !(new int[] { 6, 58, 86 }).Contains(x.GWID)))
                {
                    if (data.Any(x => x.typeID == i.TypeID))
                        continue;

                    var payInfo = new PayAmountInfo()
                    {
                        gwid = i.GWID,
                        typeID = i.TypeID,
                        popular = i.Popular,
                        sort = i.Sort,
                        payType = Convert.ToInt32(i.TradeCode),
                        amounts = i.AmountSort,
                        minAmount = Convert.ToInt32(i.MinValue),
                        maxAmount = Convert.ToInt32(i.MaxValue),
                        status = i.Status ? 1 : 0
                    };

                    data.Add(payInfo);
                }

                listAmount.data.payAmountInfo = data;

                responseString = JsonConvert.SerializeObject(listAmount);               
            }
            catch (Exception ex)
            {
                SiteHelper.Log(guid, string.Format("取得支付金额接口amountlist [error] {0}", ex), Tungyi_logFile);
            }

            return responseString;
        }

        public static List<PayPoints> GetPayPointConverts()
        {
            var res = SiteHelper.GetAPIResult<List<PayPoints>>($"/Pay/GetPayPointConverts");
            return res;
        }

        public static string PayAmountlist(string guid, int platformID)
        {
            var responseString = string.Empty;

            try
            {
                var gwid = 0;
                var data = new List<PayAmountInfo>();
                var listAmount = new Amount { data = new Data { payAmountInfo = new List<PayAmountInfo>() } };
                var list = SiteHelper.GetAPIResult<List<PayGateway>>($"/Pay/GetPayAmountList?platformID={platformID}");
                var listPayPointConverts = GetPayPointConverts();
                //是否開啟統一支付
                if (TungyiEnable == "1")
                {
                    if (list.Any(x => x.GWID == (int)PayGatewayType.Tungyi))
                    {
                        //統一支付
                        listAmount = JsonConvert.DeserializeObject<Amount>(getamountlist(guid));
                        gwid = (int)PayGatewayType.Tungyi;
                    }
                    else if (list.Any(x => x.GWID == (int)PayGatewayType.TungyiHonda))
                    {
                        //轰达統一
                        listAmount = JsonConvert.DeserializeObject<Amount>(getamountlistByHonda(guid));
                        gwid = (int)PayGatewayType.TungyiHonda;
                    }
                    else if (list.Any(x => x.GWID == (int)PayGatewayType.TungyiDF))
                    {
                        //巔峰統一
                        listAmount = JsonConvert.DeserializeObject<Amount>(getamountlistByDF(guid));
                        gwid = (int)PayGatewayType.TungyiDF;
                    }

                    if (listAmount?.data?.payAmountInfo != null)
                    {
                        foreach (var i in listAmount.data.payAmountInfo)
                        {
                            var payGateway = list.FirstOrDefault(x => x.GWID == gwid && x.TradeCode == i.payType.ToString());
                            if (payGateway != null)
                            {
                                i.gwid = payGateway.GWID;
                                i.gatewayName = payGateway.GatewayName;
                                i.typeID = payGateway.TypeID;
                                i.typeName = payGateway.TypeName;
                                i.tradeCode = payGateway.TradeCode;
                                i.popular = payGateway.Popular;
                                i.sort = payGateway.Sort;
                                i.isRange = payGateway.IsRange ? 1 : 0;
                                i.priority = payGateway.Priority;
                                i.amountsPoint = listPayPointConverts.Where(item => item.PayTypeID == payGateway.TypeID).ToList();
                                i.currencyCode = payGateway.CurrencyCode;
                                i.isMustBeConverted = payGateway.IsMustBeConverted;
                                data.Add(i);
                            }
                        }
                    }
                }
                
                foreach (var i in list.Where(x => !(new int[] { 6, 58, 86 }).Contains(x.GWID)))
                {
                    var payInfo = new PayAmountInfo()
                    {
                        gwid = i.GWID,
                        gatewayName = i.GatewayName,
                        typeID = i.TypeID,
                        typeName = i.TypeName,                     
                        tradeCode = i.TradeCode,
                        amounts = i.AmountSort,
                        amountsPoint = listPayPointConverts.Where(item=>item.PayTypeID == i.TypeID).ToList(),
                        currencyCode = i.CurrencyCode,
                        isMustBeConverted = i.IsMustBeConverted,
                        maxAmount = Convert.ToInt32(i.MaxValue),
                        status = i.Status ? 1 : 0,
                        popular = i.Popular,
                        sort = i.Sort,
                        isRange = i.IsRange ? 1 : 0,
                        priority = i.Priority
                    };

                    data.Add(payInfo);
                }
                listAmount = listAmount == null ? new Amount { data = new Data { payAmountInfo = new List<PayAmountInfo>() } } : listAmount;
                listAmount.data.payAmountInfo = data;

                responseString = JsonConvert.SerializeObject(listAmount);
            }
            catch (Exception ex)
            {
                SiteHelper.Log(guid, string.Format("取得支付金额接口amountlist [error] {0}", ex), Tungyi_logFile);
            }

            return responseString;
        }
    }
}