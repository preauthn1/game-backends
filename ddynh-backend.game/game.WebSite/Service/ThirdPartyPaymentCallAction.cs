using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using game.Entity.Payment;
using game.Infrastructure.Extensions;
using static game.WebSite.PayConfig;

namespace game.WebSite.Services
{
    public class ThirdPartyPaymentCallAction
    {
        /// <summary>
        /// 是否儲值成功
        /// </summary>
        private bool _isPayStateSuccess;
        /// <summary>
        /// 是否進行測試(單元測試等)
        /// </summary>
        private bool _isTesting;
        /// <summary>
        /// LOG檔案ID
        /// </summary>
        private string _logGuID;
        /// <summary>
        /// LOG檔案
        /// </summary>
        private string _logFile;
        /// <summary>
        /// 資料庫訂單ID
        /// </summary>
        private int _orderID;
        /// <summary>
        /// 第三方訂單ID(ThirdPartyOderID)
        /// </summary>
        private string _payOrderID;
        /// <summary>
        /// 訂單資料
        /// </summary>
        private CDPayOrder _order;
        /// <summary>
        /// 來源類型
        /// </summary>
        private PayState _payState;
        /// <summary>
        /// LOG DB 訊息
        /// </summary>
        private string _logDBMessage;
        /// <summary>
        /// LOG檔案訊息
        /// </summary>
        private string _logFileMessage;
        /// <summary>
        /// 異步回調字串
        /// </summary>
        private string _responseString;
        /// <summary>
        /// 是否儲值成功
        /// </summary>
        public bool IsPayStateSuccess
        {
            get
            {
                return _isPayStateSuccess;
            }
        }
        public PayState PayState
        {
            get { return _payState; }
        }

        /// <summary>
        /// 建構式
        /// </summary>
        /// <param name="logGuID">檔案唯一ID</param>
        /// <param name="logFile">檔案名稱</param>
        public ThirdPartyPaymentCallAction(string logGuID, string logFile)
        {
            _isPayStateSuccess = false;
            _isTesting = false;
            _logGuID = logGuID;
            _logFile = logFile;
            _logFileMessage = string.Empty;
            _logDBMessage = string.Empty;
            _orderID = 0;
            _payOrderID = "0";
            _order = null;
            _payState = PayState.Success;
        }

        /// <summary>
        /// 設定order資料
        /// </summary>
        public void SetOrderData(int orderID, string payOrderID)
        {
            if (string.IsNullOrEmpty(payOrderID) || orderID == 0)
                return;

            _orderID = orderID;
            _payOrderID = payOrderID;
        }

        public void LogResponseString(string responseString = null)
        {
            if (string.IsNullOrEmpty(responseString))
            {
                //取得回調string，並記錄LOG
                var obj = new ResponseStringTransform();
                _responseString = obj.GetResponseString();
            }
            else
            {
                _responseString = responseString;
            }

            _logFileMessage = string.Format("支付回调 [FormData] {0}", _responseString);
            WriteLogFile(_logFileMessage);
        }

        /// <summary>
        /// 取得回調string
        /// </summary>
        public string GetResponseString()
        {
            return _responseString;
        }

        /// <summary>
        /// LOG取得回調
        /// </summary>
        private void LogResponse(bool isResponseSuccess = false)
        {
            if (isResponseSuccess)
                return;
            _logDBMessage = $"responseCode:{isResponseSuccess.ToString()}";
            _logFileMessage = $"{_orderID} 支付回调 订单失敗";
            WriteLogFile(_logFileMessage);
            WriteErrorLogDB(_logDBMessage);
        }
        /// <summary>
        /// 是否取得回調成功
        /// </summary>
        /// <returns>是否結束callback</returns>
        public bool CheckResponseIsSuccess(bool isResponseSuccess = false)
        {
            //回調錯誤
            if (!isResponseSuccess)
            {
                //LOG
                LogResponse(isResponseSuccess);
                _isPayStateSuccess = false;
                _payState = PayState.Error;
                return true;
            }
            return false;
        }

        /// <summary>
        /// LOG交易是否成功
        /// </summary>
        private void LogTransaction(bool isTransactionSuccess = false)
        {
            if (isTransactionSuccess)
                return;
            _logDBMessage = $"responseCode:{isTransactionSuccess.ToString()}";
            _logFileMessage = $"{_orderID} 交易失敗 订单失敗";
            WriteLogFile(_logFileMessage);
            WriteErrorLogDB(_logDBMessage);
        }
        /// <summary>
        /// 確認交易是否成功
        /// </summary>
        /// <returns>是否結束callback</returns>
        public bool CheckTransactionIsSuccess(bool isTransactionSuccess = false)
        {
            //交易失敗
            if (!isTransactionSuccess)
            {
                //LOG
                LogTransaction(isTransactionSuccess);
                _isPayStateSuccess = false;
                _payState = PayState.Error;
                return true;
            }
            return false;
        }

        /// <summary>
        /// LOG交易是否為儲值
        /// </summary>
        private void LogTransactionStatus(bool isPayment = false)
        {
            if (isPayment)
                return;
            _logFileMessage = $"{_orderID} 交易失敗 订单失敗 订单类型非储值";
            WriteLogFile(_logFileMessage);
        }
        /// <summary>
        /// 確認交易是否為儲值
        /// </summary>
        /// <param name="isPayment">是否為儲值</param>
        public bool CheckTransactionIsPayment(bool isPayment = false)
        {
            //非為存款
            if (!isPayment)
            {
                //LOG
                LogTransactionStatus(isPayment);
                _isPayStateSuccess = false;
                _payState = PayState.Error;
                return true;
            }
            return false;
        }

