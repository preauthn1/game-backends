using game.Entity.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using game.Infrastructure.Extensions;
using game.Infrastructure;

namespace game.WebSite.Services
{
    public class ThirdPartyPaymentPostAction
    {
        /// <summary>
        /// 是否進行測試(單元測試等)
        /// </summary>
        private bool _isTesting;
        /// <summary>
        /// return URL
        /// </summary>
        private string _returnHtml;
        /// <summary>
        /// LOG檔案ID
        /// </summary>
        private string _logGuID;
        /// <summary>
        /// LOG檔案
        /// </summary>
        private string _logFile;
        /// <summary>
        /// LOG檔案訊息
        /// </summary>
        private string _logFileMessage;
        /// <summary>
        /// LOG DB 訊息
        /// </summary>
        private string _logDBMessage;
        /// <summary>
        /// OrderList資料表結構
        /// </summary>
        private CDPayOrder _order;
        /// <summary>
        /// Merchant資料表結構
        /// </summary>
        private CDMerchantUUID _merchant;

        /// <summary>
        /// 建構式
        /// </summary>
        /// <param name="logGuID">檔案唯一ID</param>
        /// <param name="logFile">檔案名稱</param>
        public ThirdPartyPaymentPostAction(string logGuID, string logFile)
        {
            _isTesting = false;
            _returnHtml = "目前支付人员太多, 请稍后再试";
            _logGuID = logGuID;
            _logFile = logFile;
            _logFileMessage = string.Empty;
            _logDBMessage = string.Empty;
            _order = null;
            _merchant = null;
        }

        /// <summary>
        /// 設定是否進行測試
        /// </summary>
        public void SetIsTesting(bool isTesting)
        {
            _isTesting = isTesting;
        }

        /// <summary>
        /// 設定Order訂單資料
        /// </summary>
        /// <param name="order">CDPayOrder</param>
        public bool SetOrderData(CDPayOrder order = null)
        {
            if (order == null)
            {
                _logFileMessage = "訂單(order)設定失敗";
                WriteLogFile(_logFileMessage);
                return false;
            }
            _order = order;
            return true;
        }

        /// <summary>
        /// 設定支付商 merchantinfo
        /// </summary>
        /// <param name="merchant">CDMerchantUUID</param>
        public bool SetMerChant(CDMerchantUUID merchant = null)
        {
            if (_order == null)
                return false;

            if (merchant == null)
            {
                _logFileMessage = $"{_order.Id} 支付请求失敗 [pay list] 0";
                WriteLogFile(_logFileMessage);
                _logDBMessage = "支付通道不足";
                WriteErrorLogDB(_logDBMessage);
                return false;
            }
            _merchant = merchant;
            _logFileMessage = $"訂單資訊 [ID]{_order.Id}、[Amount]{_order.Amount.ToString()}、[payType]{_merchant.BankCode}";
            WriteLogFile(_logFileMessage);
            return true;
        }

        /// <summary>
        /// 設定Return Url
        /// </summary>
        public void SetReturnHtml(string returnHtml)
        {
            if (string.IsNullOrEmpty(returnHtml))
                return;

            _returnHtml = returnHtml;

            _logFileMessage = $"返回HTML [returnHtml]{returnHtml}";
            WriteLogFile(_logFileMessage);
        }

        public bool LogPayType(PayType payType = null)
        {
            if (_order == null)
                return false;

            if (payType == null)
            {
                _logFileMessage = $"{_order.Id} 支付请求失敗 無法取得支付類型";
                WriteLogFile(_logFileMessage);
                _logDBMessage = "無法取得支付類型";
                WriteErrorLogDB(_logDBMessage);
                return false;
            }

            _logFileMessage = $"訂單資訊 [支付類型]{payType.BankCode}、[是否定額]{payType.AmountFixed.ToString()}";
            WriteLogFile(_logFileMessage);
            return true;
        }

        /// <summary>
        /// 取得Return Url
        /// </summary>
        public string GetReturnHtml()
        {
            return _returnHtml;
        }

        /// <summary>
        /// DB紀錄 訂單ID and 渠道ID and 交易訂單號
        /// </summary>
        public void LogThirdPartyOderID(string transNO)
        {
            if (string.IsNullOrEmpty(transNO))
                return;

            Service.CDNewModifyTransID(_order.Id, transNO, _merchant.GWID);
        }

        /// <summary>
        /// 取得自動發送form html
        /// </summary>
        /// <param name="dictionary">postdata</param>
        /// <param name="url">form action</param>
        /// <param name="method">form method(GET、POST)</param>
        public string GetFormAutoSubmitHtml(Dictionary<string, object> dictionary,string url, string method = "POST")
        {
            string returnHtml = string.Empty;

            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(method) || dictionary.IsEmptyOrNull())
                return returnHtml;

