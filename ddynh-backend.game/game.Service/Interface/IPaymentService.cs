using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.DTO.ThirdPartyPay;
using game.Entity.Payment;
using game.Entity.ViewModels;

namespace game.Service
{
    public interface IPaymentService
    {
        #region 主后台相关支付

        /// <summary>
        /// 获取渠道
        /// </summary>
        /// <returns></returns>
        List<PayGateway> CDGetPayGateway(int PayTypeID);

        List<PayGateway> CDGetPayGatewayAll();

        /// <summary>
        /// 获取渠道
        /// </summary>
        /// <returns></returns>
        List<PayType> CDGetPayType(int operatorID, int? targetID);

        PayType GetPayTypeByID(int payTypeID);


        /// <summary>
        /// 根据GWID typeID更新渠道
        /// </summary>
        /// <returns></returns>
        bool SetPayType(int GWID, int typeID, string amountSort, string tradeCode, bool isRange, bool status, ref string message);

        /// <summary>
        /// 根据GWID 更新第三方渠道权重
        /// </summary>
        /// <returns></returns>
        bool SetPayGateway(int GWID, int priority, ref string message);

        bool UpdatePayGateway(string code, string name, string cur, int info, bool supPayin, bool supPayout, bool isRange, string bankCodes);

        bool UpdatePayType(string code, int type, string cur, string bankCode, int minAmount, int maxAmount, bool amountFixed = false, string fixedAmount = "");

        bool UpdateMerchantInfo(int gwid, string infodata, int merchantId, int platformId);

        bool UpdateMerchantStatus(int id, int status);

        List<CDMerchantList> CDGetMerchantList(int gwID, int id, int? platformId, int adminID);

        List<CDMerchantUUID> GetMerchantUuidList(int payTypeID, int paltfromID);

        List<CDPayOrder> CDGetPayOrderByID(int orderID);

        List<PayTypeManagementAmountListModel> GetPayTypeManagementAmountList(int? platformID);


        List<game.Entity.Pay.PayGateway> GetAmountList(int? platformID, int type = 0);
        #endregion
        PayGatewayEntity GetGatewayByID(int gwID);

        int GetPlatformId(int UserID);

        bool UpdatePayTypeDisable(int type, int id, int payTypeId, int platformId);

        bool UpdatePlatformPayType(int id, int payTypeId, int platformId, int? minAmount, int? maxAmount, int isRange, string amountList, int scale, int? status);

        bool CheckPaymentOrder(int orderID, string transID);

        bool InsertThirdPartyPay(InsertThirdPartyPayModel input);

        /// <summary>
        /// 获取代付廠商
        /// </summary>
        /// <returns></returns>
        List<ConvertGateway> GetConvertGateway(int operatorID, int? convertType, int? userId, int? bankCodeID = null);
    }
}
