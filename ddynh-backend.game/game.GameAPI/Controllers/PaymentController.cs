using System;
using System.Collections.Generic;
using System.Web.Http;
using game.Service;
using game.Infrastructure;
using game.Entity.Payment;
using FW.Core.Helper;
using Newtonsoft.Json;
using game.Entity.ViewModels;
using game.Entity.DTO.ThirdPartyPay;
using System.Linq;

namespace game.GameAPI.Controllers
{
    public class PaymentController : BaseAPIController
    {
        private IPaymentService _paymentService;
        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        #region 主后台支付相关功能

        [HttpGet]
        public List<PayType> CDGetPayType(int operatorID, int? targetID)
        {
            return _paymentService.CDGetPayType(operatorID, targetID);
        }

        [HttpGet]
        public PayType GetPayTypeByID(int payTypeID)
        {
            return _paymentService.GetPayTypeByID(payTypeID);
        }


        [HttpGet]
        public PayGatewayEntity GetGatewayByID(int gwID)
        {
            var query = _paymentService.GetGatewayByID(gwID);
            return query;
        }


        [HttpGet]
        public List<PayGateway> CDGetPayGateway(int PayTypeID = 0)
        {
            if (PayTypeID > 0)
            {
                return _paymentService.CDGetPayGateway(PayTypeID);
            }
            else
            {
                return _paymentService.CDGetPayGatewayAll();
            }
        }

        #endregion

        [HttpGet]
        public APIResult ResetPayGateway(string url, string auth)
        {

            //判斷
            //call XXX 至3rd取回資料結構

            //call server寫入DB
            //bool result = _payService.CDFailOrder(orderID, transID, transData);  //example

            string message = "url:" + url + " auth:" + auth;
            bool result = true;
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult AddPayGateway(string code, string name, string cur, int info, bool supPayin, bool supPayout, bool isRange, string bankCodes)
        {
            //call server寫入DB
            bool Result = _paymentService.UpdatePayGateway(code, name, cur, info, supPayin, supPayout, isRange, bankCodes);  //example
            string Message = (Result == true) ? "新增成功" : "新增失敗";
            return new APIResult(Result, Message);
        }

        [HttpGet]
        public APIResult AddPayType(string code, int type, string cur, string bankCode, int minAmount, int maxAmount, bool amountFixed = false, string fixedAmount = "")
        {
            //call server寫入DB
            bool Result = _paymentService.UpdatePayType(code, type, cur, bankCode, minAmount, maxAmount, amountFixed, fixedAmount);
            string Message = (Result == true) ? "新增成功" : "新增失敗";
            return new APIResult(Result, Message);
        }

        [HttpGet]
        //public APIResult AddMerchantInfo(string gwcode, string infodata, int merchantId, string uuid, int platformid)
        public APIResult AddMerchantInfo(int gwid, string infodata, int merchantId, int platformid)
        {
            //call server寫入DB
            var message = string.Empty;
            bool Result = _paymentService.UpdateMerchantInfo(gwid, infodata, merchantId, platformid);
            return new APIResult(Result, message);
        }

        [HttpGet]
        public APIResult UpdateInfoStatus(int id, int status)
        {
            bool Result = _paymentService.UpdateMerchantStatus(id, status);
            string Message = (Result == true) ? "新增成功" : "新增失敗";
            return new APIResult(Result, Message);
        }

        [HttpGet]
        public List<CDMerchantList> CDGetMerchantList(int gwID, int id = 0, int? platformId = null, int adminID = 0)
        {
            return _paymentService.CDGetMerchantList(gwID, id, platformId, adminID);
        }

        [HttpGet]
        public List<CDMerchantUUID> GetMerchantUuid(int payTypeID, int platformID)
        {
            return _paymentService.GetMerchantUuidList(payTypeID, platformID);
        }

        [HttpGet]
        public List<CDPayOrder> CDGetPayOrderByID(int orderID)
        {
            return _paymentService.CDGetPayOrderByID(orderID);
        }

        [HttpGet]
        public List<game.Entity.Pay.PayGateway> GetAmountList(int? platformID, int type = 0)
        {
            return _paymentService.GetAmountList(platformID, type);
        }

        [HttpGet]
        public List<PayTypeManagementAmountListModel> GetPayTypeManagementAmountList(int? platformID)
        {
            var query = _paymentService.GetPayTypeManagementAmountList(platformID);
            return query;
        }

        [HttpGet]
        public int GetPlatformId(int UserID)
        {
            return _paymentService.GetPlatformId(UserID);
        }

        [HttpGet]
        public APIResult UpdatePayTypeDisable(int type, int id = 0, int payTypeId = 0, int platformId = 0)
        {
            bool Result = _paymentService.UpdatePayTypeDisable(type, id, payTypeId, platformId);
            string Message = (Result == true) ? "新增成功" : "新增失敗";
            return new APIResult(Result, Message);
        }

        [HttpGet]
        public APIResult UpdatePlatformPayType(int id, int payTypeId, int platformId, int? minAmount, int? maxAmount, int isRange, string amountList, int scale = 0, int status = 0)
        {
            bool Result = _paymentService.UpdatePlatformPayType(id, payTypeId, platformId, minAmount, maxAmount, isRange, amountList, scale, status);
            string Message = (Result == true) ? "變更成功" : "變更失敗";
            return new APIResult(Result, Message);
        }

        [HttpGet]
        public APIResult CheckPaymentOrder(int orderID, string transID = null)
        {
            bool Result = _paymentService.CheckPaymentOrder(orderID, transID);
            string Message = (Result) ? "交易标识不存在" : "交易标识已存在";
            return new APIResult(Result, Message);
        }

        [HttpPost]
        public APIResult InsertThirdPartyPay(InsertThirdPartyPayFormModel model)
        {
            var intput = new InsertThirdPartyPayModel
            {
                chvPayGatewayCode = model.paygateway_code,
                chvPayGatewayName = model.paygateway_name,
                chvPayGatewayUrl = model.paygateway_url,
                chvPayGatewayBankCodes = string.Join(",", model.paytype_BankCode.Select(row => string.Format("CNY_{0}", row))),
                chvPayTypeBankCodes = string.Join(",", model.paytype_BankCode)
            };

            bool Result = _paymentService.InsertThirdPartyPay(intput);
            string Message = (Result == true) ? "新增成功" : "新增失敗";
            return new APIResult(Result, Message);
        }

        [HttpPost]
        public ThirdPartyPayMerchantInfo GetThirdPartyPayMerchanInfoData(ThirdPartyPayMerchantInfo model)
        {
            return model;
        }

        [HttpGet]
        public List<ConvertGateway> GetConvertGateway(int operatorID, int? convertType, int? userId, int? bankCodeID = null)
        {
            return _paymentService.GetConvertGateway(operatorID, convertType, userId, bankCodeID);
        }
    }
}