            returnHtml = "<form method=\"" + method + "\" name=\"form\" action=\"" + url + "\" enctype=\"application/x-www-form-urlencoded\">";
            foreach (var item in dictionary)
            {
                returnHtml += "<input type=\"hidden\" id=\"" + item.Key + "\" name=\"" + item.Key + "\" value=\"" + item.Value + "\">";
            }
            returnHtml += "</form>";
            returnHtml += "<script>window.onload = function(){document.forms[\"form\"].submit();}</script>";
            return returnHtml;
        }

        /// <summary>
        /// 開啟瀏覽頁送出表單
        /// </summary>
        /// <param name="formHtml">表單HTML</param>
        public string FormSubmitToRedirectHtmlOpenBrowser(string formHtml,string payCenterDoamin,string AESSecretKey)
        {
            string formString = "<html><body>" + formHtml + "</body></html>";
            string AESEncryptString = MyAESEncryption.ByteToHex(MyAESEncryption.AESEncrypt(formString, AESSecretKey)).Replace("-", "");
            string url = payCenterDoamin + "paycenter/open/?s=" + AESEncryptString;
            return GenerateRedirectHtmlOpenBrowser(url);
        }

        /// <summary>
        /// 開啟瀏覽器跳轉網頁
        /// </summary>
        /// <param name="targetUrl">跳轉網址</param>
        public string GenerateRedirectHtmlOpenBrowser(string targetUrl)
        {
            //目前有兩種狀況會開瀏覽器1.XX.href、2.XX.replace
            //<script>後方的 } 不可刪除，跳轉會失效
            return "<script>location.href='" + targetUrl + "';}</script>";
        }

        /// <summary>
        /// LOG紀錄md5使用的string
        /// </summary>
        public void LogPayOrderPostSignBeforeString(string md5String)
        {
            _logFileMessage = $"支付資訊 [md5使用的string]{md5String}";
            WriteLogFile(_logFileMessage);
        }

        /// <summary>
        /// LOG紀錄form data
        /// </summary>
        /// <param name="postDataString">post資料json string</param>
        /// <param name="url">form action</param>
        /// <param name="method">form method</param>
        public void LogPayOrderPostDataString(string postDataString, string url = null, string method = "POST")
        {
            _logFileMessage = $"支付資訊 [URL]{url} [METHOD]{method} [POSTDATA]{postDataString}";
            WriteLogFile(_logFileMessage);
        }

        /// <summary>
        /// LOG紀錄form return
        /// </summary>
        /// <param name="returnString">form return</param>
        public void LogPayReturnString(string returnString = null)
        {
            _logFileMessage = $"支付回傳資訊 [RETURN]{returnString}";
            WriteLogFile(_logFileMessage);
        }

        /// <summary>
        /// LOG紀錄回傳參數
        /// </summary>
        public void LogPayReturnMessage(bool isSuccess = false, string message = null)
        {
            if(isSuccess)
            {
                _logFileMessage = $"支付回傳请求成功 [message]{message}";
            }
            else
            {
                _logFileMessage = $"支付回傳请求失敗 [message]{message}";
                _logDBMessage = $"[message]{message}";
                WriteErrorLogDB(_logDBMessage);
            }
            WriteLogFile(_logFileMessage);
        }

        /// <summary>
        /// LOG紀錄回傳值簽名是否正確
        /// </summary>
        /// <param name="isSuccess">簽名是否正確</param>
        /// <param name="md5String">驗證簽名的md5前字串</param>
        public void LogPayReturnSign(bool isSuccess = false, string md5String = null)
        {
            if(isSuccess)
            {
                _logFileMessage = $"支付回傳簽名成功 [md5String]{md5String}";
            }
            else
            {
                _logFileMessage = $"支付回傳簽名失敗 [md5String]{md5String}";
            }
            WriteLogFile(_logFileMessage);
        }

        /// <summary>
        /// Log紀錄檔案
        /// </summary>
        /// <param name="message">Log紀錄內容</param>
        public void WriteLogFile(string message = null)
        {
            if (_isTesting)
                return;

            if (!string.IsNullOrEmpty(message))
            {
                SiteHelper.Log(_logGuID, message, _logFile);
            }
            else
            {
                SiteHelper.Log(_logGuID, "message設定錯誤", _logFile);
            }
        }

        /// <summary>
        /// Log紀錄資料庫
        /// </summary>
        /// <param name="message">Log紀錄內容</param>
        public void WriteErrorLogDB(string message = null)
        {
            if (_isTesting)
                return;

            if (_order == null)
                return;

            if (!string.IsNullOrEmpty(message))
            {
                Service.CDFailOrder(_order.Id, string.Empty, $" {message}");
            }
        }
    }
}