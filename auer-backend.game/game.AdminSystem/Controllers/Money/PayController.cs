using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using System.Web.Mvc;
using FW.Core.Helper;
using FW.Core.Helper.Enum;
using game.Entity.Pay;
using game.Infrastructure;
using NPOI.XSSF.UserModel;
using game.Entity.Area;
using Newtonsoft.Json;

namespace game.AdminSystem.Controllers.Money
{
    /// <summary>
    /// 金币管理 - 支付订单管理
    /// </summary>
    public class PayController : BaseController
    {

        /// <summary>
        /// 3-支付订单查询
        /// </summary>
        /// <returns></returns>
        public ActionResult OrderList(int? platformids, string submitType, int? areaID = null, int? userID = null, string loginName = "", string nickName = "", int? orderID = null, int? GWID = null, string ipAddress = null, int? payTypeID = null, CDOrderStatus? status = null, int timeType = 0, DateTime? createBeginTime = null, DateTime? createEndTime = null, int p = 1)
        {
            if (createBeginTime == null) createBeginTime = Convert.ToDateTime(DateTime.Now.ToShortDateString());

            var adminID = SiteService.GetAdminUserID();
            userID = UserService.GetUserID(userID, loginName, nickName);

            switch (submitType)
            {
                case "Export":
                    var datas = SiteHelper.GetAPIResult<PagedList<OrderListDTO>>($"/Pay/CDGetPayOrder?orderID={orderID}&userID={userID}&GWID={GWID}&ipAddress={ipAddress}"
                        + $"&payTypeID={payTypeID}&status={status}&timeType={timeType}&createBeginTime={createBeginTime}&createEndTime={createEndTime}&pageIndex={p}"
                        + $"&platformId={platformids}&areaid={areaID}&adminId={adminID}&pageSize={65500}");
                    return this.exportOrderList(datas.PageData, createBeginTime, createEndTime);
                default:
                    var list = SiteHelper.GetAPIResult<PagedList<OrderListDTO>>($"/Pay/CDGetPayOrder?orderID={orderID}&userID={userID}&GWID={GWID}&ipAddress={ipAddress}&payTypeID={payTypeID}&status={status}"
                        + $"&timeType={timeType}&createBeginTime={createBeginTime}&createEndTime={createEndTime}&pageIndex={p}&platformId={platformids}&areaid={areaID}&adminId={adminID}");
                    return View(list);
            }
        }

