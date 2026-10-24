using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Pay;
using game.Infrastructure;

namespace game.Service
{
    public class PayService : IPayService
    {
        private IRepositoryFactory _repoFactory;
        private IDatabaseFactory _dbFactory;

        public PayService(IRepositoryFactory repoFactory, IDatabaseFactory dbFactory)
        {
            _repoFactory = repoFactory;
            _dbFactory = dbFactory;
        }

        #region  主后台相关功能
        public bool CDCreateOrder(Entity.Pay.CDPayOrder order, ref string message)
        {
            int orderID = 0;
            SqlParameter[] paras = {
                                    new SqlParameter("@intPayTypeID",order.PayTypeID),
                                    new SqlParameter("@intUserID",order.UserID),
                                    new SqlParameter("@lngAmount",order.Amount),
                                    new SqlParameter("@chvIPAddress",order.IPAddress),
                                    new SqlParameter("@chvTransID",order.TransID),
                                    new SqlParameter("@chvTransData",order.TransData),
                                    new SqlParameter("@intOrderID",orderID),
                                    DBHelper.GetProcedureErrorMsg()
                                   };


            paras[6].Direction = System.Data.ParameterDirection.Output;
            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Pay_CreateOrder", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);
            try
            {
                order.Id = paras[6].Value == null ? 0 : Convert.ToInt32(paras[6].Value);
            }
            catch
            {
                order.Id = 0;
            }
            return returnValue == 1;
        }

        public CDPayOrder CDGetPayOrderByID(int orderID)
        {
            return _repoFactory.CreateRepository<CDPayOrder>().GetById(orderID);
        }

        public bool CDFinishOrder(int orderID, long amount, string ipAddress, string transID, string data, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intOrderID",orderID),
                                    new SqlParameter("@lngAmount",amount),
                                    new SqlParameter("@chvIPAddress",ipAddress),
                                    new SqlParameter("@chvTransID",transID),
                                    new SqlParameter("@chvTransData",data),
                                    DBHelper.GetProcedureErrorMsg()
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Pay_FinishedOrder", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);
            return returnValue == 1;
        }

        public bool CDFinishThirdPartyOrder(int orderType, int orderID, int playerID, string transID, long amount, string ipAddress, string data, int status, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intOrderType",orderType),
                                    new SqlParameter("@intOrderID",orderID),
                                    new SqlParameter("@intPlayerID",playerID),
                                    new SqlParameter("@chvTransID",transID),
                                    new SqlParameter("@lngAmount",amount),
                                    new SqlParameter("@chvIPAddress",ipAddress),
                                    new SqlParameter("@intStatus",status),
                                    new SqlParameter("@chvTransData",data),
                                    DBHelper.GetProcedureErrorMsg()
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Pay_FinishedThirdpartyOrder", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);
            return returnValue == 1;
        }



        public bool CDFailOrder(int orderID, string transID, string data)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intOrderID",orderID),
                                    new SqlParameter("@chvTransID",transID),
                                    new SqlParameter("@chvTransData",data)
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Pay_FailOrder", paras);
            return returnValue == 1;
        }

        public bool FinishedOrderShanfu(int playerId, string orderId, string teamOrderId, string chaseOrderId, int payType, int amount, int presentAmount, string status, int time, ref string localOrderID, ref string message)
        {

            int _localOrderID = 0;
            var sp_localOrderID = new SqlParameter
            {
                ParameterName = "@OrderID",
                Value = _localOrderID,
                Direction = ParameterDirection.Output
            };

            SqlParameter[] paras = {
                                    new SqlParameter("@chvStatus",status),
                                    new SqlParameter("@intPayType",payType),
                                    new SqlParameter("@intUserID",playerId),
                                    new SqlParameter("@chvOrderID",orderId),
                                    new SqlParameter("@teamOrderID",teamOrderId),
                                    new SqlParameter("@chaseOrderID",chaseOrderId),
                                    new SqlParameter("@lngAmount",amount),
                                    new SqlParameter("@lngPresentAmount",presentAmount),
                                    new SqlParameter("@intTime",time),
                                    sp_localOrderID,
                                    DBHelper.GetProcedureErrorMsg()
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Pay_FinishedOrderShanfu", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);
            localOrderID = sp_localOrderID.Value.ToString();
            return returnValue == 1;
        }

        /// <summary>
        /// 提现成功异步回调
        /// </summary>
        /// <param name="orderId">订单号</param>
        /// <param name="status">第三方状态</param>
        /// <param name="transID">第三方订单号</param>
        /// <param name="transData">第三方数据</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool WithdrawCallbackByCK(int orderId, int status, string transID, string transData, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@chvOrderId",orderId),
                                    new SqlParameter("@intTransStatus",status),
                                    new SqlParameter("@chvTransID",transID),
                                    new SqlParameter("@chvTransData",transData),
                                    DBHelper.GetProcedureErrorMsg()
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_LogDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Money_CKWithdrawCallback", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);
            return returnValue == 1;
        }

