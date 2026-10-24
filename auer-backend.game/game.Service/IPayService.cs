using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.Pay;

namespace game.Service
{
    /// <summary>
    /// 支付接口
    /// </summary>
    public interface IPayService
    {

        #region 主后台相关支付

        /// <summary>
        /// 创建支付订单
        /// </summary>
        /// <param name="order"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool CDCreateOrder(CDPayOrder order, ref string message);       

        /// <summary>
        /// 根据ID获取支付订单
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        CDPayOrder CDGetPayOrderByID(int orderID);

        /// <summary>
        /// 修改第三方交易标识
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="transID"></param>
        /// <returns></returns>
        bool CDModifyTransID(int orderID, string transID);

        /// <summary>
        /// settle order
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="pcode"></param>
        /// <param name="cid"></param>
        /// <param name="coid"></param>
        /// <param name="cuid"></param>
        /// <param name="paid"></param>
        /// <param name="amount"></param>
        /// <param name="erqc"></param>
        /// <returns></returns>
        bool CreateGashPaySettleStatus(string msgType, string pcode, string cid, string coid, string cuid, string paid, string amount, string erqc, bool isSuccess);

        /// <summary>
        /// 修改第三方交易渠道
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        bool CDUpdateGWID(CDPayOrder order);

        /// <summary>
        /// 根据ID获取第三方交易设定
        /// </summary>
        /// <param name="payTypeID"></param>
        /// <returns></returns>
        List<PayGateway> CDGetCDPayType(int payTypeID);

        /// <summary>
        /// 根據渠道取得可用的支付通道(SP內根據areaid決定撈取的支付通道)
        /// </summary>
        /// <param name="payTypeID"></param>
        /// <param name="areaID"></param>
        /// <returns></returns>
        List<PayGateway> CDGetCDPayTypeByAreaID(int payTypeID, int areaID);

        /// <summary>
        /// 获取支付金额接口
        /// </summary>
        /// <returns></returns>
        List<PayGateway> GetAmountList(int? platformID);

        /// <summary>
        /// 获取支付金额接口
        /// </summary>
        /// <returns></returns>
        List<PayGateway> GetPayAmountList(int? platformID);

        List<PayPointConvert> GetPayPointConverts();

        /// <summary>
        /// 获取代付廠商
        /// </summary>
        /// <returns></returns>
        List<ConvertGateway> GetConvertGateway(int operatorID);

        /// <summary>
        /// 获取渠道
        /// </summary>
        /// <returns></returns>
        List<PayGateway> CDGetPayType(int operatorID);

        /// <summary>
        /// 获取營運商支付方式
        /// </summary>
        /// <param name="operatorID"></param>
        /// <returns></returns>
        List<PayGateway> GetCompanyPayType(int operatorID);

        /// <summary>
        /// 取得平台支付方式
        /// </summary>
        /// <param name="operatorID"></param>
        /// <param name="platformID"></param>
        /// <returns></returns>
        List<PayGateway> CDGetUnionPlatformPayType(int operatorID, int platformID);

        bool SetUnionPlatformPayType(int operatorID, int platformID, string param, ref string message);

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


        /// <summary>
        /// 完成支付订单
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="amount"></param>
        /// <param name="ipAddress"></param>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool CDFinishOrder(int orderID, Int64 amount, string ipAddress, string transID, string data, ref string message);

        bool CDFinishThirdPartyOrder(int orderType, int orderID, int playerID, string transID, Int64 amount, string ipAddress, string data, int status, ref string message);

        /// <summary>
        /// 完成支付订单
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="transID"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        bool CDFailOrder(int orderID, string transID, string data);

        /// <summary>
        /// 获取支付订单
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="adminId"></param>
        /// <param name="ipAddress"></param>
        /// <param name="payTypeID"></param>
        /// <param name="status"></param>
        /// <param name="createBeginTime"></param>
        /// <param name="createEndTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<OrderListDTO> CDGetPayOrder(int? platformId, int? orderID, int? adminId, int? userID, string ipAddress, int? GWID, int? payTypeID, CDOrderStatus? status, int timeType, DateTime? createBeginTime, DateTime? createEndTime, int? areaID, int pageIndex, int pageSize);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payTypeID"></param>
        /// <returns></returns>
        PagedList<PlatformLog> GetPlatformLog(int operatorID, int? platformId = null, int? areaID = null, DateTime? statTime = null, DateTime? endTime = null, int? payTypeID = null, int pageIndex = 0, int pageSize = 0);

        PagedList<OrderLog> GetOrderLogByCS(int adminID, string CSName, int? userID, int? orderID, DateTime? startTime, DateTime? endTime
            , int? payTypeID, int? GWID, int pageIndex = 0, int pageSize = 0);

        List<OrderLog> ExportOrderLogByCS(int adminID, string CSName, int? userID, int? orderID, DateTime? startTime, DateTime? endTime
           , int? payTypeID, int? GWID);


        /// <summary>
        /// 閃付支付
        /// </summary>
        /// <param name="playerId">充值玩家游戏ID</param>
        /// <param name="orderId">代理系统订单号</param>
        /// <param name="teamOrderId">项目组订单号,status = Z 才传此字段</param>
        /// <param name="chaseOrderId">追分订单号,status = Z 才传此字段</param>
        /// <param name="payType">支付类型</param>
        /// <param name="amount">单位"分" 必须大于零</param>
        /// <param name="presentAmount">优惠金额,单位"分", 无优惠传0</param>
        /// <param name="status">S 向玩家上分, Z 向玩家扣分</param>
        /// <param name="time">请求时间戳(秒)</param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool FinishedOrderShanfu(int playerId, string orderId, string teamOrderId, string chaseOrderId, int payType, int amount, int presentAmount, string status, int time, ref string localOrderID, ref string message);
    
        /// <summary>
        /// 提现成功异步回调
        /// </summary>
        /// <param name="orderId">订单号</param>
        /// <param name="status">第三方状态</param>
        /// <param name="transID">第三方订单号</param>
        /// <param name="transData">第三方数据</param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool WithdrawCallbackByCK(int orderId, int status, string transID, string transData, ref string message);

        // <summary>
        /// 河马代付提现成功异步回调
        /// </summary>
        /// <param name="orderId">订单号</param>
        /// <param name="status">第三方状态</param>
        /// <param name="transID">第三方订单号</param>
        /// <param name="transData">第三方数据</param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool WithdrawCallbackByHema(string orderId, string status, string transID, string transData, ref string message);

        /// <summary>
        /// 代付提现成功异步回调
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="status"></param>
        /// <param name="transID"></param>
        /// <param name="transData"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool WithdrawCallback(string orderId, int status, string transID, string transData, ref string message);

        /// <summary>
        /// 客服手動補單支付金額
        /// </summary>
        /// <param name="adminID">後台操作者ID</param>
        /// <param name="CSOrderID">订单ID</param>
        /// <param name="CSAmount">充值補償金额(帶入人民币)</param>
        /// <param name="CSMemo">客服備註</param>
        /// <param name="IPAddress"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool FinishedOrderByCS(int adminID, string CSOrderID, string CSAmount, string CSMemo, string IPAddress, ref string message);

        bool CreateRedirectData(RedirectData order, ref string message);

        RedirectData GetRedirectData(int orderId, string gid);

        #endregion
    }
}
