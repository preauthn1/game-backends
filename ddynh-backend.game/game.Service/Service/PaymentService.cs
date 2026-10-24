using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.DTO.ThirdPartyPay;
using game.Entity.Payment;
using game.Entity.ViewModels;
using game.Infrastructure;
using game.Service.DAO;

namespace game.Service.Service
{
    public class PaymentService : BaseService , IPaymentService
    {
        private IRepositoryFactory _repoFactory;
        private IDatabaseFactory _dbFactory;
        private PayGatewayRepository PayGatewayRepository { get; set; }

        public PaymentService(IRepositoryFactory repoFactory, IDatabaseFactory dbFactory)
        {
            _repoFactory = repoFactory;
            _dbFactory = dbFactory;
        }

        #region  主后台相关功能


        public PayGatewayEntity GetGatewayByID(int gwID)
        {
            var query = PayGatewayRepository.GetById(gwID);
            return query;
        }


        public List<PayGateway> CDGetPayGateway(int PayTypeID)
        {
            SqlParameter[] paras = {
                    new SqlParameter("@intPayTypeID", PayTypeID),
                    new SqlParameter("@chvRtnMsg", SqlDbType.VarChar, 200)
            };
            return _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<PayGateway>("dbo.[PrWeb_GetPaymentGateway]", paras).ToList();
        }

        public List<PayGateway> CDGetPayGatewayAll()
        {
            return _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<PayGateway>("dbo.[PrWeb_GetPaymentGatewayAll]").ToList();
        }

        public List<PayType> CDGetPayType(int operatorID, int? targetID)
        {
            SqlParameter[] paras = {
                new SqlParameter("@intGWID", targetID),
                new SqlParameter("@chvRtnMsg", SqlDbType.VarChar, 200)
            };
            return _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<PayType>("dbo.[PrWeb_GetPaymentType]", paras).ToList();
        }

        public PayType GetPayTypeByID(int payTypeID)
        {
            SqlParameter[] paras = {
                new SqlParameter("@intPayTypeID", payTypeID)
            };

            return _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<PayType>("dbo.[PrWeb_GetPayTypeByID]", paras).FirstOrDefault();
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
      
        public bool UpdatePayGateway(string code, string name, string cur, int info, bool supPayin, bool supPayout, bool isRange, string bankCodes)
        {
            SqlParameter[] paras = {
                new SqlParameter("@chvGatewayCode", code),
                new SqlParameter("@chvGatewayName", name),
                new SqlParameter("@intMerchantInfo", info),
                new SqlParameter("@tnySupportPayin", supPayin),
                new SqlParameter("@tnySupportPayout", supPayout),
                new SqlParameter("@tnyReviseAmount", isRange),
                new SqlParameter("@chvBankCodes", bankCodes),
                new SqlParameter("@intID", 0),
                new SqlParameter("@chvRtnMsg", SqlDbType.VarChar, 200)
            };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.[PrWeb_UpdatePaymentGateway]", paras);
            return returnValue == 1;
        }

        public bool UpdatePayType(string code, int type, string cur, string bankCode, int minAmount, int maxAmount, bool amountFixed = false, string fixedAmount = "")
        {
            SqlParameter[] paras = {
                new SqlParameter("@chvGWCode", code),
                new SqlParameter("@tnyType", type),
                new SqlParameter("@chvCurrencies", cur),
                new SqlParameter("@chvBankCode", bankCode),
                new SqlParameter("@intMinAmount", (int)minAmount),
                new SqlParameter("@intMaxAmount", (int)maxAmount),
                new SqlParameter("@tnyAmountFixed", (bool)amountFixed),
                new SqlParameter("@tnyFixedAmount", fixedAmount),
                new SqlParameter("@chvRtnMsg", SqlDbType.VarChar, 200)
            };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.[PrWeb_UpdatePaymentType]", paras);
            return returnValue == 1;
        }

        public bool UpdateMerchantInfo(int gwid, string infodata, int merchantId, int platformId)
        {
            SqlParameter[] paras = {
                new SqlParameter("@intGWID", gwid),
                new SqlParameter("@chvInfo", infodata),
                new SqlParameter("@intPlatformId", platformId),
                new SqlParameter("@intMerchantId", merchantId),
                new SqlParameter("@chvRtnMsg", SqlDbType.VarChar, 200)
            };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.[PrWeb_UpdatePaymentMerchantInfo]", paras);
            return returnValue == 1;
        }

        public bool UpdateMerchantStatus(int id, int status)
        {
            SqlParameter[] paras = {
                new SqlParameter("@intMerchantId", id),
                new SqlParameter("@intStatus", status),
                new SqlParameter("@chvRtnMsg", SqlDbType.VarChar, 200)
            };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.[PrWeb_UpdatePaymentMerchantStatus]", paras);
            return returnValue == 1;
        }

        public List<CDMerchantList> CDGetMerchantList(int gwID, int id = 0, int? platformId = null, int adminID = 0)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intGWID", gwID),
                                    new SqlParameter("@intAdminID", adminID),
                                    new SqlParameter("@intPlatformId", platformId),
                                    new SqlParameter("@intID", id),
                                    new SqlParameter("@chvRtnMsg", SqlDbType.VarChar, 200)
                                   };
            return _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<CDMerchantList>("dbo.[PrWeb_GetMerchantList]", paras).ToList();
        }

