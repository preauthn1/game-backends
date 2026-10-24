using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using game.Entity.Pay;
using game.Infrastructure;
using Newtonsoft.Json;
using static game.WebSite.PaymentHelper.tungyi;
using static game.WebSite.PayConfig;

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

        /// <summary>
        /// 根据ID获取支付订单
        /// </summary>
        public static List<Entity.Payment.CDPayOrder> CDGetAEPayOrderByID(int orderID)
        {
            try
            {
                var list = SiteHelper.GetAPIResult<List<Entity.Payment.CDPayOrder>>(string.Format("/Payment/CDGetPayOrderByID?orderID={0}", orderID));
                return list;
            }

            catch (Exception ex)
            {
                return null;
            }
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

        public static bool CDNewModifyTransID(int orderID, string transID, int gwid)
        {
            return SiteHelper.GetAPIResult<APIResult>(string.Format("/Pay/CDNewModifyTransID?orderID={0}&transID={1}&gwid={2}", orderID, transID, gwid)).Result;
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

        public static bool CDFinishAEOrder(int orderID, Int64 amount, string ipAddress, string transID, string data, ref string message)
        {
            var api = SiteHelper.GetAPIResult<APIResult>(string.Format("/Pay/CDFinishAEOrder?orderID={0}&amount={1}&ipAddress={2}&transID={3}&transData={4}",
                orderID, amount, HttpContext.Current.Server.UrlEncode(ipAddress), transID, HttpContext.Current.Server.UrlEncode(data)
                ));
            SiteHelper.Log($"{orderID} CDFinishAEOrder: Result={api.Result} message:{api.Message}", FinishOrder_logFile);
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
        /// <param name="orderId">支付中心订单号</param>
        /// <param name="teamOrderId">项目组订单号</param>
        /// <param name="orderAmount">支付金额,单位:分</param>
        /// <param name="realAmount">实际支付金额,单位:分</param>
        /// <param name="status">订单状态,pending:处理中,success:处理成功,fail:处理失败</param>
        /// <param name="time">交易结束时间,unix秒级时间戳</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static APIResult WithdrawCallback(string orderId, string teamOrderId, int orderAmount, int realAmount, string status, string time)
        {
            var res = SiteHelper.GetAPIResult<APIResult>($"/Pay/WithdrawCallback?orderId={orderId}&teamOrderId={teamOrderId}"
                + $"&orderAmount={orderAmount}&realAmount={realAmount}&status={status}&time={time}");
            return res;
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
        #endregion

        public static string NewAmountList(string guid, int platformID)
        {
            var responseString = string.Empty;
            try
            {
                var data = new List<PayAmountInfo>();
                var listAmount = new Amount() { data = new Data() };
                var list = SiteHelper.GetAPIResult<List<PayGateway>>($"/Payment/GetAmountList?platformID={platformID}");

                //排序
                var payList = from pro in list
                              orderby pro.Scale descending, pro.Sort descending 
                              select pro;

                foreach (var i in payList)
                {
                    //if (data.Any(x => x.typeName == i.TypeName))
                    //    continue;

                    var payInfo = new PayAmountInfo()
                    {
                        typeID = i.TypeID,
                        popular = i.Popular,
                        sort = i.Sort,
                        payType = Convert.ToInt32(i.TradeCode),
                        amounts = i.AmountSort,
                        minAmount = Convert.ToInt32(i.MinValue),
                        maxAmount = Convert.ToInt32(i.MaxValue),
                        status = i.Status ? 1 : 0,
                        isRange = i.IsRange ? 1 : 0,
                        gwid = i.GWID,
                        gatewayName = i.GatewayName,
                        typeName = i.TypeName
                    };

                    data.Add(payInfo);
                }

                listAmount.data.payAmountInfo = data;
                responseString = JsonConvert.SerializeObject(listAmount);
            }
            catch (Exception ex)
            {
                SiteHelper.Log(guid, string.Format("取得支付金额接口amountlist [error] {0}", ex), PayConfig.AE_logFile);
            }

            return responseString;
        }

        public static string MergeAmountList(string guid, int platformID)
        {
            var responseString = string.Empty;
            try
            {
                var data = new List<PayAmountInfo>();
                var listAmount = new Amount() { data = new Data() };
                var list = SiteHelper.GetAPIResult<List<PayGateway>>($"/Payment/GetAmountList?platformID={platformID}");

                //固額分類調整
                SpecialTypeID(ref list);

                //合併算法
                list = MergeListByTypeID(list); 

                //排序
                var payList = from pro in list
                              orderby pro.Scale descending, pro.Sort descending
                              select pro;

                foreach (var i in payList)
                {
                    var payInfo = new PayAmountInfo()
                    {
                        typeID = i.TypeID,
                        popular = i.Popular,
                        sort = i.Sort,
                        payType = Convert.ToInt32(i.TradeCode),
                        amounts = i.AmountSort,
                        minAmount = Convert.ToInt32(i.MinValue),
                        maxAmount = Convert.ToInt32(i.MaxValue),
                        status = i.Status ? 1 : 0,
                        isRange = i.IsRange ? 1 : 0,
                        gwid = i.GWID,
                        gatewayName = i.GatewayName,
                        typeName = i.TypeName
                    };

                    data.Add(payInfo);
                }

                listAmount.data.payAmountInfo = data;
                responseString = JsonConvert.SerializeObject(listAmount);
            }
            catch (Exception ex)
            {
                SiteHelper.Log(guid, string.Format("取得支付金额接口amountlist [error] {0}", ex), PayConfig.AE_logFile);
            }

            return responseString;
        }
        public static void SpecialTypeID(ref List<PayGateway> list)
        {
            //請參考enum
            var referenceEnum = PayType.Alipay;

            //Alipay
            //AliQRpay
            //AliFixedpay
            //Weixin
            //WeixinQR
            //WeixinFixed

            foreach (var i in list)
            {
                if (i.TypeID == 1 || i.TypeID == 11)
                {
                    if (!i.IsRange) //支付寶固額
                    {
                        i.TypeID = 12;
                        i.Sort += 1;
                    } else
                    {
                        i.TypeID = 1;
                    }
                }
                else if ((i.TypeID == 2 || i.TypeID == 21)) 
                {
                    if (!i.IsRange) //微信固額
                    {
                        i.TypeID = 22;
                        i.Sort += 1;
                    }
                    else
                    {
                        i.TypeID = 2;
                    }
                }
            }
        }

        public static List<PayGateway> MergeListByTypeID(List<PayGateway> list)
        {
            var returnList = new List<PayGateway>();
            var groups = list.GroupBy(x => x.TypeID).Select(x => x.FirstOrDefault());
            
            foreach (var g in groups) //開始按TypeID合併
            {
                var rows = from row in list
                           where row.TypeID == g.TypeID
                           select row;

                var defaultRow = rows.FirstOrDefault();

                List<string> amountList = new List<string>();
                foreach (var row in rows)
                {
                    defaultRow.MinValue = (row.MinValue < defaultRow.MinValue) ? row.MinValue : defaultRow.MinValue; //下限
                    defaultRow.MaxValue = (row.MaxValue > defaultRow.MaxValue) ? row.MaxValue : defaultRow.MaxValue; //上限
                    //快選金額
                    var arr = row.AmountSort.Split(',');
                    amountList.AddRange(arr);
                }

                //相同金額的合併
                var newAmountList = amountList
                    .GroupBy(s => s).Select(s => new
                    {
                        Metric = Convert.ToInt32(s.Key),
                        Count = s.Count()
                    }).OrderByDescending(s => s.Metric)
                    .ToList();

                var amounts = new List<string>();

                int min = newAmountList.Min(s => s.Metric);
                var itemToRemove = newAmountList.SingleOrDefault(r => r.Metric == min);
                newAmountList.Remove(itemToRemove);
                amounts.Add(min.ToString()); //最小 最大金額 務必要在項目中，優先加進amountList

                int max = newAmountList.Max(s => s.Metric);
                itemToRemove = newAmountList.SingleOrDefault(r => r.Metric == max);
                newAmountList.Remove(itemToRemove);
                amounts.Add(max.ToString());

                foreach(var a in newAmountList)
                {
                    //UI呈現上最多12個
                    if (amounts.Count()>=12)
                    {
                        break;
                    }
                    amounts.Add(a.Metric.ToString());
                }

                amountList = amounts.OrderBy(a => Convert.ToInt32(a)).ToList();
                
                defaultRow.AmountSort = string.Join(",", amountList);
                defaultRow.GatewayName = "";
                defaultRow.GWID = 88;
                defaultRow.TradeCode = defaultRow.TypeID.ToString();
                returnList.Add(defaultRow);
            }
            return returnList;
        }
     

        public static string RandomAmountList(string guid, int platformID)
        {
            var responseString = string.Empty;
            try
            {
                var data = new List<PayAmountInfo>();
                var listAmount = new Amount() { data = new Data() };
                var list = SiteHelper.GetAPIResult<List<PayGateway>>($"/Payment/GetAmountList?platformID={platformID}");

                //random
                Random rnd = new Random();
                foreach (var i in list)
                {
                    i.Popular = rnd.Next(1, 100);
                }
                //排序
                var payList = from pro in list
                              orderby pro.Scale descending, pro.Sort descending, pro.Popular descending
                              select pro;

                foreach (var i in payList)
                {
                    if (data.Any(x => x.typeName == i.TypeName))
                        continue;

                    var payInfo = new PayAmountInfo()
                    {
                        typeID = i.TypeID,
                        popular = i.Popular,
                        sort = i.Sort,
                        payType = Convert.ToInt32(i.TradeCode),
                        amounts = i.AmountSort,
                        minAmount = Convert.ToInt32(i.MinValue),
                        maxAmount = Convert.ToInt32(i.MaxValue),
                        status = i.Status ? 1 : 0,
                        isRange = i.IsRange ? 1 : 0,
                        gwid = i.GWID,
                        gatewayName = i.GatewayName,
                        typeName = i.TypeName
                    };

                    data.Add(payInfo);
                }

                listAmount.data.payAmountInfo = data;
                responseString = JsonConvert.SerializeObject(listAmount);
            }
            catch (Exception ex)
            {
                SiteHelper.Log(guid, string.Format("取得支付金额接口amountlist [error] {0}", ex), PayConfig.AE_logFile);
            }

            return responseString;
        }

        public static Entity.Payment.PayGateway GetPayGateway(int PayTypeID)
        {
            var list = SiteHelper.GetAPIResult<List<Entity.Payment.PayGateway>>($"/Payment/CDGetPayGateway?PayTypeID={PayTypeID}");
            return list[0];
        }
    }
}