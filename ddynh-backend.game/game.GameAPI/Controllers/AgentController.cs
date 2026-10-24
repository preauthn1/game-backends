using System;
using System.Collections.Generic;
using System.Web.Http;
using FW.Core.Helper;
using game.Entity.Agent;
using game.Entity.Stat;
using game.Infrastructure;
using game.Service;

namespace game.GameAPI.Controllers
{
    public class AgentController : BaseAPIController
    {
        private IAgentService _iAgentService;

        public AgentController(IAgentService iAgentService)
        {
            _iAgentService = iAgentService;
        }

        #region 代理用户

        /// <summary>
        /// 根据后台登录ID获取代理ID
        /// </summary>
        /// <param name="adminSysID">后台登录ID</param>
        /// <returns></returns>
        [HttpGet]
        public int GetAgentIDByAdminSysID(int adminSysID)
        {
            return _iAgentService.GetAgentIDByAdminSysID(adminSysID);
        }

        /// <summary>
        /// 根据agentID获取代理
        /// </summary>
        /// <param name="agentID"></param>
        /// <returns></returns>
        [HttpGet]
        public AgentUserInfo GetAgentUserByID(int agentID)
        {
            return _iAgentService.GetAgentUserByID(agentID);
        }

        /// <summary>
        /// 添加代理
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        [HttpPost]
        public APIResult AddAgent(AgentUserInfo agent)
        {
            string message = "";
            bool result = _iAgentService.AddAgent(agent, ref message);
            return new APIResult(result, message, agent.Id);
        }

        /// <summary>
        /// 修改代理信息
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        [HttpPost]
        public APIResult ModifyAgent(AgentUserInfo agent)
        {
            string message = "";
            bool result = _iAgentService.ModifyAgent(agent, ref message);
            return new APIResult(result, message);
        }

        /// <summary>
        /// 代理列表
        /// </summary>
        /// <param name="platformID"></param>
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
        [HttpGet]
        public PagedList<AgentUserInfo> GetAgentList(int? platformID, int? userID, bool? isShow, string name, string wxno, string qq, string groupName, DateTime? loginBeginTime, DateTime? loginEndTime, int pageIndex = 1, int pageSize = 20)
        {
            return _iAgentService.GetAgentList(platformID, userID, isShow, name, wxno, qq, groupName, loginBeginTime, loginEndTime, pageIndex, pageSize);
        }

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
        [HttpGet]
        public PagedList<AgentApplyInfo> GetAgentApplyList(int? platformID, string name, string tel, string qq, DateTime? createBeginTime, DateTime? createEndTime, int pageIndex = 1, int pageSize = 20)
        {
            return _iAgentService.GetAgentApplyList(platformID, name, tel, qq, createBeginTime, createEndTime, pageIndex, pageSize);
        }

        /// <summary>
        /// 删除代理申请
        /// </summary>
        /// <param name="applyId"></param>
        /// <returns></returns>
        [HttpGet]
        public APIResult DeleteAgentApply(int applyId)
        {
            string message = "";
            bool result = _iAgentService.DeleteAgentApply(applyId, ref message);
            return new APIResult(result, message);
        }

        public AgentApplyInfo GetAgentApplyByID(int ID = 0)
        {
            if (ID == 0) return null;
            return _iAgentService.GetAgentApplyByID(ID);
        }

        #endregion

        #region 代理举报

        /// <summary>
        /// 代理申请列表
        /// </summary>
        /// <param name="name"></param>
        /// <param name="createBeginTime"></param>
        /// <param name="createEndTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public PagedList<AgentReportLog> GetAgentReportLogList(int adminId, int? platformID, string name, DateTime? createBeginTime, DateTime? createEndTime, int pageIndex = 1, int pageSize = 20)
        {
            return _iAgentService.GetAgentReportLogList(adminId, platformID, name, createBeginTime, createEndTime, pageIndex, pageSize);
        }

        /// <summary>
        /// 删除代理申请
        /// </summary>
        /// <param name="reportLogId"></param>
        /// <returns></returns>
        [HttpGet]
        public APIResult DeleteAgentReportLog(int reportLogId)
        {
            string message = "";
            bool result = _iAgentService.DeleteAgentReportLog(reportLogId, ref message);
            return new APIResult(result, message);
        }

        #endregion

        #region 代理交易记录（转账）

        /// <summary>
        /// 代理转账
        /// </summary>
        /// <param name="agentID"></param>
        /// <param name="userID"></param>
        /// <param name="amount"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        [HttpGet]
        public APIResult AgentMoneyTransfer(int agentID, int userID, long amount, string ip)
        {
            string message = "";
            bool result = _iAgentService.AgentMoneyTransfer(agentID, userID, amount, ip, ref message);
            return new APIResult(result, message);
        }


        /// <summary>
        /// 取得总代理资讯
        /// </summary>
        /// <param name="agentID"></param>
        /// <returns></returns>
        [HttpGet]
        public AgentTotal GetAgentInfoById(int operatorID, int agentID)
        {
            return _iAgentService.GetAgentInfoById(operatorID, agentID);
        }

        /// <summary>
        /// 总代理转账
        /// </summary>
        /// <param name="id"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        [HttpGet]
        public APIResult AgentTotalTransfer(int agentID, long amount, string userID, string LoginName)
        {
            string message = "";
            bool result = _iAgentService.AgentTotalTransfer(agentID, amount, userID, LoginName, ref message);
            return new APIResult(result, message);
        }

        /// <summary>
        /// 代理交易记录列表
        /// </summary>
		/// <param name="platformID"></param>
		/// <param name="areaID"></param>
        /// <param name="agentID">代理商ID</param>
        /// <param name="name"></param>
        /// <param name="sourceID"></param>
        /// <param name="createBeginTime"></param>
        /// <param name="createEndTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public PagedList<TransferLog> GetTransferLogList(int? platformID, int? areaID, int? agentID, string name, int? sourceID, DateTime? createBeginTime, DateTime? createEndTime, int pageIndex = 1, int pageSize = 20)
        {
            return _iAgentService.GetTransferLogList(platformID, areaID, agentID, name, sourceID, createBeginTime, createEndTime, pageIndex, pageSize);
        }

        /// <summary>
        /// 代理交易类型列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<AgentTransferType> GetAgentTransferTypeList()
        {
            return _iAgentService.GetAgentTransferTypeList();
        }

        /// <summary>
        /// 查詢總代理轉帳紀錄
        /// </summary>
        /// <param name="agentid"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public PagedList<AgentTransferInfo> GetTransferInfo(int operatorID, int? agentid = null, DateTime? beginTime = null, DateTime? endTime = null, int pageIndex = 1, int pageSize = 20)
        {
            return _iAgentService.GetTransferInfo(operatorID, agentid, beginTime, endTime, pageIndex, pageSize);
        }

        #endregion

        #region 代理交易日报表

        /// <summary>
        /// 代理交易记录日报表
        /// </summary>
        /// <param name="platformID"></param>
		/// <param name="areaID"></param>
        /// <param name="agentID">代理商ID</param>
        /// <param name="name"></param>
        /// <param name="sourceID"></param>
        /// <param name="statBeginTime"></param>
        /// <param name="statEndTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public PagedList<TransferSourceLog> GetTransferSourceLogList(int? platformID, int? areaID, int? agentID, string name, int? sourceID, DateTime? statBeginTime, DateTime? statEndTime, int pageIndex = 1, int pageSize = 20)
        {
            return _iAgentService.GetTransferSourceLogList(platformID, areaID, agentID, name, sourceID, statBeginTime, statEndTime, pageIndex, pageSize);
        }

        #endregion
    }
}