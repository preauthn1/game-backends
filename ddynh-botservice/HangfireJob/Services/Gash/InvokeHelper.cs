using HangfireJob.EFModels.Gash;
using HangfireJob.Models.Gash;
using System;
using example.invalid;
using example.invalid;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace HangfireJob.Services.Gash
{
    public class InvokeHelper
    {
        public InvokeHelper() { }

        public static bool CallDoAPI(GashSettleLog gashSettleLog, string url)
        {
            var strXmlDoc = "<TRANS>";
            strXmlDoc += "<MSG_TYPE>" + gashSettleLog.MsgType + "</MSG_TYPE>";
            strXmlDoc += "<PCODE>" + gashSettleLog.PCode + "</PCODE>";
            strXmlDoc += "<CID>" + gashSettleLog.CId + "</CID>";
            strXmlDoc += "<COID>" + gashSettleLog.COId + "</COID>";
            strXmlDoc += "<CUID>" + gashSettleLog.CUId + "</CUID>";
            strXmlDoc += "<PAID>" + gashSettleLog.PAId + "</PAID>";
            strXmlDoc += "<AMOUNT>" + gashSettleLog.Amount + "</AMOUNT>";
            strXmlDoc += "<ERQC>" + gashSettleLog.ERQC + "</ERQC>"; // ******請款請求是帶ERQC不是ERPC
            strXmlDoc += "</TRANS>";

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(strXmlDoc);

            var res = InvokeService(GetSendData(xmlDoc), url);
            var c = Encoding.GetEncoding("utf-8").GetString(Convert.FromBase64String(res.InnerText));
            XmlSerializer settleReponse = new XmlSerializer(typeof(EnvelopeReponse));
            EnvelopeReponse envelope = new EnvelopeReponse();
            using (StringReader reader = new StringReader(c))
            {
                envelope = (EnvelopeReponse)(settleReponse.Deserialize(reader));
                /*
                *  若接收到GPS 系統回傳請款交易的RCODE=9004、RCODE=9997、RCODE =9998、 RCODE=9999 或未收到GPS 系統回覆請款結果時，請再次發動請款交易直到請款成功為止。
                *  建議每5 分鐘針對上述情況的交易成功單再次發動請款，以避免有漏單情況發生。 
               */
                if (!(envelope.RCODE == "0000" && envelope.PAY_STATUS == "S"))
                {
                    return false;
                }
                return true;
            }
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

        public static XmlDocument InvokeService(string data, string url)
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
                    var soapXml = rd.ReadToEnd();
                    XmlDocument soapEnvelop = new XmlDocument();
                    soapEnvelop.LoadXml(soapXml);
                    return soapEnvelop;
                }
            }
        }

        private static HttpWebRequest CreateSOAPWebRequest(string url)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(url);
            Req.Headers.Add(@"SOAPAction:http://example.invalid/getResponse");
            Req.ContentType = "text/xml;charset=\"utf-8\"";
            Req.Accept = "text/xml";
            Req.Method = "POST";
            return Req;
        }
    }
}
