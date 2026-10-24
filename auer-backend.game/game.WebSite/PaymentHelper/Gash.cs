using game.Entity.Pay;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    public class Gash
    {
        public static string post(CDPayOrder cDPayOrder, string orderCurrency, string guid)
        {
            TransactionKey key = new TransactionKey()
            {
                Key = Gash_Key,
                IV = Gash_IV,
                Password = [REDACTED],
            };

            var strXmlDoc = "<?xml version=\"1.0\"?><TRANS>";
            strXmlDoc += "<MSG_TYPE>" + "0100" + "</MSG_TYPE>";
            strXmlDoc += "<PCODE>" + "300000" + "</PCODE>";
            strXmlDoc += "<CID>" + Gash_CID + "</CID>";
            strXmlDoc += "<COID>" + cDPayOrder.Id + "_" + DateTime.Now.Millisecond.ToString() + "</COID>";
            strXmlDoc += "<CUID>" + orderCurrency + "</CUID>";

            switch (cDPayOrder.PayTypeID)
            {
                case (int)PayType.COPGAM05:
                    strXmlDoc += "<PAID>" + PaidType.COPGAM05.ToString() + "</PAID>";
                    break;
                // *注意!!使用 COPGAM09交易參數傳遞上，
                // 使用者的遊戲帳號必須填入欄位 USER_ACCTID 並帶給 GPS
                // 且需確保為唯 一值，避免儲值進錯誤的遊戲帳號。
                case (int)PayType.COPGAM09:
                    strXmlDoc += "<PAID>" + PaidType.COPGAM09.ToString() + "</PAID>";
                    strXmlDoc += "<USER_ACCTID>" + cDPayOrder.UserID + "</USER_ACCTID>"; // 商家 User 帳號
                    break;
                case (int)PayType.BNK00701:
                    strXmlDoc += "<PAID>" + PaidType.BNK00701.ToString() + "</PAID>";
                    break;
                case (int)PayType.BNK82204:
                    strXmlDoc += "<PAID>" + PaidType.BNK82204.ToString() + "</PAID>";
                    break;
                default:
                    break;
            }


            strXmlDoc += "<AMOUNT>" + cDPayOrder.Amount.ToString("0.00") + "</AMOUNT>";
            strXmlDoc += "<ERQC></ERQC>";
            strXmlDoc += "<RETURN_URL>" + Gash_Callback_Url + "</RETURN_URL>";
            strXmlDoc += "<ORDER_TYPE>" + "M" + "</ORDER_TYPE>";
            strXmlDoc += "</TRANS>";


            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(strXmlDoc);
            var szERQC = GetERQC(xmlDoc, key);
            xmlDoc.DocumentElement["ERQC"].InnerText = szERQC;

            var szHtml = "<html xmlns=\"http://example.invalid/1999/xhtml\"><head><title>Credits Platform</title></head><body>";
            szHtml += String.Format("<form name=\"aspnetForm\" method=\"post\" action=\"{0}\">", Gash_Order_Url);
            szHtml += String.Format("<input type=\"hidden\" id=\"data\" name=\"data\" value=\"{0}\" />", GetSendData(xmlDoc));
            szHtml += "<script type=\"text/javascript\">document.forms[0].submit();</script></form>";
            szHtml += "</body></html>";

            SiteHelper.Log(guid, $"{cDPayOrder.Id} 支付请求 [data] {szHtml}", Gash_Log_File);
            return szHtml;
        }

        public static PayState call(string logGuid)
        {
            try
            {
                var requestType = HttpContext.Current.Request.RequestType; Dictionary<string, string> parameters = null;
                switch (requestType)
                {
                    case "GET":
                        parameters = HttpContext.Current.Request.QueryString.AllKeys.Cast<string>()
                        .ToDictionary(k => k, v => HttpContext.Current.Request[v]).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                        break;
                    default:
                        parameters = HttpContext.Current.Request.Form.AllKeys.Cast<string>()
                      .ToDictionary(k => k, v => HttpContext.Current.Request[v]).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                        break;
                }
                var requestData = string.Empty;
                foreach (string s in parameters.Keys)
                {
                    requestData += s + "=" + parameters[s];
                }
                SiteHelper.Log(logGuid, $"支付回调 [FormData] {requestData}", Gash_Log_File);

                XmlSerializer transReponse = new XmlSerializer(typeof(TransReponse));
                var base64 = parameters.ContainsKey("data") ? parameters["data"] : string.Empty;
                string c = Encoding.GetEncoding("utf-8").GetString(Convert.FromBase64String(base64));
                TransReponse trans = new TransReponse();
                using (StringReader reader = new StringReader(c))
                {
                    trans = (TransReponse)(transReponse.Deserialize(reader));
                }

                var orderStatus = false;
                // 需進行訂單查詢作業，收到交易成功，須再進行請款。 
                if (trans.PAY_STATUS == "0" || trans.PAY_STATUS == "W")
                {
                    // 查單
                    var checkOrderXmlDoc = "<TRANS>";
                    checkOrderXmlDoc += "<MSG_TYPE>" + "0100" + "</MSG_TYPE>";
                    checkOrderXmlDoc += "<PCODE>" + "200000" + "</PCODE>";
                    checkOrderXmlDoc += "<CID>" + trans.CID + "</CID>";
                    checkOrderXmlDoc += "<COID>" + trans.COID + "</COID>";
                    checkOrderXmlDoc += "<CUID>" + trans.CUID + "</CUID>";
                    checkOrderXmlDoc += "<PAID>" + trans.PAID + "</PAID>";
                    checkOrderXmlDoc += "<AMOUNT>" + trans.AMOUNT.ToString("0.00") + "</AMOUNT>";
                    checkOrderXmlDoc += "<ERQC></ERQC>"; // ******請款請求是帶ERQC不是ERPC
                    checkOrderXmlDoc += "</TRANS>";

                    XmlDocument checkOrderXml = new XmlDocument();
                    checkOrderXml.LoadXml(checkOrderXmlDoc);
                    TransactionKey checlPrderKey = new TransactionKey()
                    {
                        Key = Gash_Key,
                        IV = Gash_IV,
                        Password = [REDACTED],
                    };
                    var szCheckorderERQC = GetERQC(checkOrderXml, checlPrderKey);
                    checkOrderXml.DocumentElement["ERQC"].InnerText = szCheckorderERQC;

                    var resCheckOrderBase64 = GetSendData(checkOrderXml);

                    SiteHelper.Log(logGuid, $"查單请求 [base64] {resCheckOrderBase64}", Gash_Log_File);
                    var resCheckOrder = InvokeService(resCheckOrderBase64, CheckOrder_Url);
                    SiteHelper.Log(logGuid, $"查單回應 [FormData] {resCheckOrder.InnerXml}", Gash_Log_File);

                    XmlSerializer orderCheckReponse = new XmlSerializer(typeof(TransReponse));
                    TransReponse orderChecktrans = new TransReponse();
                    using (StringReader reader = new StringReader(c))
                    {
                        orderChecktrans = (TransReponse)(orderCheckReponse.Deserialize(reader));
                    }
                    if (orderChecktrans.PAY_RCODE == "0000" && orderChecktrans.PAY_STATUS == "S") 
                    {
                        orderStatus = true;
                    }
                }

                if (!(trans.PAY_RCODE == "0000" && trans.PAY_STATUS == "S") && !orderStatus)
                {
                    SiteHelper.Log(logGuid, $"{trans.COID} GASH回调错误 {trans.RMSG_CHI}", Gash_Log_File);
                    return PayState.Error;
                }
                else
                {
                    // update player's wallet
                    var order = Service.CDGetPayOrderByID(Convert.ToInt32((trans.COID.Split('_')[0])));
                    if (order.Status == CDOrderStatus.Paied)
                    {
                        // 防止對方重複打
                        SiteHelper.Log(logGuid, $"{trans.COID} 已經完成訂單 {trans.RMSG_CHI}", Gash_Log_File);
                        return PayState.Success;
                    }
                    var data = $"GASH回调 完成支付: 支付订单号: {order.Id} , 渠道订单号: {trans.RRN} 状态: success";
                    string msg = "";
                    if (!(Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, trans.RRN, data, ref msg)))
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} GASH回调 金幣充值错误 {msg}", Gash_Log_File);
                        Service.CDFailOrder(order.Id, trans.RRN, $"GASH订单号:{order.Id} , 渠道订单号: {trans.RRN} , 支付金额: {order.Amount}, 状态: fail");
                        return PayState.Error;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} GASH回调 金幣充值成功 支付订单号:{order.Id} 金额:amount={order.Amount} 渠道订单号:{trans.RRN}", Gash_Log_File);
                        // 接收到GPS 系統回傳的PAY_STATUS 為S 且RCODE =0000 時，接續進行請款作業。
                        var strXmlDoc = "<TRANS>";
                        strXmlDoc += "<MSG_TYPE>" + "0500" + "</MSG_TYPE>";
                        strXmlDoc += "<PCODE>" + "300000" + "</PCODE>";
                        strXmlDoc += "<CID>" + trans.CID + "</CID>";
                        strXmlDoc += "<COID>" + trans.COID + "</COID>";
                        strXmlDoc += "<CUID>" + trans.CUID + "</CUID>";
                        strXmlDoc += "<PAID>" + trans.PAID + "</PAID>";
                        strXmlDoc += "<AMOUNT>" + trans.AMOUNT.ToString("0.00") + "</AMOUNT>";
                        strXmlDoc += "<ERQC></ERQC>"; // ******請款請求是帶ERQC不是ERPC
                        strXmlDoc += "</TRANS>";

                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(strXmlDoc);
                        TransactionKey key = new TransactionKey()
                        {
                            Key = Gash_Key,
                            IV = Gash_IV,
                            Password = [REDACTED],
                        };
                        var szERQC = GetERQC(xmlDoc, key);
                        xmlDoc.DocumentElement["ERQC"].InnerText = szERQC;

                        SiteHelper.Log(logGuid, $"請款请求 [FormData] {xmlDoc.InnerXml}", Gash_Log_File);
                        var resBase64 = GetSendData(xmlDoc);
                        SiteHelper.Log(logGuid, $"請款请求 [base64] {resBase64}", Gash_Log_File);
                        var res = InvokeService(resBase64, GashSettle_Url);
                        SiteHelper.Log(logGuid, $"請款回應 [FormData] {res.InnerXml}", Gash_Log_File);
                        c = Encoding.GetEncoding("utf-8").GetString(Convert.FromBase64String(res.InnerText));
                        SiteHelper.Log(logGuid, $"請款回應 [base64] {c}", Gash_Log_File);
                        XmlSerializer settleReponse = new XmlSerializer(typeof(EnvelopeReponse));
                        EnvelopeReponse envelope = new EnvelopeReponse();
                        using (StringReader reader = new StringReader(c))
                        {
                            envelope = (EnvelopeReponse)(settleReponse.Deserialize(reader));
                            /*
                            *  若接收到GPS 系統回傳請款交易的RCODE=9004、RCODE=9997、RCODE =9998、 RCODE=9999 或未收到GPS 系統回覆請款結果時，請再次發動請款交易直到請款成功為止。
                            *  建議每5 分鐘針對上述情況的交易成功單再次發動請款，以避免有漏單情況發生。 
                           */
                            settleReponse = new XmlSerializer(typeof(SettleRequest));
                            SettleRequest request = new SettleRequest();
                            using (StringReader r = new StringReader(xmlDoc.InnerXml))
                            {
                                request = (SettleRequest)(settleReponse.Deserialize(r));
                            }
                            if (!(envelope.RCODE == "0000" && envelope.PAY_STATUS == "S"))
                            {
                                SiteHelper.Log(logGuid, $"請款失敗 [FormData] {c}", Gash_Log_File);
                                Service.CreateGashPaySettleStatus(request, false);
                            }
                            else
                            {
                                Service.CreateGashPaySettleStatus(request, true);
                                // update order status
                                SiteHelper.Log(logGuid, $"請款成功 [FormData] {c}", Gash_Log_File);
                            }
                        }
                    }
                }
                return PayState.Success;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("CASH回调 异常错误 {0}", ex.ToString()), Gash_Log_File);
                return PayState.Error;
            }
        }

        [XmlRoot(ElementName = "TRANS")]
        public class TransRequest
        {
            public byte MSG_TYPE { get; set; }
            public uint PCODE { get; set; }
            public string CID { get; set; }
            public string COID { get; set; }
            public string CUID { get; set; }
            public string PAID { get; set; }
            public string AMOUNT { get; set; }
            public string ERQC { get; set; }
            public string RETURN_URL { get; set; }
            public OrderType ORDER_TYPE { get; set; }
        }

        [XmlRoot(ElementName = "TRANS")]
        public class SettleRequest
        {
            public string MSG_TYPE { get; set; }
            public string PCODE { get; set; }
            public string CID { get; set; }
            public string COID { get; set; }
            public string CUID { get; set; }
            public string PAID { get; set; }
            public string AMOUNT { get; set; }
            public string ERQC { get; set; }
        }

        public enum PaidType
        {
            [Description("COPGAM05")]
            COPGAM05 = 41,
            [Description("COPGAM09")]
            COPGAM09 = 42,
            [Description("BNK00701")]
            BNK00701 = 43,
            [Description("BNK82204")]
            BNK82204 = 44,
        }

        public enum OrderType
        {
            /// <summary>
            ///  表示指定付款代收業者
            /// </summary>
            [Description("M")]
            M = 1,
            /// <summary>
            /// 表示不指定付款代收業者 
            /// </summary>
            [Description("E")]
            E = 2,
        }

        [XmlRoot(ElementName = "TRANS")]
        public class TransReponse
        {
            public string CID { get; set; }
            public string COID { get; set; }
            public string CUID { get; set; }
            public decimal AMOUNT { get; set; }
            public string PAID { get; set; }
            /// <remarks/>
            public string PAY_STATUS { get; set; }

            /// <remarks/>
            public string PAY_RCODE { get; set; }

            /// <remarks/>
            public string RCODE { get; set; }

            public string ERPC { get; set; }
            public string RMSG_CHI { get; set; }
            public string RRN { get; set; }
        }

        [XmlRoot(ElementName = "TRANS")]
        public class EnvelopeReponse
        {
            public string MSG_TYPE { get; set; }
            public string PCODE { get; set; }
            public string CID { get; set; }
            public string COID { get; set; }
            public string RRN { get; set; }
            public string CUID { get; set; }
            public string PAID { get; set; }
            public string AMOUNT { get; set; }
            public string ERPC { get; set; }
            public string PAY_STATUS { get; set; }
            public string RCODE { get; set; }
        }

        public class TransactionKey
        {
            public string Password; //交易密碼
            public string Key;      //交易密鑰1
            public string IV;       //交易密鑰2
        }

        public static String GetERQC(XmlDocument xmlDoc, TransactionKey key)
        {
            String strCID;
            String strCOID;
            String strCUID;
            String strAmt;

            strCID = "";
            strCOID = "";
            strCUID = "";
            strAmt = "";

            #region ERQC Data 組成為 「CID + COID + CUID(3) + AMOUNT(12,2) + PASSWORD」
            // ERQC Data 組成為 「CID + COID + CUID(3) + AMOUNT(12,2) + PASSWORD」
            // ===============================================================================
            /* 修正加密驗證方式
                * 
                * Note:
                * 條件：前期需已完成資料之檢核
                * 1. CID : Content ID
                * 2. COID : Content Order ID
                * 3. CUID : ISO Alpha Currency code (ex: TWD)
                * 4. AMOUNT : 12 碼整數 + 2 碼小數 , 不含小數點
                * 5. PASSWORD : [REDACTED] 所屬的 MID 密碼
                * 
                */
            // ===============================================================================
            #endregion

            if (xmlDoc.GetElementsByTagName("CID").Count > 0)
                strCID = xmlDoc.GetElementsByTagName("CID").Item(0).InnerText;

            if (xmlDoc.GetElementsByTagName("COID").Count > 0)
                strCOID = xmlDoc.GetElementsByTagName("COID").Item(0).InnerText;

            if (xmlDoc.GetElementsByTagName("CUID").Count > 0)
                strCUID = xmlDoc.GetElementsByTagName("CUID").Item(0).InnerText;

            if (xmlDoc.GetElementsByTagName("AMOUNT").Count > 0)
                strAmt = xmlDoc.GetElementsByTagName("AMOUNT").Item(0).InnerText;

            return GetERQC(strCID, strCOID, strCUID, strAmt, key);
        }

        private static String GetERQC(String strCID, String strCOID, String strCUID, String strAmt, TransactionKey key)
        {
            String strDatas;
            String strKey;
            String strIV;
            String strRtnValue;

            String strPassword;

            strDatas = "{0}{1}{2}{3}{4}";
            strRtnValue = "";

            strPassword = "";

            #region ERQC Data 組成為 「CID + COID + CUID(3) + AMOUNT(12,2) + PASSWORD」
            // ERQC Data 組成為 「CID + COID + CUID(3) + AMOUNT(12,2) + PASSWORD」
            // ===============================================================================
            /* 修正加密驗證方式
                * 
                * Note:
                * 條件：前期需已完成資料之檢核
                * 1. CID : Content ID
                * 2. COID : Content Order ID
                * 3. CUID : ISO Alpha Currency code (ex: TWD)
                * 4. AMOUNT : 12 碼整數 + 2 碼小數 , 不含小數點
                * 5. PASSWORD : [REDACTED] 所屬的 MID 密碼
                * 
                */
            // ===============================================================================
            #endregion

            // 驗證用的 AMOUNT 需整理成 14 碼
            if (strAmt.Contains("."))
            {
                strAmt = strAmt.Substring(0, strAmt.IndexOf(".")) + ((strAmt.Length - strAmt.IndexOf(".")) > 3 ? strAmt.Substring(strAmt.IndexOf(".") + 1, 2) : strAmt.Substring(strAmt.IndexOf(".") + 1).PadRight(2, '0'));
                strAmt = strAmt.PadLeft(14, '0');
            }
            else
            {
                strAmt = (strAmt + "00").PadLeft(14, '0');
            }

            strPassword = [REDACTED];
            strKey = key.Key;
            strIV = key.IV;

            strDatas = string.Format(strDatas, strCID, strCOID, strCUID, strAmt, strPassword);

            strRtnValue = Cryptography.MAC(Cryptography.TripleDESEncrypt(strDatas, strKey, strIV));

            return strRtnValue;
        }

        public static String GetSendData(XmlDocument xmlDoc)
        {
            StringWriter sw = new StringWriter();
            XmlTextWriter xw = new XmlTextWriter(sw);
            xmlDoc.WriteTo(xw);
            string returnValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(sw.ToString()));
            xw.Close();
            sw.Close();
            return returnValue;
        }

        public static XmlDocument InvokeService(string data, string url )
        {
            HttpWebRequest request = CreateSOAPWebRequest(url);
            XmlDocument SOAPReqBody = new XmlDocument();
            var strXmlDoc = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            strXmlDoc += "<soap:Envelope xmlns:xsi=\"http://example.invalid/2001/XMLSchema-instance\" xmlns:xsd=\"http://example.invalid/2001/XMLSchema\" xmlns:soap=\"http://example.invalid/soap/envelope/\">";        
            strXmlDoc += "<soap:Body><getResponse xmlns=\"http://example.invalid/\"><data>" + data + "</data></getResponse></soap:Body></soap:Envelope>";

            SOAPReqBody.LoadXml(strXmlDoc);
            using (Stream stream = request.GetRequestStream())
            {
                SOAPReqBody.Save(stream);
            }

            using (WebResponse serviceres = request.GetResponse())
            {
                using (StreamReader rd = new StreamReader(serviceres.GetResponseStream()))
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)serviceres).StatusCode;
                    //reading stream  
                    var soapXml = rd.ReadToEnd();XmlDocument soapEnvelop = new XmlDocument();
                    soapEnvelop.LoadXml(soapXml);
                    return soapEnvelop;
                }
            }
        }

        public static HttpWebRequest CreateSOAPWebRequest(string url)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(url);
            Req.Headers.Add(@"SOAPAction:http://example.invalid/getResponse");
            Req.ContentType = "text/xml;charset=\"utf-8\"";
            Req.Accept = "text/xml";
            Req.Method = "POST";
            return Req;
        }
    }

    internal class Cryptography
    {
        public static String SpecialTripleDESEncrypt(String data, String key, String iv)
        {
            byte[] byResult = null;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                ICryptoTransform cryptoTransform = null;
                TripleDESCryptoServiceProvider oCrypto = new TripleDESCryptoServiceProvider();

                cryptoTransform = oCrypto.CreateEncryptor(Convert.FromBase64String(key), Convert.FromBase64String(iv));

                using (CryptoStream encStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    using (StreamWriter streamWriter = new StreamWriter(encStream, Encoding.ASCII))
                    {
                        // Write the originalValue to the stream.
                        streamWriter.WriteLine(data);
                        // Close the StreamWriter.
                        streamWriter.Close();
                    }
                    // Close the CryptoStream.
                    encStream.Close();
                }
                // Get an array of bytes that represents the memory stream.
                byResult = memoryStream.ToArray();
                // Close the memory stream.
                memoryStream.Close();
            }

            return Convert.ToBase64String(byResult);
        }

        public static String SpecialTripleDESDecrypt(String data, String key, String iv)
        {
            byte[] byResult = null;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                ICryptoTransform cryptoTransform = null;
                TripleDESCryptoServiceProvider oCrypto = new TripleDESCryptoServiceProvider();

                cryptoTransform = oCrypto.CreateDecryptor(Convert.FromBase64String(key), Convert.FromBase64String(iv));

                using (CryptoStream encStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    using (StreamWriter streamWriter = new StreamWriter(encStream, Encoding.ASCII))
                    {
                        // Write the originalValue to the stream.
                        streamWriter.WriteLine(data);
                        // Close the StreamWriter.
                        streamWriter.Close();
                    }
                    // Close the CryptoStream.
                    encStream.Close();
                }
                // Get an array of bytes that represents the memory stream.
                byResult = memoryStream.ToArray();
                // Close the memory stream.
                memoryStream.Close();
            }

            return Convert.ToBase64String(byResult);
        }

        public static String TripleDESEncrypt(String strString, String strKey, String strIV)
        {
            TripleDESCryptoServiceProvider crypto = new TripleDESCryptoServiceProvider();
            if (strKey.Length != 32) throw new ArgumentException("TripleDES 金鑰長度必須為 32 個位元。");
            crypto.Key = Convert.FromBase64String(strKey);
            if (strIV.Length != 12) throw new ArgumentException("TripleDES 向量長度必須為 12 個位元。");
            if (strIV.LastIndexOf("=") != 11) throw new ArgumentException("TripleDES 向量格式不正確。");
            crypto.IV = Convert.FromBase64String(strIV);

            byte[] byResult = null;
            // Encrypt a string to bytes.
            using (MemoryStream memoryStream = new MemoryStream())
            {
                ICryptoTransform cryptoTransform = crypto.CreateEncryptor();

                using (CryptoStream encStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    UTF8Encoding oUTF8 = new UTF8Encoding(false);

                    using (StreamWriter streamWriter = new StreamWriter(encStream, oUTF8))
                    {
                        // Write the originalValue to the stream.
                        streamWriter.Write(strString);
                        // Close the StreamWriter.
                        streamWriter.Close();
                    }
                    // Close the CryptoStream.
                    encStream.Close();
                }
                // Get an array of bytes that represents the memory stream.
                byResult = memoryStream.ToArray();
                // Close the memory stream.
                memoryStream.Close();
            }

            return Convert.ToBase64String(byResult);
        }

        public static String TripleDESDecrypt(String strString, String strKey, String strIV)
        {
            TripleDESCryptoServiceProvider crypto = new TripleDESCryptoServiceProvider();
            if (strKey.Length != 32) throw new ArgumentException("TripleDES 金鑰長度必須為 32 個位元。");
            crypto.Key = Convert.FromBase64String(strKey);
            if (strIV.Length != 12) throw new ArgumentException("TripleDES 向量長度必須為 12 個位元。");
            if (strIV.LastIndexOf("=") != 11) throw new ArgumentException("TripleDES 向量格式不正確。");
            crypto.IV = Convert.FromBase64String(strIV);

            String szResult = null;

            using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(strString)))
            {
                ICryptoTransform cryptoTransform = crypto.CreateDecryptor();

                // Create a CryptoStream using the memory stream and the CSP DES key. 
                using (CryptoStream encStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read))
                {
                    UTF8Encoding oUTF8 = new UTF8Encoding(false);

                    // Create a StreamReader for reading the stream.
                    using (StreamReader streamReader = new StreamReader(encStream, oUTF8))
                    {
                        // Read the stream as a string.
                        szResult = streamReader.ReadToEnd();
                        // Close the streams.
                        streamReader.Close();
                    }
                    // Close the CryptoStream.
                    encStream.Close();
                }
                // Close the memory stream.
                memoryStream.Close();
            }

            return szResult;
        }

        public static String StringToMD5(String strString)
        {
            MD5CryptoServiceProvider crypto = new MD5CryptoServiceProvider();

            byte[] byValue = Encoding.UTF8.GetBytes(strString);
            byte[] byHash = crypto.ComputeHash(byValue);

            crypto.Clear();
            return Convert.ToBase64String(byHash);
        }

        public static String StringToSHA1(String strString)
        {
            SHA1CryptoServiceProvider crypto = new SHA1CryptoServiceProvider();

            byte[] byValue = Encoding.UTF8.GetBytes(strString);
            byte[] byHash = crypto.ComputeHash(byValue);

            crypto.Clear();
            return Convert.ToBase64String(byHash);
        }

        public static String MAC(String strString)
        {
            return StringToSHA1(strString);
        }
    }
}