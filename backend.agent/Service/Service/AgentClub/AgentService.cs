using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Repository;
using Repository.Entity.agentclub;
using Repository.ViewModel;

namespace Service.Service.AgentClub
{
    public class AgentService
    {
        private AgentClubFactory _repoFactory = new AgentClubFactory();

        public AgentUsers GetAgentUser(string username)
        {
            try
            {
                var result = _repoFactory.CreateRepository<AgentUsers>().GetAll().Where(c => username == c.LoginName).FirstOrDefault();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public AgentUsers GetAgentUser(Guid id)
        {
            try
            {
                var result = _repoFactory.CreateRepository<AgentUsers>().GetAll().Where(c => id == c.ID).FirstOrDefault();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<AgentUsers> GetAgentUserAll()
        {
            try
            {
                var result = _repoFactory.CreateRepository<AgentUsers>().GetAll().Where(c => (c.Class != 99) && (c.Class != 100) && !string.IsNullOrEmpty(c.InviteCode)).ToList();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public AgentRateRange GetRateRange(Guid id)
        {
            try
            {
                var result = _repoFactory.CreateRepository<AgentRateRange>().GetAll().Where(c => id == c.ID).FirstOrDefault();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public AgentInformation GetInformation(Guid agentUID)
        {
            try
            {
                SqlParameter[] paras = {
                    new SqlParameter("@AgentUID",(object)agentUID??DBNull.Value)
                };
                var result = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForDataList<AgentInformation>("PrWeb_AgentInformation_Get", paras).FirstOrDefault();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public RregisteredCount GetRregisteredCount(Guid agentUID)
        {
            try
            {
                SqlParameter[] paras = {
                    new SqlParameter("@AgentUID",(object)agentUID??DBNull.Value)
                };

                var result = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForDataList<RregisteredCount>("PrWeb_AgentUseRregisteredCount_Get", paras).FirstOrDefault();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<WithdrawApply> GetAgentWithdraw(Guid parentID, int? user_id, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                SqlParameter[] paras = {
                    new SqlParameter("@intUserID",(object)user_id??DBNull.Value),
                    new SqlParameter("@dtStartDate",(object)startDate??DBNull.Value),
                    new SqlParameter("@dtEndDate",(object)endDate??DBNull.Value)
                };

                var result = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForDataList<WithdrawApply>("PrWeb_WithdrawApply_Get", paras).ToList();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool AgentWithdraw(Guid agentUID, long? remainAmount, string IPAddress, ref string msg)
        {
            try
            {
                var chvErrMsg = new SqlParameter
                {
                    ParameterName = "@chvErrMsg",
                    Direction = ParameterDirection.Output,
                    Size = 32
                };

                SqlParameter[] paras = {
                    new SqlParameter("@AgentUID",(object)agentUID??DBNull.Value),
                    new SqlParameter("@intWithdrawAmount",(object)remainAmount??DBNull.Value),
                    new SqlParameter("@IPAddress",(object)IPAddress??DBNull.Value),
                    chvErrMsg
                };

                var returnValue = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForResult("PrWeb_AgentWithdraw", paras);
                msg = chvErrMsg.Value.ToString();
                return returnValue == 1;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool UpdateWithdrawApply(Guid applyUID, ApplyStatus status, ref string msg)
        {
            try
            {
                var chvErrMsg = new SqlParameter
                {
                    ParameterName = "@chvErrMsg",
                    Direction = ParameterDirection.Output,
                    Size = 32
                };

                SqlParameter[] paras = {
                    new SqlParameter("@ApplyUID",(object)applyUID??DBNull.Value),
                    new SqlParameter("@tnyStatus",(object)status??DBNull.Value),
                    chvErrMsg
                };

                var returnValue = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForResult("PrWeb_WithdrawApply_Update", paras);
                msg = chvErrMsg.Value.ToString();
                return returnValue == 1;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<AgentApply> GetAgentApply(Guid parentID, int? user_id, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                SqlParameter[] paras = {
                    new SqlParameter("@AgentUID",(object)parentID??DBNull.Value),
                    new SqlParameter("@intUserID",(object)user_id??DBNull.Value),
                    new SqlParameter("@dtStartDate",(object)startDate??DBNull.Value),
                    new SqlParameter("@dtEndDate",(object)endDate??DBNull.Value)
                };

                var result = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForDataList<AgentApply>("PrWeb_AgentApply_Get", paras).ToList();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool AgentPassed(Guid applyUID, ApplyStatus status, string inviteCode, ref string msg)
        {
            try
            {
                var chvErrMsg = new SqlParameter
                {
                    ParameterName = "@chvErrMsg",
                    Direction = ParameterDirection.Output,
                    Size = 32
                };

                SqlParameter[] paras = {
                    new SqlParameter("@ApplyUID",(object)applyUID??DBNull.Value),
                    new SqlParameter("@tnyStatus",(object)status??DBNull.Value),
                    new SqlParameter("@chvInviteCode",(object)inviteCode??DBNull.Value),
                    chvErrMsg
                };

                var returnValue = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForResult("PrWeb_ApplyAgent", paras);
                msg = chvErrMsg.Value.ToString();
                return returnValue == 1;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public List<AgentCustomer> GetAgentUser(Guid agentUID, int? user_id, string loginName, AgentClass? agentClass, ref string msg, ref long sumTotal, bool isAdmin = false)
        {
            try
            {
                var chvErrMsg = new SqlParameter
                {
                    ParameterName = "@chvErrMsg",
                    Direction = ParameterDirection.Output,
                    Size = 32
                };

                var total = new SqlParameter
                {
                    Value = 0,
                    ParameterName = "@intSumTotal",
                    Direction = ParameterDirection.Output,
                    Size = 32
                };

                SqlParameter[] paras = {
                    new SqlParameter("@AgentUID",(object)agentUID??DBNull.Value),
                    new SqlParameter("@intUserID",(object)user_id??DBNull.Value),
                    new SqlParameter("@chvloginName",(object)loginName??DBNull.Value),
                    new SqlParameter("@tnyType",(object)agentClass??DBNull.Value),
                    new SqlParameter("@bitIsAdmin",(object)isAdmin??DBNull.Value),
                    total,
                    chvErrMsg
                };

                var result = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForDataList<AgentCustomer>("PrWeb_AgentUser_Get", paras).ToList();
                msg = chvErrMsg.Value.ToString();
                long.TryParse(total.Value.ToString(), out sumTotal);
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool UpdateAgentUser(Guid operateUID, Guid agentUID, long? remainAmount, int? userID, string email, string tel, string weixin, string qq,
            int? level, string QRUrl, vRateRange rateRange, string inviteCode, string password, string encrypt, int? agentStatus, int? allowWithdraw, ref string msg)
        {
            try
            {
                var chvErrMsg = new SqlParameter
                {
                    ParameterName = "@chvErrMsg",
                    Direction = ParameterDirection.Output,
                    Size = 32
                };

                SqlParameter[] paras = {
                    new SqlParameter("@OperateUID",(object)operateUID??DBNull.Value),
                    new SqlParameter("@AgentUID",(object)agentUID??DBNull.Value),
                    new SqlParameter("@intUerID",(object)userID??DBNull.Value),
                    new SqlParameter("@chvEmail",(object)email??DBNull.Value),
                    new SqlParameter("@chvTel",(object)tel??DBNull.Value),
                    new SqlParameter("@chvWeixin",(object)weixin??DBNull.Value),
                    new SqlParameter("@chvQQ",(object)qq??DBNull.Value),
                    new SqlParameter("@intLevel",(object)level??DBNull.Value),
                    new SqlParameter("@chvQRUrl",(object)QRUrl??DBNull.Value),
                    new SqlParameter("@intDomainID",null),
                    new SqlParameter("@chvPassword",(object)password??DBNull.Value),
                    new SqlParameter("@chvEncrypt",(object)encrypt??DBNull.Value),
                    new SqlParameter("@intDistributeRate",(object)rateRange.DistributeRate??DBNull.Value),
                    new SqlParameter("@tnyStatus",(object)agentStatus??DBNull.Value),
                    new SqlParameter("@chvInviteCode",(object)inviteCode??DBNull.Value),
                    new SqlParameter("@intAllowWithdraw",(object)allowWithdraw??DBNull.Value),
                    chvErrMsg
                };

                var returnValue = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForResult("PrWeb_AgentUser_Update", paras);
                msg = chvErrMsg.Value.ToString();
                return returnValue == 1;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool UpdateAgentUserQRUrl(Guid agentUID, string QRUrl)
        {
            var _result = _repoFactory.CreateRepository<AgentUsers>().GetAll();

            foreach (var item in _result)
            {
                item.QRUrl = $"{QRUrl}?a={ item.InviteCode}";
                _repoFactory.CreateRepository<AgentUsers>().Update(item);
            }

            return true;
        }

        public bool UpdateInformation(Guid operateUID, Guid agentUID, int userID, string email, string tel, string weixin, string qq, string QRUrl,
            string OfficialUser1, string OfficialUser2, string OfficialUser3, int User1Type, int User2Type, int User3Type, ref string msg)
        {
            try
            {
                var chvErrMsg = new SqlParameter
                {
                    ParameterName = "@chvErrMsg",
                    Direction = ParameterDirection.Output,
                    Size = 32
                };
                
                SqlParameter[] paras = {
                    new SqlParameter("@OperateUID",(object)operateUID??DBNull.Value),
                    new SqlParameter("@AgentUID",(object)agentUID??DBNull.Value),
                    new SqlParameter("@intUerID",(object)userID??DBNull.Value),
                    new SqlParameter("@chvEmail",(object)email??DBNull.Value),
                    new SqlParameter("@chvTel",(object)tel??DBNull.Value),
                    new SqlParameter("@chvWeixin",(object)weixin??DBNull.Value),
                    new SqlParameter("@chvQQ",(object)qq??DBNull.Value),
                    new SqlParameter("@chvQRUrl",(object)QRUrl??DBNull.Value),
                    new SqlParameter("@intUser1Type", (object)User1Type ?? DBNull.Value),
                    new SqlParameter("@intUser2Type", (object)User2Type ?? DBNull.Value),
                    new SqlParameter("@intUser3Type", (object)User3Type ?? DBNull.Value),
                    new SqlParameter("@chvOfficialUser1", (object)OfficialUser1 ?? DBNull.Value),
                    new SqlParameter("@chvOfficialUser2", (object)OfficialUser2 ?? DBNull.Value),
                    new SqlParameter("@chvOfficialUser3", (object)OfficialUser3 ?? DBNull.Value),
                    chvErrMsg
                };

                var returnValue = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForResult("PrWeb_AgentUser_Update", paras);
                msg = chvErrMsg.Value.ToString();
                return returnValue == 1;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<AgentCustomer> GetAgentCustomer(Guid agentUID, int? user_id, string loginName, AgentClass? agentClass, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                SqlParameter[] paras = {
                    new SqlParameter("@AgentUID",(object)agentUID??DBNull.Value),
                    new SqlParameter("@intUserID",(object)user_id??DBNull.Value),
                    new SqlParameter("@chvloginName",(object)loginName??DBNull.Value),
                    new SqlParameter("@tnyClass",(object)agentClass??DBNull.Value),
                    new SqlParameter("@dtStartDate",(object)startDate??DBNull.Value),
                    new SqlParameter("@dtEndDate",(object)endDate??DBNull.Value)
                };

                var result = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForDataList<AgentCustomer>("PrWeb_AgentCustomer_Get", paras).ToList();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<GrandConfig> GetGrandConfigItems(Guid agentUID)
        {
            try
            {
                var result = _repoFactory.CreateRepository<GrandConfig>().GetAll().Where(c => agentUID == c.AgentID).ToList();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<vGrandConfig> GetGrandConfig(Guid agentUID, int? user_id, string loginName)
        {
            try
            {
                SqlParameter[] paras = {
                    new SqlParameter("@AgentUID",(object)agentUID??DBNull.Value),
                    new SqlParameter("@intUserID",(object)user_id??DBNull.Value),
                    new SqlParameter("@chvloginName",(object)loginName??DBNull.Value)
                };
                var result = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForDataList<vGrandConfig>("PrWeb_GrandConfig_Get", paras).ToList();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public vMaxMinRate GetAgentMaxMinRate(Guid OperateUID, Guid agentID)
        {
            try
            {
                SqlParameter[] paras = {
                    new SqlParameter("@OperateUID",(object)OperateUID??DBNull.Value),
                    new SqlParameter("@AgentUID",(object)agentID??DBNull.Value)
                };

                var result = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForDataList<vMaxMinRate>("PrWeb_AgentMaxMinRate_Get", paras).FirstOrDefault();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }




        public bool UpdateGrandConfig(Guid operateUID, Guid? grandUID, Guid agentUID, Guid userUID, int distributeRate, ref string msg)
        {
            try
            {
                var chvErrMsg = new SqlParameter
                {
                    ParameterName = "@chvErrMsg",
                    Direction = ParameterDirection.Output,
                    Size = 32
                };

                SqlParameter[] paras = {
                    new SqlParameter("@OperateUID",(object)operateUID??DBNull.Value),
                    new SqlParameter("@GrandUID",(object)grandUID??DBNull.Value),
                    new SqlParameter("@ParentUID",(object)agentUID??DBNull.Value),
                    new SqlParameter("@UserUID",(object)userUID??DBNull.Value),
                    new SqlParameter("@intDistributeRate",(object)distributeRate??DBNull.Value),
                    chvErrMsg
                };
                var returnValue = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForResult("PrWeb_GrandConfig_Update", paras);
                msg = chvErrMsg.Value.ToString();
                return returnValue == 1;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool DeleteGrandConfig(Guid operateUID, Guid? grandUID, ref string msg)
        {
            try
            {
                var chvErrMsg = new SqlParameter
                {
                    ParameterName = "@chvErrMsg",
                    Direction = ParameterDirection.Output,
                    Size = 32
                };

                SqlParameter[] paras = {
                    new SqlParameter("@OperateUID",(object)operateUID??DBNull.Value),
                    new SqlParameter("@GrandUID",(object)grandUID??DBNull.Value),
                    chvErrMsg
                };
                var returnValue = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForResult("PrWeb_GrandConfig_Delete", paras);
                msg = chvErrMsg.Value.ToString();
                return returnValue == 1;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool UpdatePassword(Guid id, string password, string encrypt, ref string msg)
        {
            try
            {
                var chvErrMsg = new SqlParameter
                {
                    ParameterName = "@chvErrMsg",
                    Direction = ParameterDirection.Output,
                    Size = 32
                };

                SqlParameter[] paras = {
                    new SqlParameter("@UID",(object)id??DBNull.Value),
                    new SqlParameter("@chvPassword",(object)password??DBNull.Value),
                    new SqlParameter("@chvEncrypt",(object)encrypt??DBNull.Value),
                    chvErrMsg
                };
                var returnValue = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForResult("PrWeb_AgentUser_Update_Password", paras);
                msg = chvErrMsg.Value.ToString();
                return returnValue == 1;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool CreateAgent(Guid operateUID, int? platformID, int? userID, string loginName, string inviteCode, string password, string encrypt, string email, string tel
            , string weixin, string qq, string QRUrl, string vCode, ref string msg)
        {
            try
            {
                var chvErrMsg = new SqlParameter
                {
                    ParameterName = "@chvErrMsg",
                    Direction = ParameterDirection.Output,
                    Size = 32
                };

                SqlParameter[] paras = {
                    new SqlParameter("@OperateUID",(object)operateUID??DBNull.Value),
                    new SqlParameter("@platformID",(object)platformID??DBNull.Value),
                    new SqlParameter("@chvLoginName",(object)loginName??DBNull.Value),
                    new SqlParameter("@intUserID",(object)userID??DBNull.Value),
                    new SqlParameter("@chvEmail",(object)email??DBNull.Value),
                    new SqlParameter("@chvTel",(object)tel??DBNull.Value),
                    new SqlParameter("@chvWeixin",(object)weixin??DBNull.Value),
                    new SqlParameter("@chvQQ",(object)qq??DBNull.Value),
                    new SqlParameter("@chvPassword",(object)password??DBNull.Value),
                    new SqlParameter("@chvEncrypt",(object)encrypt??DBNull.Value),
                    new SqlParameter("@chvInviteCode",(object)inviteCode??DBNull.Value),
                    new SqlParameter("@chvQRUrl",(object)QRUrl??DBNull.Value),
                    new SqlParameter("@chvVCode",(object)vCode??DBNull.Value),
                    chvErrMsg
                };
                var returnValue = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForResult("PrWeb_AgentUser_Create_Area", paras);
                msg = chvErrMsg.Value.ToString();
                return returnValue == 1;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool CustomerUpgrade(Guid operateUID, int userID, string inviteCode, string QRUrl, ref string msg)
        {
            try
            {
                var chvErrMsg = new SqlParameter
                {
                    ParameterName = "@chvErrMsg",
                    Direction = ParameterDirection.Output,
                    Size = 32
                };

                SqlParameter[] paras = {
                    new SqlParameter("@OperateUID",(object)operateUID??DBNull.Value),
                    new SqlParameter("@intUserID",(object)userID??DBNull.Value),
                    new SqlParameter("@chvInviteCode",(object)inviteCode??DBNull.Value),
                    new SqlParameter("@chvQRUrl",(object)QRUrl??DBNull.Value),
                    chvErrMsg
                };
                var returnValue = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForResult("PrWeb_CustomerUpgrade", paras);
                msg = chvErrMsg.Value.ToString();
                return returnValue == 1;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public int GetAgentRecursiveDown(Guid agentUID)
        {
            try
            {
                SqlParameter[] paras = {
                    new SqlParameter("@AgentUID",(object)agentUID??DBNull.Value)
                };

                var returnValue = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForResult("PrWeb_AgentRecursiveDownCount_Get", paras);
                return returnValue;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public int GetAgentApplyCount(Guid agentUID)
        {
            try
            {
                SqlParameter[] paras = {
                    new SqlParameter("@AgentUID",(object)agentUID??DBNull.Value)
                };

                var returnValue = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForResult("PrWeb_AgentApplyCount_Get", paras);
                return returnValue;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool CreateAutoAgentApply(string inviteCode, string IPAddress)
        {
            try
            {
                var chvErrMsg = new SqlParameter
                {
                    ParameterName = "@chvErrMsg",
                    Direction = ParameterDirection.Output,
                    Size = 32
                };

                var redirectURL = new SqlParameter
                {
                    ParameterName = "@chvRedirectURL",
                    Direction = ParameterDirection.Output,
                    Size = 32
                };

                SqlParameter[] paras = {
                    new SqlParameter("@chvInviteCode",(object)inviteCode??DBNull.Value),
                    new SqlParameter("@chvIPAddress",(object)IPAddress??DBNull.Value),
                    redirectURL,
                    chvErrMsg
                };
                var returnValue = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForResult("PrWeb_AutoAgentApply_Create", paras);
                return returnValue == 1;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
