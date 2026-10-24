using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Data;
using FW.Core.Repository;
using game.Entity.Money;
using game.Infrastructure;
using FW.Core.Helper;

namespace game.Service
{
    public class MoneyService : IMoneyService
    {
        private IRepositoryFactory _repoFactory;
        private IDatabaseFactory _dbFactory;
        public MoneyService(IRepositoryFactory repoFactory, IDatabaseFactory dbFactory)
        {
            _repoFactory = repoFactory;
            _dbFactory = dbFactory;
        }


        public Cash GetUserCash(int userID)
        {
            return _repoFactory.CreateRepository<Cash>().GetById(userID);
        }

        public bool UpdateUserCash(int userID, Int64 amount, Int16 sourceType, string ipAddress, string memo, ref long currentAmount, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intUserID",userID),
                                    new SqlParameter("@sintSourceType",sourceType),
                                    new SqlParameter("@lngUpdateAmount",amount),
                                    new SqlParameter("@chvIPAddress",ipAddress),
                                    new SqlParameter("@chvMemo",memo),
                                    new SqlParameter("@lngCurrentAmount",currentAmount),
                                    DBHelper.GetProcedureErrorMsg()
                                   };
            paras[5].Direction = ParameterDirection.Output;

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_UpdateUserCash", paras);
            currentAmount = paras[5].Value == null ? 0 : Convert.ToInt64(paras[5].Value);
            message = Convert.ToString(paras.LastOrDefault().Value);
            return returnValue == 1;
        }

        public PagedList<MoneyLogReport> GetMoneyLog(int? userID, int? sourceTypeGroup, short? sourceType, string ipAddress, string memo, DateTime? beginTime, DateTime? endTime, int? areaID, int? platformId, int pageIndex, int pageSize)
        {
            int pageCount = 0;
            SqlParameter[] paras = {
                                    new SqlParameter("@intUserID",userID),
                                    new SqlParameter("@intSourceTypeGroup",sourceTypeGroup),
                                    new SqlParameter("@intSourceType",sourceType),
                                    new SqlParameter("@chvIPAddress",ipAddress),
                                    new SqlParameter("@chvMemo",memo),
                                    new SqlParameter("@beginTime",beginTime),
                                    new SqlParameter("@endTime",endTime),
                                    new SqlParameter("@intAreaID",areaID),
                                    new SqlParameter("@intPlatformID",platformId),
                                    new SqlParameter("@intPageIndex",pageIndex),
                                    new SqlParameter("@intPageSize",pageSize),
                                    new SqlParameter("@intPageCount",pageCount),
                                   };
            paras.LastOrDefault().Direction = ParameterDirection.Output;
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<MoneyLogReport>("dbo.[PrWeb_Stat_GetMoneyLog]", paras).ToList();
            pageCount = Convert.ToInt32(paras.LastOrDefault().Value);

            var pageList = new PagedList<MoneyLogReport>();
            pageList.PageData = list;
            pageList.PageIndex = pageIndex;
            pageList.PageSize = pageSize;
            pageList.TotalCount = Convert.ToInt32(pageCount);
            pageList.TotalPage = Common.GetTotalPage(pageSize, Convert.ToInt32(pageCount));

            return pageList;
        }



        public List<MoneySourceType> GetMoneySourceType()
        {
            return _repoFactory.CreateRepository<MoneySourceType>().Entities.ToList();
        }


        public List<MoneySourceGroup> GetMoneySourceGroup()
        {
            return _repoFactory.CreateRepository<MoneySourceGroup>().Entities.OrderBy(m => m.Sort).ToList();
        }


        #region 银行相关

        public Bank GetUserBank(int userID)
        {
            return _repoFactory.CreateRepository<Bank>().GetById(userID);
        }

