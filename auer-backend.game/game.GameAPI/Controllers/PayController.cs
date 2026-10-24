using System;
using System.Collections.Generic;
using System.Web.Http;
using game.Service;
using game.Infrastructure;
using game.Entity.Pay;
using FW.Core.Helper;
using Newtonsoft.Json;

namespace game.GameAPI.Controllers
{
    public class PayController : BaseAPIController
    {

        private IPayService _payService;
        public PayController(IPayService payService)
        {
            _payService = payService;
        }

        #region 主后台支付相关功能
        [HttpPost]
        public APIResult CDCreateOrder(CDPayOrder order)
        {
            string message = "";
            bool result = _payService.CDCreateOrder(order, ref message);
            return new APIResult(result, message, order);
        }

        [HttpGet]
        public APIResult CreateGashPaySettleStatus(string msgType, string pcode, string cid, string coid,string cuid, string paid, string amount, string erqc, bool isSuccess)
        {
            string message = "";
            bool result = _payService.CreateGashPaySettleStatus(msgType, pcode, cid, coid, cuid, paid, amount, erqc, isSuccess);
            return new APIResult(result, message);
        }

        [HttpGet]
        public CDPayOrder CDGetPayOrderByID(int orderID)
        {
            return _payService.CDGetPayOrderByID(orderID);
        }
        [HttpGet]
        public APIResult CDFinishOrder(int orderID, Int64 amount, string ipAddress, string transID, string transData)
        {
            string message = "";
            bool result = _payService.CDFinishOrder(orderID, amount, ipAddress, transID, transData, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult FinishedOrderShanfu(int playerId, string orderId, string teamOrderId, string chaseOrderId, int payType, int amount, int presentAmount, string status, int time)
        {
            var message = string.Empty;
            var localOrderID = string.Empty;
            var result = _payService.FinishedOrderShanfu(playerId, orderId, teamOrderId, chaseOrderId, payType, amount, presentAmount, status, time, ref localOrderID, ref message);
            return new APIResult(result, message, localOrderID);
        }

        [HttpGet]
        public APIResult WithdrawCallbackByCK(int orderId, int status, string transID, string transData)
        {
            var message = string.Empty;
            var result = _payService.WithdrawCallbackByCK(orderId, status, transID, transData, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult WithdrawCallbackByHema(string orderId, string status, string transID, string transData)
        {
            var message = string.Empty;
            var result = _payService.WithdrawCallbackByHema(orderId, status, transID, transData, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult WithdrawCallback(string orderId, int status, string transID, string transData)
        {
            var message = string.Empty;
            var result = _payService.WithdrawCallback(orderId, status, transID, transData, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult CDFinishThirdPartyOrder(int orderType, int orderID, int playerID, string transID, Int64 amount, string ipAddress, string transData, int status)
        {
            string message = "";
            bool result = _payService.CDFinishThirdPartyOrder(orderType, orderID, playerID, transID, amount, ipAddress, transData, status, ref message);
            return new APIResult(result, message);
        }


        [HttpGet]
        public APIResult CDFailOrder(int orderID, string transID, string transData)
        {
            string message = "";
            bool result = _payService.CDFailOrder(orderID, transID, transData);
            return new APIResult(result, message);
        }

        [HttpGet]
        public PagedList<OrderListDTO> CDGetPayOrder(int? platformId, int? orderID = null, int? adminId = null, int? userID = null, string ipAddress = null, int? GWID = null, int? payTypeID = null, CDOrderStatus? status = null, int timeType = 0, DateTime? createBeginTime = null, DateTime? createEndTime = null, int? areaID = null, int pageIndex = 1, int pageSize = 20)
        {
            return _payService.CDGetPayOrder(platformId, orderID, adminId, userID, ipAddress, GWID, payTypeID, status, timeType, createBeginTime, createEndTime, areaID, pageIndex, pageSize);
        }


        [HttpGet]
        public APIResult CDModifyTransID(int orderID, string transID)
        {
            string message = "";
            bool result = _payService.CDModifyTransID(orderID, transID);
            return new APIResult(result, message);
        }

        [HttpPost]
        public APIResult CDUpdateGWID(CDPayOrder order)
        {
            string message = "";
            bool result = _payService.CDUpdateGWID(order);
            return new APIResult(result, message, order);
        }

        [HttpGet]
        public List<ConvertGateway> GetConvertGateway(int operatorID)
        {
            return _payService.GetConvertGateway(operatorID);
        }

        [HttpGet]
        public List<PayGateway> CDGetPayType(int operatorID)
        {
            return _payService.CDGetPayType(operatorID);
        }

        [HttpGet]
        public List<PayGateway> GetCompanyPayType(int operatorID)
        {
            return _payService.GetCompanyPayType(operatorID);
        }

        [HttpGet]
        public List<PayGateway> CDGetUnionPlatformPayType(int operatorID, int platformID)
        {
            return _payService.CDGetUnionPlatformPayType(operatorID, platformID);
        }

        [HttpGet]
        public APIResult SetUnionPlatformPayType(int operatorID, int platformID, string param)
        {
            string message = "";
            var result = _payService.SetUnionPlatformPayType(operatorID, platformID, param, ref message);

            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult CDSetPayType(int GWID, int typeID, string amountSort, string tradeCode, bool isRange, bool status)
        {
            string message = "";
            bool result = _payService.SetPayType(GWID, typeID, amountSort, tradeCode, isRange, status, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult SetPayGateway(PayGateway gateway)
        {
            string message = "";
            bool result = false;

            result = _payService.SetPayGateway(gateway.GWID, gateway.Priority, ref message);

            return new APIResult(result, message);
        }


        public List<PayGateway> GetPayGateway(int payTypeID)
        {
            return _payService.CDGetCDPayType(payTypeID);
        }

        /// <summary>
        /// 根據渠道取得可用的支付通道(SP內根據areaid決定撈取的支付通道)
        /// </summary>
        /// <param name="payTypeID"></param>
        /// <param name="areaID"></param>
        /// <returns></returns>
        public List<PayGateway> GetPayGatewayByAreaID(int payTypeID, int areaID)
        {
            return _payService.CDGetCDPayTypeByAreaID(payTypeID, areaID);
        }

        public List<PayGateway> GetAmountList(int? platformID)
        {
            return _payService.GetAmountList(platformID);
        }

        public List<PayGateway> GetPayAmountList(int? platformID)
        {
            return _payService.GetPayAmountList(platformID);
        }

        public List<PayPointConvert> GetPayPointConverts()
        {
            return _payService.GetPayPointConverts();
        }

        public PagedList<PlatformLog> GetPlatformLog(int operatorID, int? platformId = null, int? areaID = null, DateTime? statTime = null, DateTime? endTime = null, int? payTypeID = null, int pageIndex = 0, int pageSize = 0)
        {
            return _payService.GetPlatformLog(operatorID, platformId, areaID, statTime, endTime, payTypeID, pageIndex, pageSize);
        }

        [HttpGet]
        public PagedList<OrderLog> GetOrderLogByCS(int adminID, string CSName, int? userID, int? orderID, DateTime? startTime, DateTime? endTime
            , int? payTypeID, int? GWID, int pageIndex = 0, int pageSize = 0)
        {
            return _payService.GetOrderLogByCS(adminID, CSName, userID, orderID, startTime, endTime, payTypeID, GWID, pageIndex, pageSize);
        }

        [HttpGet]
        public List<OrderLog> ExportOrderLogByCS(int adminID, string CSName, int? userID, int? orderID, DateTime? startTime, DateTime? endTime
          , int? payTypeID, int? GWID)
        {
            return _payService.ExportOrderLogByCS(adminID, CSName, userID, orderID, startTime, endTime, payTypeID, GWID);
        }

        [HttpGet]
        public APIResult FinishedOrderByCS(int adminID, string CSOrderID, string CSAmount, string CSMemo, string IPAddress)
        {
            string message = "";
            bool result = _payService.FinishedOrderByCS(adminID, CSOrderID, CSAmount, CSMemo, IPAddress, ref message);
            return new APIResult(result, message);
        }

        [HttpPost]
        public APIResult CDCreateOrder11(CDPayOrder order)
        {
            string message = "";
            bool result = _payService.CDCreateOrder(order, ref message);
            return new APIResult(result, message, order);
        }

        [HttpPost]
        public APIResult CreateRedirectData(RedirectData order)
        {
            string message = "";
            bool result = _payService.CreateRedirectData(order, ref message);
            return new APIResult(result, message, order);
        }

        [HttpGet]
        public RedirectData GetRedirectData(int orderId, string gid)
        {
            return _payService.GetRedirectData(orderId, gid);
        }

        #endregion
    }
}