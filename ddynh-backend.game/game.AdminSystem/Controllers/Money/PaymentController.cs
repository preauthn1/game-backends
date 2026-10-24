using System;
using example.invalid;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;
using game.Entity.Payment;
using game.Entity.HttpClient;
using game.Infrastructure;
using Newtonsoft.Json;
using FW.Core.Helper;
using FW.Core.Helper.Enum;
using NPOI.XSSF.UserModel;
using Newtonsoft.Json.Linq;
using game.Infrastructure.Extensions;
using game.Entity.ViewModels;
using game.Entity.Money;
using game.Entity.Area;
using game.Entity;
using game.Entity.Game;
using System.Web.Script.Serialization;

namespace game.AdminSystem.Controllers.Money
{
    public class PaymentController : BaseController
    {
        public static string AEPayAuth = ConfigurationManager.AppSettings["AEPayAuth"];
        public static string AEPayUrl = ConfigurationManager.AppSettings["AEPayUrl"];

        //支付訂單查詢
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 3-支付订单查询
        /// </summary>
        /// <returns></returns>
        public ActionResult OrderList(int? platformids, string submitType, int? areaID = null, int? userID = null, string loginName = "", string nickName = "", 
            int? orderID = null, int? GWID = null, string ipAddress = null, int? payTypeID = null, game.Entity.Pay.CDOrderStatus? status = null, int timeType = 0, 
            DateTime? createBeginTime = null, DateTime? createEndTime = null, int p = 1 , int pageSize = 20)
        {
            if (createBeginTime == null) createBeginTime = Convert.ToDateTime(DateTime.Now.ToShortDateString());

            var adminID = SiteService.GetAdminUserID();
            ViewBag.operatorID = adminID;
            userID = UserService.GetUserID(userID, loginName, nickName);

            switch (submitType)
            {
                case "Export":
                    var datas = SiteHelper.GetAPIResult<PagedList<game.Entity.Pay.OrderListDTO>>($"/Pay/CDGetPayOrder?orderID={orderID}&userID={userID}&GWID={GWID}&ipAddress={ipAddress}"
                        + $"&payTypeID={payTypeID}&status={status}&timeType={timeType}&createBeginTime={createBeginTime}&createEndTime={createEndTime}&pageIndex={p}"
                        + $"&platformId={platformids}&areaid={areaID}&adminId={adminID}&pageSize={20000}");
                    return this.exportOrderList(datas.PageData, createBeginTime, createEndTime);
                default:
                    var list = SiteHelper.GetAPIResult<PagedList<game.Entity.Pay.OrderListDTO>>($"/Pay/CDGetPayOrder?orderID={orderID}&userID={userID}&GWID={GWID}&ipAddress={ipAddress}&payTypeID={payTypeID}&status={status}"
                        + $"&timeType={timeType}&createBeginTime={createBeginTime}&createEndTime={createEndTime}&pageIndex={p}&pageSize={pageSize}&platformId={platformids}&areaid={areaID}&adminId={adminID}");
                    return View(list);
            }
        }

        private ActionResult exportOrderList(List<game.Entity.Pay.OrderListDTO> datas, DateTime? beginTime, DateTime? endTime)
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
                        game.Entity.Pay.OrderListDTO _order = datas[i];
                        var _row = _sheet.CreateRow(3 + i);
                        var lastUpdateTime = _order.LastUpdateTime != null ? _order.LastUpdateTime.Value.ToString("yyyy/MM/dd HH:mm:ss") : string.Empty;

                        _row.CreateCell(0).SetCellValue(_order.PlatformName);
                        _row.CreateCell(1).SetCellValue(_order.CreateTime.ToString("yyyy/MM/dd HH:mm:ss"));
                        _row.CreateCell(2).SetCellValue(lastUpdateTime);
                        _row.CreateCell(3).SetCellValue(_order.OrderID);
                        _row.CreateCell(4).SetCellValue(_order.GatewayName);
                        _row.CreateCell(5).SetCellValue(_order.TypeName);
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

        public ActionResult PayGatewayList()
        {
            return View();
        }

        public ActionResult Reset()
        {
            ViewBag.AEPayAuth = AEPayAuth;
            ViewBag.AEPayUrl = AEPayUrl;
            return View();
        }