        /// <summary>
        /// 3-支付订单查询 4-手动完成充值
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        public ActionResult FinishOrder(int orderID, string transID, string transData)
        {
            var payOrder = SiteHelper.GetAPIResult<CDPayOrder>(string.Format("/Pay/CDGetPayOrderByID?orderID={0}", orderID));

            var api = SiteHelper.GetAPIResult<APIResult>(string.Format("/Pay/CDFinishOrder?orderID={0}&amount={1}&ipAddress={2}&transID={3}&transData={4}",
                    payOrder.Id, payOrder.Amount, Server.UrlEncode(SystemHelper.UserIPAddress), transID, transData));


            var data = new
            {
                status = api.Result,
                msg = api.Message
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 3-支付订单查询
        /// </summary>
        /// <returns></returns>
        public ActionResult PayTypeList()
        {
            return View();
        }

        public ActionResult PlatformPayment()
        {
            return View();
        }

        public ActionResult GetPayTypeAll()
        {
            var operatorID = CurrentUser.UserID;
            var payTypeList = SiteHelper.GetAPIResult<List<PayGateway>>($"/Pay/GetCompanyPayType?operatorID={operatorID}");
            var platformList = DataService.GetAllPlatform().FindAll(m => SiteHelper.GetPlatformIDs().Contains(m.Id));

            var platforms = from pro in platformList
                            select new
                            {
                                id = pro.Id,
                                name = pro.Name
                            };

            var payTypes = from pro in payTypeList
                           select new
                            {
                               GWID = pro.GWID,
                               GatewayName = pro.GatewayName,
                               TypeID = pro.TypeID,
                               TypeName = pro.TypeName
                           };

            var data = new
            {
                payment = payTypes,
                paltform = platforms

            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUnionPlatformPayType(int platformID)
        {
            var operatorID = CurrentUser.UserID;
            var payTypeList = SiteHelper.GetAPIResult<List<PayGateway>>($"/Pay/CDGetUnionPlatformPayType?operatorID={operatorID}&platformID={platformID}");

            var payTypes = from pro in payTypeList
                           select new
                           {
                               GWID = pro.GWID,
                               GatewayName = pro.GatewayName,
                               TypeID = pro.TypeID,
                               TypeName = pro.TypeName
                           };

            var data = new
            {
                payment = payTypes
            };
            return Json(data, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult SetUnionPlatformPayType(int platformID, string gateway)
        {
            var operatorID = CurrentUser.UserID;
            List<PayGateway> payGatewayList = JsonConvert.DeserializeObject<List<PayGateway>>(gateway);
            var paytype = string.Join("|", payGatewayList.Select(x => $"{x.GWID},{x.TypeID},{(x.Status?1:0)}"));

            var api = SiteHelper.GetAPIResult<APIResult>($"/Pay/SetUnionPlatformPayType?operatorID={operatorID}&platformID={platformID}&param={paytype}");
            var data = new
            {
                status = api.Result,
                msg = api.Message
            };
            return Json(data);
        }

        /// <summary>
        /// 获取桌面列表数据
        /// </summary>
        /// <param name="serverID">房间编号</param>
        /// <returns></returns>
        public ActionResult GetPayType()
        {
            var operatorID = CurrentUser.UserID;
            var payTypes = SiteHelper.GetAPIResult<List<PayGateway>>($"/Pay/CDGetPayType?operatorID={operatorID}");

            if (payTypes != null)
            {
                return Json(payTypes, JsonRequestBehavior.AllowGet);
            }
            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetPayType(int GWID, int typeID, string amountSort, string tradeCode, bool isRange, bool status)
        {
            var api = SiteHelper.GetAPIResult<APIResult>(string.Format("/Pay/CDSetPayType?GWID={0}&typeID={1}&amountSort={2}&tradeCode={3}&isRange={4}&status={5}", GWID, typeID, amountSort, tradeCode, isRange, status));

            var data = new
            {
                status = api.Result,
                msg = api.Message
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SetPayGateway(string gateway)
        {
            var api = SiteHelper.GetAPIResult<APIResult>(string.Format("/Pay/SetPayGateway?gateway={0}", gateway));
            var data = new
            {
                status = api.Result,
                msg = api.Message
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PlatformPage()
        {
            return View();
        }

        /// <summary>
        /// 支付渠道统计
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPlatformLog(int? draw, int? start, int length, int? platformId = null, int? areaID = null, DateTime? statTime = null, DateTime? endTime = null, int? payTypeID = null)
        {
            if (statTime == null) statTime = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            var pageIndex = Math.Ceiling((decimal)start.Value / length) + 1;

            var operatorID = CurrentUser.UserID;
            var api = SiteHelper.GetAPIResult<PagedList<PlatformLog>>($"/Pay/GetPlatformLog?operatorID={operatorID}&platformId={platformId}&areaID={areaID}&statTime={statTime}"
                + $"&endTime={endTime}&payTypeID={payTypeID}&pageIndex={pageIndex}&pageSize={length}");

            var PageData = api.PageData.Where(x => !string.IsNullOrEmpty(x.TypeName));
            var _calculatorTotal = api.PageData.Where(data => !string.IsNullOrEmpty(data.TransData)).Select(x => x.TransData).FirstOrDefault();

            var query = from pro in PageData
                        select new
                        {
                            TypeName = pro.TypeName,
                            GatewayName = pro.GatewayName,
                            TotalAmount = pro.TotalAmount,
                            TotalPointAmount = pro.TotalPointAmount,
                            TotalCount = pro.TotalCount,
                            SuccessCount = pro.SuccessCount,
                            SuccessRate = pro.SuccessRate
                        };

            return Json(new
            {
                draw = draw,
                recordsTotal = PageData.Count(),
                recordsFiltered = PageData.Count(),
                success = true,
                data = query,
                transData = _calculatorTotal
            });
        }

        private ActionResult exportOrderList(List<OrderListDTO> datas, DateTime? beginTime, DateTime? endTime)
        {
            var _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "ExportTemplat", "Pay_OrderList.xlsx");

            using (FileStream file = new FileStream(_path, FileMode.Open, FileAccess.Read))
            {
                #region 畫Excel

                var _workbook = new XSSFWorkbook(file);
                var _sheet = _workbook.GetSheetAt(0);
                file.Close();

                _sheet.GetRow(1).GetCell(10).SetCellValue(string.Format("{0}~{1}", beginTime.Value.ToString("yyyy-MM-dd HH:mm:ss"), endTime.HasValue ? endTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : ""));

                if (datas.Count != 0)
                {
                    for (var i = 0; i < datas.Count; i++)
                    {
                        OrderListDTO _order = datas[i];
                        var _row = _sheet.CreateRow(3 + i);
                        var lastUpdateTime = _order.LastUpdateTime != null ? _order.LastUpdateTime.Value.ToString("yyyy/MM/dd HH:mm:ss") : string.Empty;

                        _row.CreateCell(0).SetCellValue(_order.PlatformName);
                        _row.CreateCell(1).SetCellValue(_order.CreateTime.ToString("yyyy/MM/dd HH:mm:ss"));
                        _row.CreateCell(2).SetCellValue(lastUpdateTime);
                        _row.CreateCell(3).SetCellValue(_order.OrderID);
                        _row.CreateCell(4).SetCellValue(_order.GatewayName);
                        _row.CreateCell(5).SetCellValue(_order.PayTypeID.GetDisplayName());
                        _row.CreateCell(6).SetCellValue(_order.UserID);
                        _row.CreateCell(7).SetCellValue(_order.LoginName.ToString());
                        _row.CreateCell(8).SetCellValue(_order.UserLevel);
                        _row.CreateCell(9).SetCellValue(_order.Amount);
                        _row.CreateCell(10).SetCellValue(_order.Status.GetDisplayName());
                        _row.CreateCell(11).SetCellValue(_order.IPAddress);
                        _row.CreateCell(12).SetCellValue(_order.TransID);
                        _row.CreateCell(13).SetCellValue(_order.TransData);
                    }
                }

                #endregion

                using (var exportData = new MemoryStream())
                {
                    _workbook.Write(exportData);
                    string saveAsFileName = string.Format("充值记录_{0:d}.xlsx", DateTime.Now).Replace("/", "-");
                    byte[] bytes = exportData.ToArray();
                    return File(bytes, "application/vnd.ms-excel", saveAsFileName);
                }
            }
        }
    }
}