        /// <summary>
        /// LOG資料庫order查無訂單
        /// </summary>
        private void LogNoOrder()
        {
            _logFileMessage = $"{_orderID} 支付回调 查無訂單";
            WriteLogFile(_logFileMessage);
        }

        /// <summary>
        /// 檢查訂單資料
        /// </summary>
        /// <returns>是否結束callback</returns>
        public bool CheckOrderData()
        {
            //尚未設定orderID
            if (_orderID == 0)
            {
                _isPayStateSuccess = false;
                _payState = PayState.Error;
                return true;
            }
            var orderlist = Service.CDGetAEPayOrderByID(_orderID);
            //資料庫order查無訂單
            if (orderlist.IsEmptyOrNull())
            {
                _isPayStateSuccess = false;
                _payState = PayState.Error;
                //LOG
                LogNoOrder();
                return true;
            }
            //取得訂單
            _order = orderlist.FirstOrDefault();
            return false;
        }

        /// <summary>
        /// 取得訂單資料
        /// </summary>
        public CDPayOrder GetOrder()
        {
            return _order;
        }

        /// <summary>
        /// LOG response sign
        /// </summary>
        private void LogResponseSign(bool isSignSuccess, string signMd5String)
        {
            if (isSignSuccess)
                return;
            _logDBMessage = $"responseSign:{isSignSuccess.ToString()}";
            _logFileMessage = $"{_orderID} 支付回调 signError 订单失敗 [SignMd5String]{signMd5String}";
            WriteLogFile(_logFileMessage);
            WriteErrorLogDB(_logDBMessage);
        }

        /// <summary>
        /// 檢查response sign
        /// </summary>
        /// <returns>是否結束callback</returns>
        public bool CheckResponseSign(bool isSignSuccess, string signMd5String)
        {
            LogResponseSign(isSignSuccess, signMd5String);
            _isPayStateSuccess = isSignSuccess;
            if(!isSignSuccess)
            {
                _payState = PayState.SignError;
            }

            //如果sign錯誤則不可繼續執行
            return !isSignSuccess;
        }

        /// <summary>
        /// LOG訂單狀態已經完成,無須金幣充值
        /// </summary>
        private void LogFinishOrderNOStored()
        {
            _logFileMessage = $"{_orderID} 支付回调 訂單狀態已經完成,無須金幣充值";
            WriteLogFile(_logFileMessage);
        }
        /// <summary>
        /// LOG回調訂單金額與資料庫訂單金額不符
        /// </summary>
        private void LogResposeAmountError(string responseAmount)
        {
            _logFileMessage = string.Format($"{_orderID}  支付回调 支付無效 merchantOrderId:{_payOrderID},merchantOrderAmount{_order.Amount},totalAmount:{responseAmount}");
            WriteLogFile(_logFileMessage);
        }
        /// <summary>
        /// LOG資料庫儲存
        /// </summary>
        private void LogDBSave(bool isDBSaveSuccess, string responseAmount, string message)
        {
            if (isDBSaveSuccess)
            {
                _logFileMessage = $"{_orderID} 支付回调 金幣充值成功 merchantOrderId,:{_payOrderID}, totalAmount:{responseAmount}";
            }
            else
            {
                _logFileMessage = $"{_orderID} 支付回调 金幣充值错误 {message}";
                _logDBMessage = message;
                WriteErrorLogDB(_logDBMessage);
            }
            WriteLogFile(_logFileMessage);
        }
        /// <summary>
        /// 確認金幣儲值
        /// </summary>
        /// <returns>是否結束callback</returns>
        public bool FinishOrderAndCheckMonery(int responseAmount, string userIP)
        {
            //尚未設定orderID
            if (_orderID == 0)
            {
                _isPayStateSuccess = false;
                _payState = PayState.Error;
                return true;
            }
            //尚未設定order data
            if (_order == null)
            {
                _isPayStateSuccess = false;
                _payState = PayState.Error;
                return true;
            }
            //訂單狀態已經完成,無須金幣充值
            if (_order.Status == CDOrderStatus.Paied)
            {
                _isPayStateSuccess = true;
                //LOG
                LogFinishOrderNOStored();
                return true;
            }
            //回調訂單金額與資料庫訂單金額不符
            if (responseAmount != _order.Amount)
            {
                _isPayStateSuccess = false;
                _payState = PayState.Error;
                //LOG
                LogResposeAmountError(responseAmount.ToString());
                return true;
            }

            //確認寫入資料庫是否成功
            string message = null;
            bool isDBSaveSuccess = Service.CDFinishAEOrder(_orderID, responseAmount, userIP, _payOrderID, "完成支付", ref message);
            _isPayStateSuccess = isDBSaveSuccess;
            if(!isDBSaveSuccess)
            {
                _payState = PayState.Error;
            }

            //LOG
            LogDBSave(isDBSaveSuccess, responseAmount.ToString(), message);

            //如果資料庫存入失敗，則結束callback = true
            return !isDBSaveSuccess;
        }


        public bool LogThirdPartyPayOrderStatus(string status)
        {
            _logFileMessage = $"{_orderID} 支付回调 [第三方訂單狀態]{status}";
            WriteLogFile(_logFileMessage);
            return false;
        }

        /// <summary>
        /// 設定是否進行測試
        /// </summary>
        public void SetIsTesting(bool isTesting)
        {
            _isTesting = isTesting;
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

            if (_orderID == 0)
                return;

            if (string.IsNullOrEmpty(_payOrderID))
                return;

            if (!string.IsNullOrEmpty(message))
            {
                Service.CDFailOrder(_orderID, _payOrderID, $" {message}");
            }
        }
    }
}