using System;
using System.Collections.Generic;
using System.Web.Http;
using game.Service;
using game.Entity.Money;
using game.Infrastructure;
using FW.Core.Helper;
using System.Configuration;
using example.invalid;
using game.Entity;

namespace game.GameAPI.Controllers
{
    public class MoneyController : BaseAPIController
    {
        private IMoneyService _moneyService;
        public MoneyController(IMoneyService moneyService)
        {
            _moneyService = moneyService;
        }

        [HttpGet]
        public Cash GetUserCash(int userID)
        {
            return _moneyService.GetUserCash(userID);
        }

        [HttpGet]
        public APIResult UpdateUserCash(int userID, Int64 amount, Int16 sourceType, string ipAddress, string memo)
        {
            string message = "";
            long currentAmount = 0;
            bool result = _moneyService.UpdateUserCash(userID, amount, sourceType, ipAddress, memo, ref currentAmount, ref message);
            return new APIResult(result, message, currentAmount);
        }

        [HttpGet]
        public APIResult OneClear(int userID, string ipAddress, string memo)
        {
            string message = "";
            bool result = _moneyService.OneClear(userID, ipAddress, memo, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public MoenyLogAddSumFlowAmountReport GetMoneyLog([FromUri]MoneyLogQueryFilterModel filter)
        {
            return _moneyService.GetMoneyLog(filter);
        }

        [HttpGet]
        public List<MoneySourceType> GetMoneySourceType(int? adminID)
        {
            return _moneyService.GetMoneySourceType(adminID);
        }

        [HttpGet]
        public List<MoneySourceGroup> GetMoneySourceGroup()
        {
            return _moneyService.GetMoneySourceGroup();
        }


        [HttpGet]
        public Bank GetUserBank(int userID)
        {
            return _moneyService.GetUserBank(userID);
        }

        [HttpGet]
        public APIResult UpdateUserBank(int adminID, int userID, Int64 amount, Int16 sourceType, string ipAddress, string memo)
        {
            string message = "";
            long currentAmount = 0;
            bool result = _moneyService.UpdateUserBank(adminID, userID, amount, sourceType, ipAddress, memo, ref currentAmount, ref message);
            return new APIResult(result, message, currentAmount);
        }

        public APIResult Deposit(int userID, BankOPType opType, Int64 amount, string ipAddress, string memo)
        {
            string message = "";
            bool result = _moneyService.Deposit(userID, opType, amount, ipAddress, memo, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public PagedList<BankLog> GetBankLog(int operatorID, int? userID = null, Int16? sourceType = null, string ipAddress = null, string memo = null, DateTime? beginTime = null, DateTime? endTime = null, int pageIndex = 1, int pageSize = 20)
        {
            return _moneyService.GetBankLog(operatorID, userID, sourceType, ipAddress, memo, beginTime, endTime, pageIndex, pageSize);
        }

        [HttpGet]
        public List<BankLogSourceType> GetBankLogGroupBySourceType(int operatorID, int UserID)
        {
            return _moneyService.GetBankLogGroupBySourceType(operatorID, UserID);
        }

        [HttpGet]
        public List<BankSourceType> GetBankSourceType()
        {
            return _moneyService.GetBankSourceType();
        }

        #region 兑换记录

        [HttpGet]
        public int GetConvertLogCountByStatus(string platformIDs, ConvertLogStatus status)
        {
            return _moneyService.GetConvertLogCountByStatus(platformIDs, status);
        }

        [HttpGet]
        public List<PullConvertLog> PullConvertLog(string flag, string number, int? gwid)
        {
            return _moneyService.PullConvertLog(flag, number, gwid);
        }

        //聯運專用
        [HttpGet]
        public List<PullConvertLog> UnionPullConvertLog(string flag, string number)
        {
            return _moneyService.UnionPullConvertLog(flag, number);
        }

        [HttpGet]
        public int GetPullConvertLogSatusById(int id)
        {
            return _moneyService.GetPullConvertLogSatusById(id);
        }


        [HttpGet]
        public ConvertLog GetConvertLogById(int id)
        {
            return _moneyService.GetConvertLogById(id);
        }

        [HttpGet]
        public PagedList<ConvertLog> GetConvertLog(int? platformId, int? adminId, int? userID = null, ConvertLogStatus? status = null, int? convertType = null, string realName = null, string ipAddress = null, DateTime? beginTime = null, DateTime? endTime = null, string operatorName = null, int? gwid = null, decimal? minAmount = null, decimal? maxAmount = null, int pageIndex = 1, int pageSize = 20)
        {
            return _moneyService.GetConvertLog(platformId, adminId, userID, status, convertType, realName, ipAddress, beginTime, endTime, operatorName, gwid, minAmount, maxAmount, pageIndex, pageSize);
        }

        [HttpGet]
        public APIResult ModifyConvertLogStatusPassed(int id, ConvertLogStatus status, string updateUser, string memo, int? gwid)
        {
            string message = string.Empty;
            var result = _moneyService.ModifyConvertLogStatus(id, status, updateUser, memo, gwid, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult ModifyConvertLogStatusRefused(int id, ConvertLogStatus status, string updateUser, string memo, int? gwid)
        {
            string message = string.Empty;
            var result = _moneyService.ModifyConvertLogStatus(id, status, updateUser, memo, gwid, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult ModifyConvertLogStatusMemo(int id, string updateUser, string memo)
        {
            string message = string.Empty;
            var result = _moneyService.ModifyConvertLogMemo(id, updateUser, memo, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult2 FinishedConvertLog(int id, byte status, long realTaxAmount, string transId, string updateUser, string transData)
        {
            string message = string.Empty;
            var resultCode = _moneyService.FinishedConvertLog(id, status, realTaxAmount, transId, transData, updateUser, ref message);
            bool result = resultCode == 1 ? true : false;
            return new APIResult2(result, resultCode, message);
        }

        #endregion

        #region 可疑帳戶

        [HttpGet]
        public List<ConvertSuspicious> GetSuspicious(string name, string accountID)
        {
            return _moneyService.GetSuspicious(name, accountID);
        }

        [HttpPost]
        public APIResult AddSuspicious(ConvertSuspicious model)
        {
            string message = string.Empty;
            var _result = _moneyService.AddSuspicious(model, ref message);
            return new APIResult(_result, message);
        }

        [HttpGet]
        public APIResult EditSuspicious(int id, string name, string accountID, string action)
        {
            string message = string.Empty;
            var _result = _moneyService.EditSuspicious(id, name, accountID, action, ref message);
            return new APIResult(_result, message);
        }

        #endregion

        #region  流水限制Log
        [HttpGet]
        public APIResult CreateFlowAmountLimitLog(int adminID, int userID, int auditType, int auditAmount, int sourceType, Int64 amount)
        {
            string message = "";
            bool result = _moneyService.CreateFlowAmountLimitLog(adminID, userID, auditType, auditAmount, sourceType, amount, ref message);
            return new APIResult(result, message);
        }
        #endregion
    }
}