        public bool WithdrawCallbackByHema(string orderId, string status, string transID, string transData, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@chvOrderId",orderId),
                                    new SqlParameter("@intTransStatus",status),
                                    new SqlParameter("@chvTransID",(object)transID ?? DBNull.Value),
                                    new SqlParameter("@chvTransData", (object)transData ?? DBNull.Value),
                                    DBHelper.GetProcedureErrorMsg()
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_LogDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Money_HemaWithdrawCallback", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);
            return returnValue == 1;
        }

        public bool WithdrawCallback(string orderId, int status, string transID, string transData, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intOrderId",orderId),
                                    new SqlParameter("@intStatus",status),
                                    new SqlParameter("@chvTransId",(object)transID ?? DBNull.Value),
                                    new SqlParameter("@chvTransData", (object)transData ?? DBNull.Value),
                                    DBHelper.GetProcedureErrorMsg()
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_LogDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Money_WithdrawCallback", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);
            return returnValue == 1;
        }

        public PagedList<OrderListDTO> CDGetPayOrder(int? platformId, int? orderID, int? adminId, int? userID, string ipAddress, int? GWID,
            int? payTypeID, CDOrderStatus? status, int timeType, DateTime? createBeginTime, DateTime? createEndTime, int? areaID, int pageIndex, int pageSize)
        {
            var startLastUpdateTime = createBeginTime;
            var endLastUpdateTime = createEndTime;

            int pageCount = 0;
            Int64 _totalChargeAmount = 0, _totalChargeUser = 0, _totalPointAmount = 0;
            Int64 _newTotalChargeAmount = 0, _newTotalChargeUser = 0, _newTotalPointAmount = 0;

            var PageCount = new SqlParameter
            {
                ParameterName = "@intPageCount",
                Value = pageCount,
                Direction = ParameterDirection.Output
            };

            var TotalChargeAmount = new SqlParameter
            {
                ParameterName = "@intTotalChargeAmount",
                Value = _totalChargeAmount,
                Direction = ParameterDirection.Output
            };

            var TotalPointAmount = new SqlParameter
            {
                ParameterName = "@intTotalPointAmount",
                Value = _totalPointAmount,
                Direction = ParameterDirection.Output
            };
            var TotalChargeUser = new SqlParameter
            {
                ParameterName = "@intTotalChargeUser",
                Value = _totalChargeUser,
                Direction = ParameterDirection.Output
            };

            var NewTotalChargeAmount = new SqlParameter
            {
                ParameterName = "@intNewTotalChargeAmount",
                Value = _newTotalChargeAmount,
                Direction = ParameterDirection.Output
            };

            var NewTotalPointAmount = new SqlParameter
            {
                ParameterName = "@intNewTotalPointAmount",
                Value = _newTotalPointAmount,
                Direction = ParameterDirection.Output
            };

            var NewTotalChargeUser = new SqlParameter
            {
                ParameterName = "@intNewTotalChargeUser",
                Value = _newTotalChargeUser,
                Direction = ParameterDirection.Output
            };


            SqlParameter[] paras = {
                    new SqlParameter("@intAdminID",adminId),
                    new SqlParameter("@intPlatformID",platformId),
                    new SqlParameter("@intAreaID",areaID),
                    new SqlParameter("@dtStartTime", timeType == 0 ?createBeginTime : null),
                    new SqlParameter("@dtEndTime",timeType == 0 ?createEndTime : null),
                    new SqlParameter("@dtStartLastUpdateTime  ",timeType == 1 ?startLastUpdateTime : null),
                    new SqlParameter("@dtEndLastUpdateTime",timeType == 1 ?endLastUpdateTime : null),
                    new SqlParameter("@chvUserName",null),
                    new SqlParameter("@intUserID",userID),
                    new SqlParameter("@tnyStatus",status),
                    new SqlParameter("@intOrderID",orderID),
                    new SqlParameter("@chvIPAddress",ipAddress),
                    new SqlParameter("@intPayTypeID",payTypeID),
                    new SqlParameter("@intGWID",GWID),
                    new SqlParameter("@intPageIndex",pageIndex),
                    new SqlParameter("@intPageSize",pageSize),
                    PageCount,
                    TotalChargeAmount,
                    TotalChargeUser,
                    TotalPointAmount,
                    NewTotalChargeAmount,
                    NewTotalChargeUser,
                    NewTotalPointAmount,
                    };


            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<OrderListDTO>("dbo.[PrWeb_Pay_GetOrderList_Auer]", paras);

            pageCount = Convert.ToInt32(PageCount.Value);
            _totalChargeAmount = Convert.ToInt64(TotalChargeAmount.Value);
            _totalPointAmount = Convert.ToInt64(TotalPointAmount.Value);
            _totalChargeUser = Convert.ToInt64(TotalChargeUser.Value);
            _newTotalChargeAmount = Convert.ToInt64(NewTotalChargeAmount.Value);
            _newTotalPointAmount = Convert.ToInt64(NewTotalPointAmount.Value);
            _newTotalChargeUser = Convert.ToInt64(NewTotalChargeUser.Value);

            var pageList = new PagedList<OrderListDTO>(list.ToList(), pageIndex, pageSize, pageCount,
                new
                {
                    _totalChargeAmount,
                    _totalPointAmount,
                    _totalChargeUser,
                    _newTotalChargeAmount,
                    _newTotalPointAmount,
                    _newTotalChargeUser
                });


            pageList.TotalPage = Common.GetTotalPage(pageSize, pageCount);


            return pageList;

        }