        public bool UpdateUserBank(int adminID, int userID, long amount, short sourceType, string ipAddress, string memo, ref long currentAmount, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminID",adminID),
                                    new SqlParameter("@intUserID",userID),
                                    new SqlParameter("@sintSourceType",sourceType),
                                    new SqlParameter("@lngUpdateAmount",amount),
                                    new SqlParameter("@chvIPAddress",ipAddress),
                                    new SqlParameter("@chvMemo",memo),
                                    new SqlParameter("@lngCurrentAmount",currentAmount),
                                    DBHelper.GetProcedureErrorMsg()
                                   };
            paras[6].Direction = ParameterDirection.Output;

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_UpdateUserBank", paras);
            currentAmount = paras[6].Value is DBNull || paras[6].Value == null ? 0 : Convert.ToInt32(paras[6].Value);
            message = Convert.ToString(paras.LastOrDefault().Value);
            return returnValue == 1;
        }

        public bool Deposit(int userID, BankOPType opType, long amount, string ipAddress, string memo, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intUserID",userID),
                                    new SqlParameter("@tintOpType",opType),
                                    new SqlParameter("@lngAmount",amount),
                                    new SqlParameter("@chvIPAddress",ipAddress),
                                    new SqlParameter("@chvMemo",memo),
                                    DBHelper.GetProcedureErrorMsg()
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrPs_Money_Deposit", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);
            return returnValue == 1;
        }

        public PagedList<BankLog> GetBankLog(int operatorID, int? userID, short? sourceType, string ipAddress, string memo, DateTime? beginTime, DateTime? endTime, int pageIndex, int pageSize)
        {
            int pageCount = 0;
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminID ",operatorID),
                                    new SqlParameter("@intUserID",userID),
                                    new SqlParameter("@intSourceType",sourceType),
                                    new SqlParameter("@chvIPAddress",ipAddress),
                                    new SqlParameter("@beginTime",beginTime),
                                    new SqlParameter("@endTime",endTime),
                                    new SqlParameter("@intPageIndex",pageIndex),
                                    new SqlParameter("@intPageSize",pageSize),
                                    new SqlParameter("@intPageCount",pageCount),
                                   };
            paras.LastOrDefault().Direction = ParameterDirection.Output;
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<BankLog>("dbo.PrWeb_Stat_GetBankLog", paras);
            pageCount = Convert.ToInt32(paras.LastOrDefault().Value);

            return new PagedList<BankLog>(list, pageIndex, pageSize, pageCount);
        }

        public List<BankSourceType> GetBankSourceType()
        {
            return _repoFactory.CreateRepository<BankSourceType>().Entities.ToList();
        }

        #endregion

        #region 兑换记录

        /// <summary>
        /// 获取指定状态兑换记录数(只取往后推24个小时的数据)
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public int GetConvertLogCountByStatus(string platformIDs, ConvertLogStatus status)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@chvPlatformIDs",platformIDs),
                                    new SqlParameter("@tnyStatus",status),
                                    new SqlParameter("@intCount",0)
                                   };
            paras.LastOrDefault().Direction = ParameterDirection.Output;
            _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Money_ConvertCountByStatus", paras);
            return Convert.ToInt32(paras.LastOrDefault().Value);
        }


        public ConvertLog GetConvertLogById(int id)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intID ",id),
                                    new SqlParameter("@intRtnCode",0),
                                   };
            return _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<ConvertLog>("dbo.PrPs_GetConvertLogById", paras).FirstOrDefault();
        }

        public PagedList<ConvertLog> GetConvertLog(int? platformId, int? adminId, int? userID, ConvertLogStatus? status, int? convertType, string realName, string ipAddress, DateTime? beginTime, DateTime? endTime, string operatorName, int? gwid, long? minAmount, long? maxAmount, int pageIndex, int pageSize)
        {
            int pageCount = 0;
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminID",adminId),
                                    new SqlParameter("@intPlatformID",platformId),
                                    new SqlParameter("@intUserID",userID),
                                    new SqlParameter("@tnyStatus",status),
                                    new SqlParameter("@tnyConvertType",convertType),
                                    new SqlParameter("@chvRealName",realName),
                                    new SqlParameter("@chvIPAddress",ipAddress),
                                    new SqlParameter("@dtbeginTime",beginTime),
                                    new SqlParameter("@dtendTime",endTime),
                                    new SqlParameter("@chvOperator",operatorName),
                                    new SqlParameter("@lngMinAmount",minAmount),
                                    new SqlParameter("@lngMaxAmount",maxAmount),
                                    new SqlParameter("@intGWID",gwid),
                                    new SqlParameter("@intPageIndex",pageIndex),
                                    new SqlParameter("@intPageSize",pageSize),
                                    new SqlParameter("@intPageCount",pageCount),
                                   };
            paras.LastOrDefault().Direction = ParameterDirection.Output;
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<ConvertLog>("dbo.PrWeb_Money_GetConvertLog", paras);
            pageCount = Convert.ToInt32(paras.LastOrDefault().Value);

            return new PagedList<ConvertLog>(list, pageIndex, pageSize, pageCount);
        }


        /// <summary>
        /// 更新兑换记录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="updateUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool ModifyConvertLogStatus(int id, ConvertLogStatus status, string updateUser, string memo, int? gwid, ref string message)
        {
            var log = GetConvertLogById(id);
            if (log == null)
            {
                message = "记录不存在";
                return false;
            }

            ModifyConvertLogStatus(id, status, updateUser, memo, gwid);
            message = "操作成功";
            return true;
        }

        public bool ModifyConvertLogMemo(int id, string updateUser, string memo, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intId",id),                                
                                    new SqlParameter("@chvUserName",updateUser),
                                    new SqlParameter("@chvMemo",(object)memo ?? DBNull.Value),
                                    DBHelper.GetProcedureErrorMsg()
                                   };
            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_LogDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Money_UpdateConvertLogMemo", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);
            return returnValue == 1 ? true : false;
        }

        private bool ModifyConvertLogStatus(int id, ConvertLogStatus status, string updateUser, string memo, int? gwid)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intId",id),
                                    new SqlParameter("@tnyStatus",status),
                                    new SqlParameter("@chvUserName",updateUser),
                                    new SqlParameter("@chvMemo",(object)memo ?? DBNull.Value),
                                    new SqlParameter("@intGWID",(object)gwid ?? DBNull.Value)
                                   };
            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Money_UpdateConvertLogStatus", paras);
            return returnValue == 1 ? true : false;
        }

        public int FinishedConvertLog(int id, byte status, long realTaxAmount, string transId, string transData, string updateUser, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intId",id),
                                    new SqlParameter("@tnyStatus",status),
                                    new SqlParameter("@lngRealTaxAmount",realTaxAmount),
                                    new SqlParameter("@chvTransID",transId),
                                    new SqlParameter("@chvTransData",transData),
                                    new SqlParameter("@chvUserName",updateUser),
                                    DBHelper.GetProcedureErrorMsg()
                                   };
            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Money_FinishedConvertLog", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);
            return returnValue;
        }

        #endregion

        #region 自動兌換



        /// <summary>
        /// 自動兌換 - 更新狀態
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        //public int ModifyPullConvertLogStatus(int id, ConvertLogStatus status, ref string message)
        //{
        //    //檢查狀態
        //    var log = GetConvertLogById(id);
        //    if (log == null)
        //    {
        //        message = "记录不存在";
        //        return -4;
        //    }

        //    //變更為已打款
        //    if (status == ConvertLogStatus.AlreadyPaid)
        //    {
        //        if (log.Status != ConvertLogStatus.ConvertSuccessed)
        //        {
        //            message = "审核未通过，不能修改成请求的状态";
        //            return -3;
        //        }
        //    }

        //    //更新狀態
        //    bool result = ModifyConvertLogStatus(id, status, "system");

        //    message = "更新完成";
        //    return 0;
        //}

        /// <summary>
        /// 自動兌換 - 取得兌換資料
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<PullConvertLog> PullConvertLog(string flag, string number, int? gwid)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@tnyFlag",(object)flag ?? DBNull.Value),
                                    new SqlParameter("@chvNumber",(object)number ?? DBNull.Value),                                
                                    new SqlParameter("@intGWID",(object)gwid ?? DBNull.Value),
                                    new SqlParameter("@chvUserName","system"),
                                   };
            return _dbFactory.GetDatabase(DBNameConfig.DBName_LogDB).ExecuteStoreProcedureForDataList<PullConvertLog>("dbo.PrWeb_Money_PullConvertLog", paras).ToList();
        }

        /// <summary>
        /// 聯運自動兌換 - 取得兌換資料
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<PullConvertLog> UnionPullConvertLog(string flag, string number)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@tnyFlag",flag),
                                    new SqlParameter("@chvNumber",number),
                                    new SqlParameter("@chvUserName","system")
                                   };
            return _dbFactory.GetDatabase(DBNameConfig.DBName_LogDB).ExecuteStoreProcedureForDataList<PullConvertLog>("dbo.PrWeb_Money_UnionPullConvertLog", paras).ToList();
        }

        /// <summary>
        /// 自動兌換 - 取得狀態
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetPullConvertLogSatusById(int id)
        {
            SqlParameter[] paras = {
                new SqlParameter("@intID ", id),
                new SqlParameter("@intStatus",0)
            };
            paras.LastOrDefault().Direction = ParameterDirection.Output;
            _dbFactory.GetDatabase(DBNameConfig.DBName_LogDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Money_PullConvertLogGetStatus", paras);
            return Convert.ToInt32(paras.LastOrDefault().Value);
        }


        #endregion

        #region 可疑帳戶


        public List<ConvertSuspicious> GetSuspicious(string name, string accountID)
        {
            SqlParameter[] paras = {
                    new SqlParameter("@chvName",name),
                    new SqlParameter("@chvAccountID",accountID)
                    };

            var _list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<ConvertSuspicious>("dbo.[PrWeb_Convert_GetSuspicious]", paras).ToList();
            return _list;
        }

        public bool AddSuspicious(ConvertSuspicious model, ref string message)
        {
            SqlParameter[] paras = {
                    new SqlParameter("@chvName",model.Name),
                    new SqlParameter("@chvAccountID",model.AccountID),
                    DBHelper.GetProcedureErrorMsg()
                    };

            int _returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Convert_AddSuspicious", paras);

            message = Convert.ToString(paras.LastOrDefault().Value);
            return _returnValue == 1;
        }

        public bool EditSuspicious(int id, string name, string accountID, string action, ref string message)
        {
            SqlParameter[] paras = {
                    new SqlParameter("@ID",id),
                    new SqlParameter("@chvAction",action),
                    new SqlParameter("@chvName",name),
                    new SqlParameter("@chvAccountID",accountID),
                    DBHelper.GetProcedureErrorMsg()
                    };

            int _returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Convert_EditSuspicious", paras);

            message = Convert.ToString(paras.LastOrDefault().Value);
            return _returnValue == 1;
        }

        #endregion

    }
}