        public List<CDMerchantUUID> GetMerchantUuidList(int payTypeID, int paltfromID)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intPayTypeID", payTypeID),
                                    new SqlParameter("@intPlatformID", paltfromID),
                                    new SqlParameter("@chvRtnMsg", SqlDbType.VarChar, 200)
                                   };
            return _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<CDMerchantUUID>("dbo.[PrWeb_GetMerchantUuid]", paras).ToList();
        }

        public List<CDPayOrder> CDGetPayOrderByID(int orderID)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intOrderID", orderID),
                                    new SqlParameter("@chvRtnMsg", SqlDbType.VarChar, 200)
                                   };
            return _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<CDPayOrder>("dbo.[PrWeb_GetPayOrderByID]", paras).ToList();
        }

        #endregion

        public List<game.Entity.Pay.PayGateway> GetAmountList(int? platformID, int type = 0)
        {
            SqlParameter[] paras = 
            {
                new SqlParameter("@intPlatformID",(object)platformID ?? DBNull.Value),
            };

            //考慮到API版本相容問題 type = 1 已改用下方 GetPayTypeManagementAmountList 的function了
            //預計 2019/10月後 可以刪除這編的 type = 1 的switch case

            switch (type)
            {
                case 1:
                    return _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<game.Entity.Pay.PayGateway>("dbo.[PrWeb_GetNewPaymentAmountList]", paras).ToList();
                default:
                    return _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<game.Entity.Pay.PayGateway>("dbo.[PrWeb_GetPaymentAmountList]", paras).ToList();
            }
        }

        public List<PayTypeManagementAmountListModel> GetPayTypeManagementAmountList(int? platformID)
        {
            SqlParameter[] paras = 
            {
                new SqlParameter("@intPlatformID",(object) platformID ?? DBNull.Value),
            };

            var query = _dbFactory
                .GetDatabase(DBNameConfig.DBName_CenterDB)
                .ExecuteStoreProcedureForDataList<PayTypeManagementAmountListModel>("dbo.[PrWeb_GetPayTypeManagementAmountList]", paras)
                .ToList();

            return query;
        }


        public int GetPlatformId(int UserID)
        {
            int platformId = 0;
            SqlParameter[] paras = {
                new SqlParameter("@intUserID", UserID),
                new SqlParameter("@intPlatformId", platformId),
                new SqlParameter("@chvRtnMsg", SqlDbType.VarChar, 200)
            };
            paras[1].Direction = ParameterDirection.Output;

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.[PrWeb_GetPlatformId]", paras);
            platformId = Convert.ToInt32(paras[1].Value);
            return platformId;
        }

        public bool UpdatePayTypeDisable(int type, int id, int payTypeId, int platformId)
        {
            SqlParameter[] paras = {
                new SqlParameter("@intType", type),
                new SqlParameter("@intID", id),
                new SqlParameter("@intPayTypeID", payTypeId),
                new SqlParameter("@intPlatformID", platformId),
                new SqlParameter("@chvRtnMsg", SqlDbType.VarChar, 200)
            };

            int returnValue = 0;
            returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.[PrWeb_Payment_UpdatePlatformPayTypeStatus]", paras);
            return returnValue == 1;
        }

        public bool UpdatePlatformPayType(int id, int payTypeId, int platformId, int? minAmount, int? maxAmount, int isRange, string amountList, int scale, int? status)
        {
            SqlParameter[] paras = {
                new SqlParameter("@intPlatformPayTypeId", id),
                new SqlParameter("@intPayTypeId", payTypeId),
                new SqlParameter("@intPlatformID", platformId),
                new SqlParameter("@intMinAmount", minAmount),
                new SqlParameter("@intMaxAmount", maxAmount),
                new SqlParameter("@intIsRange", isRange),
                new SqlParameter("@chvAmountList", amountList),
                new SqlParameter("@intScale", scale),
                new SqlParameter("@intStatus", status)
            };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Payment_UpdatePlatformPayType", paras);
            return returnValue == 1;
        }

        public bool CheckPaymentOrder(int orderID, string transID)
        {
            SqlParameter[] paras = {
                new SqlParameter("@intOrderID", orderID),
                new SqlParameter("@chvTransID", transID),
                new SqlParameter("@chvRtnMsg", SqlDbType.VarChar, 200)
            };

            paras[2].Direction = System.Data.ParameterDirection.Output;
            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Payment_CheckOrder", paras);
            return returnValue == 1;
        }

        public bool InsertThirdPartyPay(InsertThirdPartyPayModel input)
        {
            SqlParameter[] paras = {
                new SqlParameter("@chvPayGatewayCode", input.chvPayGatewayCode),
                new SqlParameter("@chvPayGatewayName", input.chvPayGatewayName),
                new SqlParameter("@chvPayGatewayUrl", input.chvPayGatewayUrl),
                new SqlParameter("@chvPayGatewayBankCodes", input.chvPayGatewayBankCodes),
                new SqlParameter("@chvPayTypeBankCodes", input.chvPayTypeBankCodes)
            };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Payment_InsertThirdPartyPayGatewayPayType", paras);
            return returnValue == 1;
        }

        public List<ConvertGateway> GetConvertGateway(int operatorID, int? convertType, int? userId, int? bankCodeID = null)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminId", operatorID),
                                    new SqlParameter("@intConvertType", convertType),
                                    new SqlParameter("@intUserID", userId),
                                    new SqlParameter("@intBankCodeID", bankCodeID)
                                   };
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<ConvertGateway>("dbo.[PrWeb_GetConvertGateway]", paras).ToList();
            return list;
        }
    }
}