        public bool CDModifyTransID(int orderID, string transID)
        {
            var order = _repoFactory.CreateRepository<CDPayOrder>().GetById(orderID);
            if (order != null)
            {
                order.TransID = transID;
                _repoFactory.CreateRepository<CDPayOrder>().Update(order);
                return true;
            }
            else
            {
                return false;
            }
        }


        public bool CreateGashPaySettleStatus(string msgType, string pcode, string cid, string coid, string cuid, string paid, string amount, string erqc, bool isSuccess)
        {
            var settleRecord = _repoFactory.CreateRepository<GashPaySettle>().Insert(
                new GashPaySettle
                {
                    MsgType = msgType,
                    PCode = pcode,
                    CId = cid,
                    COId = coid,
                    CUId = cuid,
                    PAId = paid,
                    Amount = amount,
                    ERQC = erqc,
                    IsSuccess = isSuccess,
                    LastUpdateTime = DateTime.UtcNow.AddHours(8),
                });
            if (settleRecord != null)
            {
                return true;
            }
            return false;
        }

        public List<ConvertGateway> GetConvertGateway(int operatorID)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminId", operatorID),
                                   };
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<ConvertGateway>("dbo.[PrWeb_GetConvertGateway]", paras).ToList();
            return list;
        }

        public List<PayGateway> CDGetPayType(int operatorID)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminId", operatorID),
                                   };
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<PayGateway>("dbo.[PrWeb_GetPayTypeAll]", paras).ToList();
            return list;
        }

        public List<PayGateway> CDGetUnionPlatformPayType(int operatorID, int platformID)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminId", operatorID),
                                    new SqlParameter("@intPlatformID", platformID),
                                   };
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<PayGateway>("dbo.[PrWeb_GetUnionPlatformPayType]", paras).ToList();
            return list;
        }

        public List<PayGateway> GetCompanyPayType(int operatorID)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminId", operatorID)
                                   };
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<PayGateway>("dbo.[PrWeb_GetCompanyPayType]", paras).ToList();
            return list;
        }

        public bool SetUnionPlatformPayType(int operatorID, int platformID, string param, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminId", operatorID),
                                    new SqlParameter("@intPlatformID", platformID),
                                    new SqlParameter("@chvPayTypeString", param),
                                    };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_UpdateUnionPlatformPayType", paras);

            if (returnValue == 1) message = "修改成功";
            return returnValue == 1;
        }

        public bool SetPayType(int GWID, int typeID, string amountSort, string tradeCode, bool isRange, bool status, ref string message)
        {

            var array = amountSort.Split(',').ToArray();
            var min = 0;
            var max = 0;

            if (array.Length > 0)
            {
                min = int.Parse(array[0]);
                max = int.Parse(array[array.Length - 1]);
            }

            SqlParameter[] paras = {
                                    new SqlParameter("@intGWID",GWID),
                                    new SqlParameter("@intTypeID",typeID),
                                    new SqlParameter("@chvAmountSort",amountSort),
                                    new SqlParameter("@chvTradeCode",tradeCode),
                                    new SqlParameter("@bitIsRange",isRange),
                                    new SqlParameter("@bitStatus",status),
                                    new SqlParameter("@intMin",min),
                                    new SqlParameter("@intMax",max)
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_UpdatePayTypeSetting", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);

            if (returnValue == 1) message = "修改成功";
            return returnValue == 1;

        }

        public bool SetPayGateway(int GWID, int priority, ref string message)
        {


            SqlParameter[] paras = {
                                    new SqlParameter("@intGWID",GWID),
                                    new SqlParameter("@tnyPriority",priority)
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_UpdatePayGateway", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);

            if (returnValue == 1) message = "修改成功";
            return returnValue == 1;

        }

        public List<PayGateway> CDGetCDPayType(int payTypeID)
        {
            SqlParameter[] paras = { new SqlParameter("@intPayTypeID", payTypeID) };

            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<PayGateway>("dbo.[PrWeb_GetPayGateway]", paras).ToList();
            return list;
        }

        /// <summary>
        /// 根據渠道取得可用的支付通道(SP內根據areaid決定撈取的支付通道)
        /// </summary>
        /// <param name="payTypeID"></param>
        /// <param name="areaID"></param>
        /// <returns></returns>
        public List<PayGateway> CDGetCDPayTypeByAreaID(int payTypeID, int areaID)
        {
            SqlParameter[] paras = {
                new SqlParameter("@intAreaID",areaID),
                new SqlParameter("@intPayTypeID", payTypeID) };

            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<PayGateway>("dbo.[PrWeb_GetPayGateway]", paras).ToList();
            return list;
        }

        /// <summary>
        /// 获取支付金额接口
        /// </summary>
        /// <returns></returns>
        public List<PayGateway> GetAmountList(int? platformID)
        {
            SqlParameter[] paras = {
                new SqlParameter("@intPlatformID",(object)platformID ?? DBNull.Value) };

            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<PayGateway>("dbo.[PrWeb_GetAmountList]", paras).ToList();
            return list;
        }

        /// <summary>
        /// 获取支付金额接口
        /// </summary>
        /// <returns></returns>
        public List<PayGateway> GetPayAmountList(int? platformID)
        {
            SqlParameter[] paras = {
                new SqlParameter("@intPlatformID",(object)platformID ?? DBNull.Value) };

            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<PayGateway>("dbo.[PrWeb_GetPayAmountList]", paras).ToList();
            return list;
        }

        public List<PayPointConvert> GetPayPointConverts()
        {
            var list = _repoFactory.CreateRepository<PayPointConvert>().Entities;
            var result = new List<PayPointConvert>();
            foreach (var item in list)
            {
                result.Add(new PayPointConvert {
                    Id = item.Id,
                    PayTypeID = item.PayTypeID,
                    Amount = item.Amount,
                    PointAmount = item.PointAmount,
                    Bonus = item.Bonus,
                });
            }
            return result;
        }

        public bool CDUpdateGWID(CDPayOrder order)
        {
            _repoFactory.CreateRepository<CDPayOrder>().Update(order);
            return true;

        }

        public PagedList<PlatformLog> GetPlatformLog(int operatorID, int? platformId = null, int? areaID = null, DateTime? statTime = null, DateTime? endTime = null, int? payTypeID = null, int pageIndex = 0, int pageSize = 0)
        {
            int pageCount = 0;
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminId",operatorID),
                                    new SqlParameter("@intPlatformID",platformId),
                                    new SqlParameter("@intAreaID",areaID),
                                    new SqlParameter("@dtStatTime",statTime),
                                    new SqlParameter("@dtEndTime",endTime),
                                    new SqlParameter("@intPayTypeID",payTypeID),
                                    new SqlParameter("@intPageIndex",pageIndex),
                                    new SqlParameter("@intPageSize",pageSize),
                                    new SqlParameter("@intPageCount",pageCount)
                                   };
            paras.LastOrDefault().Direction = ParameterDirection.Output;
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<PlatformLog>("dbo.PrWeb_Pay_GetOrderStatList_Auer", paras);
            pageCount = Convert.ToInt32(paras.LastOrDefault().Value);

            return new PagedList<PlatformLog>(list, pageIndex, pageSize, pageCount);

        }


        public PagedList<OrderLog> GetOrderLogByCS(int adminID, string CSName, int? userID, int? orderID, DateTime? startTime, DateTime? endTime
            , int? payTypeID, int? GWID, int pageIndex = 0, int pageSize = 0)
        {
            var _startTime = startTime == null ? null : startTime.Value.ToString("yyyy/MM/dd HH:mm:ss");
            var _endTime = endTime == null ? null : endTime.Value.ToString("yyyy/MM/dd HH:mm:ss");

            int pageCount = 0;
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminId",adminID),
                                    new SqlParameter("@chvCSName",(object)CSName??DBNull.Value),
                                    new SqlParameter("@intUserID",(object)userID??DBNull.Value),
                                    new SqlParameter("@intOrderID",(object)orderID??DBNull.Value),
                                    new SqlParameter("@dtStartTime",(object)_startTime??DBNull.Value),
                                    new SqlParameter("@dtEndTime",(object)_endTime??DBNull.Value),
                                    new SqlParameter("@intPayTypeID",(object)payTypeID??DBNull.Value),
                                    new SqlParameter("@intGWID",(object)GWID??DBNull.Value),
                                    new SqlParameter("@intPageIndex",pageIndex),
                                    new SqlParameter("@intPageSize",pageSize),
                                    new SqlParameter("@intPageCount",pageCount)
                                   };

            paras.LastOrDefault().Direction = ParameterDirection.Output;
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<OrderLog>("dbo.PrWeb_Pay_GetOrderLogByCS", paras);
            pageCount = Convert.ToInt32(paras.LastOrDefault().Value);
            return new PagedList<OrderLog>(list, pageIndex, pageSize, pageCount);

        }

        public List<OrderLog> ExportOrderLogByCS(int adminID, string CSName, int? userID, int? orderID, DateTime? startTime, DateTime? endTime
          , int? payTypeID, int? GWID)
        {
            var _startTime = startTime == null ? null : startTime.Value.ToString("yyyy/MM/dd HH:mm:ss");
            var _endTime = endTime == null ? null : endTime.Value.ToString("yyyy/MM/dd HH:mm:ss");
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminId",adminID),
                                    new SqlParameter("@chvCSName",(object)CSName??DBNull.Value),
                                    new SqlParameter("@intUserID",(object)userID??DBNull.Value),
                                    new SqlParameter("@intOrderID",(object)orderID??DBNull.Value),
                                    new SqlParameter("@dtStartTime",(object)_startTime??DBNull.Value),
                                    new SqlParameter("@dtEndTime",(object)_endTime??DBNull.Value),
                                    new SqlParameter("@intPayTypeID",(object)payTypeID??DBNull.Value),
                                    new SqlParameter("@intGWID",(object)GWID??DBNull.Value)
                                   };
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<OrderLog>("dbo.PrWeb_Pay_GetOrderLogByCS", paras).ToList();
            return list;
        }

        public bool FinishedOrderByCS(int adminID, string CSOrderID, string CSAmount, string CSMemo, string IPAddress, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminID",adminID),
                                    new SqlParameter("@intOrderID",CSOrderID),
                                    new SqlParameter("@lngAmount",CSAmount),
                                    new SqlParameter("@chvMemo",CSMemo),
                                    new SqlParameter("@chvIPAddress",IPAddress),
                                    DBHelper.GetProcedureErrorMsg()
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Pay_FinishedOrderByCS", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);
            return returnValue == 1;
        }

        public bool CreateRedirectData(RedirectData order, ref string message)
        {
            var intID = new SqlParameter
            {
                ParameterName = "@intID",
                Direction = ParameterDirection.Output,
                SqlDbType = SqlDbType.Int,
                Size = 64
            };

            var chvGUID = new SqlParameter
            {
                ParameterName = "@chvGUID",
                Direction = ParameterDirection.Output,
                SqlDbType = SqlDbType.VarChar,
                Size = 128
            };

            SqlParameter[] paras = {
                                    new SqlParameter("@intGWID",order.GWID),
                                    new SqlParameter("@chvFormData",order.FormData),
                                    intID,
                                    chvGUID,
                                    DBHelper.GetProcedureErrorMsg()
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Pay_CreateRedirectData", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);
            order.Id = intID.Value == null ? 0 : Convert.ToInt32(intID.Value);
            order.GUID = chvGUID.Value == null ? string.Empty : chvGUID.Value.ToString();

            return returnValue == 1;
        }

        public RedirectData GetRedirectData(int orderId, string gid)
        {
            SqlParameter[] paras = {
                    new SqlParameter("@intID", orderId),
                    new SqlParameter("@chvGUID", gid)
                    };

            return _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<RedirectData>("dbo.[PrWeb_Pay_GetRedirectData]", paras).FirstOrDefault();
        }

        #endregion
    }
}
