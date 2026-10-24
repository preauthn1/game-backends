using System;
using example.invalid;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using game.Entity.Pay;
using game.Infrastructure;
using static game.WebSite.PaymentHelper.Gash;

namespace game.WebSite
{
    public static class Service
    {
        #region 主后台
        /// <summary>
        /// 创建支付订单
        /// </summary>
        public static bool CDCreateOrder(CDPayOrder order, ref string message)
        {
            var api = SiteHelper.PostAPIResult<CDPayOrder>("/Pay/CDCreateOrder", order);
            message = api.Message;
            if (api.Result == true) order.Id = Newtonsoft.Json.JsonConvert.DeserializeObject<CDPayOrder>(api.Extend.ToString()).Id;
            return api.Result;
        }

        /// <summary>
        /// 根据ID获取支付订单
        /// </summary>
        public static CDPayOrder CDGetPayOrderByID(int orderID)
        {
            return SiteHelper.GetAPIResult<CDPayOrder>(string.Format("/Pay/CDGetPayOrderByID?orderID={0}", orderID));
        }

        public static bool CreateGashPaySettleStatus(SettleRequest settleRequest, bool isSuccess)
        {
            return SiteHelper.GetAPIResult<APIResult>(
                string.Format("/Pay/CreateGashPaySettleStatus?msgType={0}&pcode={1}&cid={2}&coid={3}&cuid={4}&paid={5}&amount={6}&erqc={7}&isSuccess={8}"
                , settleRequest.MSG_TYPE, settleRequest.PCODE, settleRequest.CID, settleRequest.COID, settleRequest.CUID, settleRequest.PAID, settleRequest.AMOUNT, settleRequest.ERQC, isSuccess)).Result;
        }

        /// <summary>
        /// 修改第三方交易渠道
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static bool CDUpdateGWID(CDPayOrder order)
        {
            var api = SiteHelper.PostAPIResult<CDPayOrder>("/Pay/CDUpdateGWID", order);
            return api.Result;
        }

        /// <summary>
        /// 修改第三方交易标识
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="transID"></param>
        /// <returns></returns>
        public static bool CDModifyTransID(int orderID, string transID)
        {
            return SiteHelper.GetAPIResult<APIResult>(string.Format("/Pay/CDModifyTransID?orderID={0}&transID={1}", orderID, transID)).Result;
        }

        /// <summary>
        /// 完成支付订单
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="amount"></param>
        /// <param name="ipAddress"></param>
        /// <param name="transID"></param>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool CDFinishOrder(int orderID, Int64 amount, string ipAddress, string transID, string data, ref string message)
        {
            var api = SiteHelper.GetAPIResult<APIResult>(string.Format("/Pay/CDFinishOrder?orderID={0}&amount={1}&ipAddress={2}&transID={3}&transData={4}",
                orderID, amount, HttpContext.Current.Server.UrlEncode(ipAddress), transID, HttpContext.Current.Server.UrlEncode(data)
                ));
            message = api.Message;
            return api.Result;
        }


        /// <summary>
        /// 第三方订单失败
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="transID"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool CDFailOrder(int orderID, string transID, string data)
        {
            transID = string.IsNullOrEmpty(transID) ? "0" : transID;

            var api = SiteHelper.GetAPIResult<APIResult>(string.Format("/Pay/CDFailOrder?orderID={0}&transID={1}&transData={2}",
                orderID, transID, HttpContext.Current.Server.UrlEncode(data)
                ));
            return api.Result;
        }


        /// <summary>
        /// 完成統一代理支付订单(久聊)
        /// </summary>
        /// <param name="orderType"></param>
        /// <param name="orderID"></param>
        /// <param name="playerID"></param>
        /// <param name="transID"></param>
        /// <param name="amount"></param>
        /// <param name="ipAddress"></param>
        /// <param name="data"></param>
        /// <param name="status"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool CDFinishThirdPartyOrder(int orderType, int orderID, string playerID, string transID, Int64 amount, string ipAddress, string data, string status, ref string message)
        {
            var api = SiteHelper.GetAPIResult<APIResult>(
                $"/Pay/CDFinishThirdPartyOrder?orderType={orderType}&orderID={orderID}&playerID={playerID}&transID={transID}&amount={amount}&ipAddress={HttpContext.Current.Server.UrlEncode(ipAddress)}&transData={HttpContext.Current.Server.UrlEncode(data)}&status={status}"
                );
            message = api.Message;
            return api.Result;
        }


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
        /// <returns></returns>
        public static APIResult FinishedOrderShanfu(int playerId, string orderId, string teamOrderId, string chaseOrderId, int payType, int amount, int presentAmount, string status, int time)
        {
            var api = SiteHelper.GetAPIResult<APIResult>($"/Pay/FinishedOrderShanfu?playerId={playerId}&orderId={orderId}&teamOrderId={teamOrderId}&chaseOrderId={chaseOrderId}"
                + $"&payType={payType}&amount={amount}&presentAmount={presentAmount}&status={status}&time={time}");
            return api;
        }       

        /// <summary>
        /// 提现成功异步回调
        /// </summary>
        /// <param name="orderId">订单号</param>
        /// <param name="status">第三方状态</param>
        /// <param name="transID">第三方订单号</param>
        /// <param name="transData">第三方数据</param>
        /// <returns></returns>
        public static APIResult WithdrawCallbackByCK(int orderId, int status, string transID, string transData)
        {
            var res = SiteHelper.GetAPIResult<APIResult>($"/Pay/WithdrawCallbackByCK?orderId={orderId}&status={status}&transID={transID}&transData={transData}");
            return res;
        }

        public static APIResult WithdrawCallbackByHema(string orderId, string status, string transID, string transData)
        {
            var res = SiteHelper.GetAPIResult<APIResult>($"/Pay/WithdrawCallbackByHema?orderId={orderId}&status={status}&transID={transID}&transData={transData}");
            return res;
        }

        /// <summary>
        /// 提现成功异步回调
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="status"></param>
        /// <param name="transID"></param>
        /// <param name="transData"></param>
        /// <returns></returns>
        public static APIResult WithdrawCallback(string orderId, int status, string transID, string transData)
        {
            var res = SiteHelper.GetAPIResult<APIResult>($"/Pay/WithdrawCallback?orderId={orderId}&status={status}&transID={transID}&transData={transData}");
            return res;
        }

        public static APIResult CreateRedirectData(int giwd, string formData)
        {
            var order = new RedirectData()
            {
                GWID = giwd,
                FormData = formData
            };
        
            var api = SiteHelper.PostAPIResult<RedirectData>("/Pay/CreateRedirectData", order);
            string url = string.Empty;

            if (api.Result == true)
            {
                var item = Newtonsoft.Json.JsonConvert.DeserializeObject<RedirectData>(api.Extend.ToString());
                url = $"{PayConfig.WebSiteHost}orderId={item.Id}&gid={item.GUID}";
            }
            
            return new APIResult(api.Result, api.Message, url);
        }

        #endregion
    }
}