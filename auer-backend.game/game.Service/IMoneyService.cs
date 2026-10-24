using System;
using System.Collections.Generic;
using FW.Core.Helper;
using FW.Core.Helper.Enum;
using game.Entity.Money;

namespace game.Service
{
    /// <summary>
    /// 金币业务
    /// </summary>
    public interface IMoneyService
    {
        /// <summary>
        /// 获取用户现金
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        Cash GetUserCash(int userID);

        /// <summary>
        /// 修改用户现金
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="amount"></param>
        /// <param name="sourceType"></param>
        /// <param name="ipAddress"></param>
        /// <param name="memo"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool UpdateUserCash(int userID, Int64 amount, Int16 sourceType, string ipAddress, string memo, ref long currentAmount, ref string message);

        /// <summary>
        /// 获取金币记录
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="sourceType"></param>
        /// <param name="ipAddress"></param>
        /// <param name="memo"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<MoneyLogReport> GetMoneyLog(int? userID, int? sourceTypeGroup, Int16? sourceType, string ipAddress, string memo, DateTime? beginTime, DateTime? endTime, int? areaID, int? platformId, int pageIndex, int pageSize);

        /// <summary>
        /// 获取现金流通渠道
        /// </summary>
        /// <returns></returns>
        List<MoneySourceType> GetMoneySourceType();

        /// <summary>
        /// 获取现金流通渠道 分组
        /// </summary>
        /// <returns></returns>
        List<MoneySourceGroup> GetMoneySourceGroup();

        #region 银行相关
        /// <summary>
        /// 获取用户银行
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        Bank GetUserBank(int userID);

        /// <summary>
        /// 修改用户银行
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="amount"></param>
        /// <param name="sourceType"></param>
        /// <param name="ipAddress"></param>
        /// <param name="memo"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool UpdateUserBank(int adminID, int userID, Int64 amount, Int16 sourceType, string ipAddress, string memo, ref long currentAmount, ref string message);

        /// <summary>
        /// 存取款
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="opType"></param>
        /// <param name="amount"></param>
        /// <param name="sourceType"></param>
        /// <param name="ipAddress"></param>
        /// <param name="memo"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool Deposit(int userID, BankOPType opType, Int64 amount, string ipAddress, string memo, ref string message);

        /// <summary>
        /// 获取银行记录
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="sourceType"></param>
        /// <param name="ipAddress"></param>
        /// <param name="memo"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<BankLog> GetBankLog(int operatorID, int? userID, Int16? sourceType, string ipAddress, string memo, DateTime? beginTime, DateTime? endTime, int pageIndex, int pageSize);

        /// <summary>
        /// 获取银行流通渠道
        /// </summary>
        /// <returns></returns>
        List<BankSourceType> GetBankSourceType();
        #endregion

        #region 兑换记录

        /// <summary>
        /// 获取指定状态兑换记录数(只取往后推24个小时的数据)
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        int GetConvertLogCountByStatus(string platformIDs, ConvertLogStatus status);

        /// <summary>
        /// 自動兌換 - 拉取兑换记录
        /// </summary>
        /// <param name="count">拉取条数</param>
        /// <returns></returns>
        List<PullConvertLog> PullConvertLog(string flag, string number, int? gwid);

        //聯運專用
        List<PullConvertLog> UnionPullConvertLog(string flag, string number);




        /// <summary>
        /// 自動兌換 - 取得狀態
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        int GetPullConvertLogSatusById(int id);

        ConvertLog GetConvertLogById(int id);

        /// <summary>
        /// 获取兑换记录
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="adminId"></param>
        /// <param name="status"></param>
        /// <param name="convertType"></param>
        /// <param name="realName"></param>
        /// <param name="ipAddress"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<ConvertLog> GetConvertLog(int? platformId, int? adminId, int? userID, ConvertLogStatus? status, int? convertType, string realName, string ipAddress, DateTime? beginTime, DateTime? endTime, string operatorName, int? gwid, long? minAmount, long? maxAmount, int pageIndex, int pageSize);

        /// <summary>
        /// 更新兑换记录
        /// </summary>
        /// <param name="message"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        bool ModifyConvertLogStatus(int id, ConvertLogStatus status, string updateUser, string memo, int? gwid, ref string message);

        /// <summary>
        /// 修改兑换备注
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateUser"></param>
        /// <param name="memo"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ModifyConvertLogMemo(int id, string updateUser, string memo, ref string message);

        /// <summary>
        /// 结束处理兑换记录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="realTaxAmount"></param>
        /// <param name="transId"></param>
        /// <param name="transData"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        int FinishedConvertLog(int id, byte status, long realTaxAmount, string transId, string transData, string updateUser, ref string message);


        #endregion


        #region 可疑帳戶

        List<ConvertSuspicious> GetSuspicious(string name, string accountID);

        bool AddSuspicious(ConvertSuspicious model, ref string message);

        bool EditSuspicious(int id, string name, string accountID, string action, ref string message);

        #endregion
    }
    /// <summary>
    /// 银行操作类型
    /// </summary>
    public enum BankOPType : byte
    {
        [EnumDescription("存款")]
        Deposit = 0,
        [EnumDescription("取款")]
        Withdraw = 1
    }
}