        [HttpGet]
        public ActionResult CheckOrder(string orderNo)
        {
            try
            {
                //至3rd Party取回資料結構
                var url = $"{AEPayUrl}/payins/{orderNo}/check";
                var httpClient = SiteHelper.HttpClient(url, "PUT", "", AEPayAuth);
                var responseString = httpClient.Data.ToString();
                return Json(responseString, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [HttpPost]
        public ActionResult Reset(string url, string auth)
        {
            if (url == null)
                return Alert("操作失败：Third Party URL不存在！");
            if (auth == null)
                return Alert("操作失败：Authorization不存在！");

            //至3rd Party取回資料結構
            url = url + "/thirdparty";
            var httpClient = SiteHelper.HttpClient(url, "GET", "", auth);
            var str = httpClient.Data.ToString();
            var message = "更新失败";

            if (httpClient.StatusCode == 200)
            {
                List<AEPaymentResult> PaymentGatewayList = JsonConvert.DeserializeObject<List<AEPaymentResult>>(str); //反序列化

                foreach (var c in PaymentGatewayList)
                {
                    bool payin = (c.payinSetting is null) ? false : true;
                    bool payout = (c.payoutSetting is null) ? false : true;

                    //add pay gateway
                    var api = SiteHelper.GetAPIResult<APIResult>($"/Payment/AddPayGateway?code={c.thirdPartyCode}&name={c.thirdPartyName}&cur={string.Join(",", c.currencies)}&info={c.requiredMerchantInfo}&supPayin={c.supportPayin}&supPayout={c.supportPayout}&isRange={c.reviseAmountByResponse}&bankCodes={string.Join(",", c.bankCodes)}");
                    var status = api.Result;
                    if (status == true)
                    {
                        //add paytype
                        if (payin)
                        {
                            foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, PayinSetting>>> type in c.payinSetting)
                            {
                                //CNY
                                foreach (KeyValuePair<string, Dictionary<string, PayinSetting>> method in type.Value)
                                {
                                    //ZHIFUBAO
                                    foreach (KeyValuePair<string, PayinSetting> setting in method.Value)
                                    {
                                        var fixedAmount = string.Empty;
                                        if (setting.Value.amountFixed == true && setting.Value.fixedAmount.Count() > 0)
                                        {
                                            var tag = "";
                                            foreach (var fa in setting.Value.fixedAmount)
                                            {
                                                fixedAmount += tag + (fa * 100);
                                                tag = ",";
                                            }
                                        }

                                        //列出全部銀行加入
                                        if (method.Key == "DEBIT_CARD" && setting.Key == "ONLINE_BANK")
                                        {
                                            foreach (string bank in c.bankCodes)
                                            {
                                                var arBbank = bank.Split('_');
                                                if (arBbank.Length == 2)
                                                {
                                                    //判斷bankcode前綴與curreices相同
                                                    if (type.Key == bank.Substring(0, type.Key.Length))
                                                    {
                                                        var apiin = SiteHelper.GetAPIResult<APIResult>($"/Payment/AddPayType?code={c.thirdPartyCode}&type=1&cur={type.Key}&bankCode={arBbank[1]}&minAmount={setting.Value.minAmount * 100}&maxAmount={setting.Value.maxAmount * 100}&amountFixed={setting.Value.amountFixed}&fixedAmount={fixedAmount}");
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var apiin = SiteHelper.GetAPIResult<APIResult>($"/Payment/AddPayType?code={c.thirdPartyCode}&type=1&cur={type.Key}&bankCode={method.Key}_{setting.Key}&minAmount={setting.Value.minAmount * 100}&maxAmount={setting.Value.maxAmount * 100}&amountFixed={setting.Value.amountFixed}&fixedAmount={fixedAmount}");
                                        }
                                    }
                                }
                            }
                        }
                        if (payout)
                        {
                            foreach (KeyValuePair<string, PayoutSetting> type in c.payoutSetting)
                            {
                                foreach (string bank in c.bankCodes)
                                {
                                    if (type.Key == bank.Substring(0, type.Key.Length))
                                    {
                                        var apiout = SiteHelper.GetAPIResult<APIResult>($"/Payment/AddPayType?code={c.thirdPartyCode}&type=2&cur={type.Key}&bankCode={bank.Substring((type.Key.Length + 1))}&minAmount={type.Value.minAmount * 100}&maxAmount={type.Value.maxAmount * 100}");
                                    }
                                }
                            }
                        }
                    }
                }
                message = "更新成功";
            }
            else
            {
                Error err = JsonConvert.DeserializeObject<Error>(str); //反序列化
                message += String.Format(" Message:{0}", err.ErrorCode);
            }
            return Alert(message);
        }

        public ActionResult MerchantList(int gwID)
        {
            var operatorID = Convert.ToInt32(CurrentUser.UserID);

            //取得商戶資訊
            var queryGateway = SiteHelper.GetAPIResult<PayGatewayViewModel>($"/Payment/GetGatewayByID?gwID={gwID}");

            if (!string.IsNullOrEmpty(queryGateway.SettingInfo))
            {
                var json_serializer = new JavaScriptSerializer();
                var SettingInfo = (IDictionary<string, object>)json_serializer.DeserializeObject(queryGateway.SettingInfo);
                ViewBag.SettingInfo = SettingInfo; 
            }
            ViewBag.operatorID = operatorID;
            ViewBag.gwID = gwID;
            ViewBag.GWCode = queryGateway.BankCodes;
            ViewBag.systems = queryGateway.Systems;
            return View();
        }

        [HttpGet]
        public ActionResult GetMerchantList(int gwID, int? platformId = null)
        {
            //取得merchantList
            var operatorID = CurrentUser.UserID;
            var merchantList = SiteHelper.GetAPIResult<List<CDMerchantList>>($"/Payment/CDGetMerchantList?gwID={gwID}&platformId={platformId}&adminID={operatorID}");
            return Json(new { merchant = merchantList }, JsonRequestBehavior.AllowGet);
        }

        //未完
        public ActionResult AddMerchantInfo(int GWID, string infoData)
        {
            return View();
        }

        //支付渠道设定
        public ActionResult Setting()
        {
            return View();
        }

        //支付平台設定
        public ActionResult PlatformSetting()
        {
            return View();
        }


        public ActionResult PayTypeList(int targetID, int systems)
        {
            ViewBag.TargetID = targetID;
            ViewBag.Systems = systems;
            return View();
        }

        [HttpGet]
        public ActionResult GetPayGateway()
        {
            var operatorID = CurrentUser.UserID;
            var payGatewayList = SiteHelper.GetAPIResult<List<PayGateway>>("/Payment/CDGetPayGateway");

            string settingApiUrl = $"ConvertGatewayBankSetting/GetByFilter?Status={NormalActiveStatusEnum.Enable.ToInteger()}";
            var querySettings = SiteHelper.GetAPIResult<List<ConvertGatewayBankSettingEntity>>(settingApiUrl);

            var payGateways = from pro in payGatewayList
                              select new
                              {
                                  ID = pro.Id,
                                  BankCount = querySettings.Where(x=> x.ConvertGatewayID == pro.Id).Count(),
                                  Systems = pro.Systems,
                                  GWCode = pro.GatewayCode,
                                  GWName = pro.GatewayName,
                                  Info = pro.MerchantInfo,
                                  Payin = pro.SupportPayin,
                                  Payout = pro.SupportPayout,
                                  ReviseAmount = (bool)pro.ReviseAmount,
                                  IsActive = pro.Status == (int)NormalActiveStatusEnum.Enable,
                                  CreateTime = pro.CreateTime.ToString("yyyy-MM-dd"),
                                  BankCodes = (pro.BankCodes != null) ? pro.BankCodes.ToString() : ""
                              };
            var data = new
            {
                payment = payGateways
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPayTypeAll()
        {
            var operatorID = CurrentUser.UserID;
            var payTypeList = SiteHelper.GetAPIResult<List<PayType>>($"/Payment/CDGetPayType?operatorID={operatorID}");
            var data = new
            {
                payment = payTypeList
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取桌面列表数据
        /// </summary>
        [HttpGet]
        public ActionResult GetPayType(int targetID)
        {
            var operatorID = CurrentUser.UserID;
            var payTypeList = SiteHelper.GetAPIResult<List<PayType>>($"/Payment/CDGetPayType?operatorID={operatorID}&targetID={targetID}");
            var data = new
            {
                payment = payTypeList
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult InfoSetting(int gwID, int targetID = 0, int? platformid = 0)
        {
            var operatorID = Convert.ToInt32(CurrentUser.UserID);

            //取得商戶資訊
            var queryGateway = SiteHelper.GetAPIResult<PayGatewayViewModel>($"/Payment/GetGatewayByID?gwID={gwID}");

            ViewBag.code = queryGateway.GatewayCode;
            var json_serializer = new JavaScriptSerializer();
            if (!string.IsNullOrEmpty(queryGateway.SettingInfo))
            {
                var SettingInfo = (IDictionary<string, object>)json_serializer.DeserializeObject(queryGateway.SettingInfo);
                ViewBag.SettingInfo = SettingInfo;
            }
            var systems = (int)queryGateway.Systems;

            ViewBag.operatorID = operatorID;
            ViewBag.platformid = platformid;
            ViewBag.gwid = gwID;
            ViewBag.id = targetID;

            //取回當前資訊
            if (targetID > 0)
            {
                var merchantList = SiteHelper.GetAPIResult<List<CDMerchantList>>($"/Payment/CDGetMerchantList?gwID={gwID}&id={targetID}&adminID={operatorID}");
                var merchantInfo = (IDictionary<string, object>)json_serializer.DeserializeObject(merchantList[0].InfoData);
                foreach (var item in merchantInfo)
                {
                    ViewData[$"txt_{item.Key}"] = item.Value;
                }
            }
            return View();
        }

        [HttpPost]
        public ActionResult InfoSetting(FormCollection post)
        {
            var operatorID = CurrentUser.UserID;
            var platformid = post["platformid"];
            int id = (post["id"] is null) ? 0 : Convert.ToInt32(post["id"]);
            int gwid = Convert.ToInt32(post["gwid"]);

            var dictionary = new Dictionary<string, object>();
            string infoStr = string.Empty;

            foreach (var item in post)
            {
                string key = item.ToString();
                if (key != "platformid" && key != "gwid" && key != "id" && key != "submitType")
                {
                    dictionary.Add(key.ToUpper(), post[key]);
                }
            }

            infoStr = JsonConvert.SerializeObject(dictionary);
            string message = "新增失敗";
            var url = $"/Payment/AddMerchantInfo?gwid={gwid}&infodata={infoStr}&merchantId={id}&platformid={platformid}";
            //寫進DB
            var api = SiteHelper.GetAPIResult<APIResult>(url);
            var status = api.Result;
            if (status)
            {
                message = "新增成功";
            }
            return ConfirmReturnList($"/Payment/MerchantList?gwID={gwid}", message);
        }

        public ActionResult UpdateInfoStatus(int gwid, int targetID, int status)
        {
            var api = SiteHelper.GetAPIResult<APIResult>($"/Payment/UpdateInfoStatus?id={targetID}&status={status}");
            var apiStatus = api.Result;
            var message = (apiStatus) ? "更新成功" : "更新失敗";
            return ConfirmReturnList($"/Payment/MerchantList?gwID={gwid}", message);
        }

        public ActionResult AmountList(int? platformid, string submitType)
        {
            var operatorID = Convert.ToInt32(CurrentUser.UserID);
            ViewBag.operatorID = operatorID;

            var query = DataService.GetMyPlatforms();

            //如果用戶只有一家平台 直接查出來
            if (query.Count() == 1)
                platformid = query.FirstOrDefault().Id;

            //介面已經調整成 一定要選下拉選單才能查資料
            if (platformid > 0)
            {
                CheckPlatformPermissionExistOrRedirect(platformid.Value);

                var list = SiteHelper.GetAPIResult<List<PayTypeManagementAmountListModel>>($"/Payment/GetPayTypeManagementAmountList?platformID={platformid}");
                ViewBag.list = list;

                //如果有特定幾個渠道關閉中 顯示hint
                var disabledAreaNames = GetPaymentDisabledAreas(platformid.Value);
                ViewBag.disabledAreaNames = disabledAreaNames;
            }

            ViewBag.CurrentSelectPlatform = platformid ?? 0;
            ViewBag.platformId = platformid;
            return View();
        }


        private List<string> GetPaymentDisabledAreas(int platformID)
        {
            List<string> disabledAreaNames = new List<string>();
            string areaApiUrl = $"/Area/GetArea?platformId={platformID}";
            var queryAreas = SiteHelper.GetAPIResult<List<Area>>(areaApiUrl);


            foreach (var item in queryAreas)
            {
                var convert = item.AreaConfig.JsonConvertToModel<AreaConfigJsonModel>();

                if (convert != null)
                {
                    bool? isEnable = convert.systemConfig?.payConfig?.UIFlagPay;

                    if (isEnable.HasValue && !isEnable.Value)
                    {
                        disabledAreaNames.Add(item.AreaName);                        
                    }
                }
            }
            return disabledAreaNames;
        }








        [HttpGet]
        public ActionResult UpdatePayType()
        {
            var responseString = "{\"result\":\"success\"}";
            return Json(responseString, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public bool UpdatePayTypeDisable(int type, int id = 0, int payTypeId= 0 , int platformId = 0)
        {
            var api = SiteHelper.GetAPIResult<APIResult>($"/Payment/UpdatePayTypeDisable?type={type}&id={id}&payTypeId={payTypeId}&platformId={platformId}");
            var apiStatus = api.Result;
            return apiStatus;
        }

        [HttpGet]
        public ActionResult UpdatePlatformPayType(int id, int payTypeId, int platformId, int? minAmount, int? maxAmount, int isRange, string amountList, int scale = 100, int status = 0)
        {
            //檢查是否包含非法的特殊符號
            if(!string.IsNullOrWhiteSpace(amountList))
            {
                var split = amountList.Split(',').ToList();

                foreach(var eachText in split)
                {
                    string trimText = eachText.Trim();

                    if (eachText.ReplaceAllSpecialChar() != trimText)
                    {
                        var failResult = new
                        {
                            status = false,
                            msg = "储存失败，您输入的固定金额列表包含不允许的符号(e.g. 不允许小数点、特殊符号)"
                        };

                        return Json(failResult, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            var api = SiteHelper.GetAPIResult<APIResult>($"/Payment/UpdatePlatformPayType?id={id}&payTypeId={payTypeId}&platformId={platformId}&minAmount={minAmount}&maxAmount={maxAmount}&isRange={isRange}&amountList={amountList}&scale={scale}&status={status}");
            var data = new
            {
                status = api.Result,
                msg = api.Message
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        /// <summary>
        /// 3-支付订单查询 4-手动完成充值
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        public ActionResult FinishAEOrder(int orderID, string transID, string transData)
        {
            //檢查是否有重複序號
            var api = SiteHelper.GetAPIResult<APIResult>($"/Payment/CheckPaymentOrder?orderID={orderID}&transID={transID}");
            if (api.Result)
            {
                var payList = SiteHelper.GetAPIResult<List<CDPayOrder>>($"/Payment/CDGetPayOrderByID?orderID={orderID}");
                var payOrder = payList[0];
                api = SiteHelper.GetAPIResult<APIResult>($"/Pay/CDFinishAEOrder?orderID={payOrder.Id}&amount={payOrder.Amount}&ipAddress={Server.UrlEncode(SystemHelper.UserIPAddress)}&transID={transID}&transData={transData}");
            }

            var data = new
            {
                status = api.Result,
                msg = api.Message
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }

    public class AEPaymentResult
    {
        public string thirdPartyCode { get; set; }
        public string thirdPartyName { get; set; }
        public List<string> currencies { get; set; }
        public Dictionary<string,
                 Dictionary<string,
                   Dictionary<string, PayinSetting>>> payinSetting { get; set; }
        private Dictionary<string, PayoutSetting> _payoutSetting;
        public Dictionary<string, PayoutSetting> payoutSetting {
            get
            {
                return _payoutSetting;
            }
            set
            {
                _payoutSetting = (value == null) ? new Dictionary<string, PayoutSetting>() : value;
            }
        }
        public int requiredMerchantInfo { get; set; }
        public bool supportPayin { get; set; }
        public bool supportPayout { get; set; }
        public bool reviseAmountByResponse { get; set; }
        public List<string> bankCodes { get; set; }
    }

    public class PayinSetting
    {
        private int _minAmount;
        public int minAmount {
            get
            {
                return _minAmount;
            }
            set
            {
                _minAmount = (String.IsNullOrEmpty(value.ToString())) ? 0 : value;
            }
        }
        private int _maxAmount;
        public int maxAmount
        {
            get
            {
                return _maxAmount;
            }
            set
            {
                _maxAmount = (String.IsNullOrEmpty(value.ToString())) ? 0 : value;
            }
        }
        public bool amountFixed { get; set; }
        public List<int> fixedAmount { get; set; }
    }

    public class PayoutSetting
    {
        public int minAmount { get; set; }
        public int maxAmount { get; set; }
    }
}

