using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.Agent;
using game.Entity.Stat;

namespace game.Service
{
    /// <summary>
    /// 代理管理接口
    /// </summary>
    public interface IAgentService
    {
        #region 代理用户

        /// <summary>
        /// 根据后台登录ID获取代理ID
        /// </summary>
        /// <param name="adminSysID">后台登录ID</param>
        /// <returns></returns>
        int GetAgentIDByAdminSysID(int adminSysID);

        /// <summary>
        /// 根据agentID获取代理
        /// </summary>
        /// <param name="agentID"></param>
        /// <returns></returns>
        AgentUserInfo GetAgentUserByID(int agentID);

        /// <summary>
        /// 添加代理
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool AddAgent(AgentUserInfo agent, ref string message);

        /// <summary>
        /// 修改代理信息
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ModifyAgent(AgentUserInfo agent, ref string message);

        /// <summary>
        /// 代理列表
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="isShow"></param>
        /// <param name="name"></param>
        /// <param name="wxno"></param>
        /// <param name="qq"></param>
        /// <param name="groupName"></param>
        /// <param name="loginBeginTime"></param>
        /// <param name="loginEndTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<AgentUserInfo> GetAgentList(int? platformID, int? userID, bool? isShow, string name, string wxno, string qq, string groupName, DateTime? loginBeginTime, DateTime? loginEndTime, int pageIndex, int pageSize);

        #endregion

        #region 代理申请

        /// <summary>
        /// 代理申请列表
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tel"></param>
        /// <param name="qq"></param>
        /// <param name="createBeginTime"></param>
        /// <param name="createEndTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<AgentApplyInfo> GetAgentApplyList(int? platformID, string name, string tel, string qq, DateTime? createBeginTime, DateTime? createEndTime, int pageIndex, int pageSize);

        /// <summary>
        /// 删除代理申请
        /// </summary>
        /// <param name="applyId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool DeleteAgentApply(int applyId, ref string message);

        /// <summary>
        /// 取得某一筆申請代理資料
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        AgentApplyInfo GetAgentApplyByID(int ID);

        #endregion

        #region 代理举报

        /// <summary>
        /// 代理举报列表
        /// </summary>
        /// <param name="name"></param>
        /// <param name="createBeginTime"></param>
        /// <param name="createEndTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<AgentReportLog> GetAgentReportLogList(int adminId, int? platformID, string name, DateTime? createBeginTime, DateTime? createEndTime, int pageIndex, int pageSize);

        /// <summary>
        /// 删除代理举报
        /// </summary>
        /// <param name="reportLogId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool DeleteAgentReportLog(int reportLogId, ref string message);

        #endregion

        #region 代理交易记录（转账）

        /// <summary>
        /// 代理转账
        /// </summary>
        /// <param name="agentID"></param>
        /// <param name="userID"></param>
        /// <param name="amount"></param>
        /// <param name="ip"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool AgentMoneyTransfer(int agentID, int userID, long amount, string ip, ref string message);

        /// <summary>
        /// 取得总代理资讯
        /// </summary>
        /// <param name="agentID"></param>
        /// <returns></returns>
        AgentTotal GetAgentInfoById(int operatorID, int agentID);

        /// <summary>
        /// 总代理转账
        /// </summary>
        /// <param name="id"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        bool AgentTotalTransfer(int agentID, long amount, string userID, string LoginName, ref string message);


        /// <summary>
        /// 代理交易记录列表
        /// </summary>
        /// <param name="platformID">平台ID</param>
        /// <param name="areaID">渠道ID</param>
        /// <param name="agentID">代理商ID</param>
        /// <param name="sourceID"></param>
        /// <param name="createBeginTime"></param>
        /// <param name="createEndTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<TransferLog> GetTransferLogList(int? platformID, int? areaID, int? agentID, string name, int? sourceID, DateTime? createBeginTime, DateTime? createEndTime, int pageIndex, int pageSize);


        /// <summary>
        /// 代理交易类型列表
        /// </summary>
        /// <returns></returns>
        List<AgentTransferType> GetAgentTransferTypeList();

        /// <summary>
        /// 查詢總代理轉帳紀錄
        /// </summary>
        /// <param name="agentid"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<AgentTransferInfo> GetTransferInfo(int operatorID, int? agentid = null, DateTime? beginTime = null, DateTime? endTime = null, int pageIndex = 1, int pageSize = 20);
        #endregion

        #region 代理交易日报表

        /// <summary>
        /// 代理交易记录日报表
        /// </summary>
        /// <param name="platformID">平台ID</param>
        /// <param name="areaID">渠道ID</param>
        /// <param name="agentID">代理商ID</param>
        /// <param name="sourceID"></param>
        /// <param name="statBeginTime"></param>
        /// <param name="statEndTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<TransferSourceLog> GetTransferSourceLogList(int? platformID, int? areaID, int? agentID, string name, int? sourceID, DateTime? statBeginTime, DateTime? statEndTime, int pageIndex, int pageSize);


        #endregion
    }
}
