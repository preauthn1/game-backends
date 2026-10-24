using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Repository;
using Repository.Entity;
using Repository.Models;
using Repository.ViewModel;
using Service.Helper;

namespace Service.Service
{
    public class AgentTotalService
    {
        private MSRepositoryFactory _repoFactory = new MSRepositoryFactory();

        public List<Repository.Entity.agent_money_total> GetTransferLog(deviceType? deviceType, transferType? transferType, int? user_agent_id, int? minPrice, int? maxPrice, DateTime? startDate, DateTime? endDate)
        {
            var result = _repoFactory.CreateRepository<Repository.Entity.agent_money_total>().GetAll().Where(c =>
                (deviceType == null || deviceType == c.device)
                && (transferType == null || transferType == c.type)
                && (user_agent_id == null || user_agent_id == c.user_agent_id)
                && (minPrice == 0 || c.money > minPrice)
                && (maxPrice == 0 || c.money < maxPrice)
                && (startDate == null || c.created_at > startDate)
                && (endDate == null || c.created_at < endDate));
            return result.ToList();
        }

        public Repository.Entity.agent_total GetAgentTotal(string username)
        {
            var result = _repoFactory.CreateRepository<Repository.Entity.agent_total>().GetAll().Where(c => username == c.username).FirstOrDefault();
            return result;
        }

        public List<Repository.Entity.agent_user> GetAgent(int? parent_id, status? status, transfer? transfer, int? agent_id, string agent_name, int? minPrice, int? maxPrice)
        {

            var result = _repoFactory.CreateRepository<Repository.Entity.agent_user>().GetAll().Where(c => (parent_id == c.parent_id)
            && (transfer == null || transfer == c.transfer)
            && (status == null || status == c.status)
            && (agent_id == null || agent_id == c.id)
            && (agent_name == null || agent_name == c.name)
            && (minPrice == 0 || minPrice > c.money)
            && (maxPrice == 0 || maxPrice < c.money));
            return result.ToList();
        }


        public bool CreateAgent(int parent_id, string name, int player_id, int min_money, int qq, string weixin
            , string alipay, long telphone, string auto_reply, string remark, transfer transfer, short level, string mailContent, ref string message)
        {
            var agentRepository = _repoFactory.CreateRepository<Repository.Entity.agent_user>();

            var player = GetPlayerInfo(player_id);
            if (player == null)
            {
                message = "很抱歉！游戏ID并不存在";
                return false;
            }

            try
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                var encrypt = DemoRandom.GetRandomString(6);
                var source = "123456" + encrypt;

                using (MD5 md5Hash = MD5.Create())
                {
                    string hash = GetMd5Hash(md5Hash, source);
                    var sequence = DemoRandom.GetRandomMumber(10000, 99999);
                    var insertData = new Repository.Entity.agent_user()
                    {
                        id = sequence,
                        telphone = telphone,
                        qq = qq,
                        area = "localhost",
                        ip = "localhost",
                        weixin = weixin,
                        alipay = alipay,
                        name = name,
                        password = [REDACTED],
                        encrypt = encrypt,
                        parent_id = parent_id,
                        player_id = player_id,
                        level = level,
                        min_money = min_money,
                        remark = remark,
                        transfer = transfer,
                        auto_reply = auto_reply,
                        created_at = DateTime.Now,
                        updated_at = DateTime.Now,
                        session_id = "trqg0spkp4p2nrr397jl3ddqu2"
                    };

                    agentRepository.Insert(insertData);
                    sendSysMail(player_id, mailContent);
                }

                message = "操作成功";
                return true;
            }
            catch (Exception ex)
            {
                message = "操作失败";
                return false;
            }
        }

        public bool SaveAgent(int id, string name, int player_id, int min_money, int qq, string weixin
            , string alipay, long telphone, string auto_reply, string remark, transfer transfer, short level, ref string message)
        {
            var agentRepository = _repoFactory.CreateRepository<Repository.Entity.agent_user>();
            var result = agentRepository.GetAll().Where(c => (id == c.id)).FirstOrDefault();

            if (result == null)
            {
                message = "很抱歉！对方代理不存在!";
                return false;
            }
            try
            {
                result.name = name;
                result.player_id = player_id;
                result.min_money = min_money;
                result.qq = qq;
                result.weixin = weixin;
                result.alipay = alipay;
                result.telphone = telphone;
                result.auto_reply = auto_reply;
                result.remark = remark;
                result.transfer = transfer;
                result.level = level;
                result.updated_at = DateTime.Now;

                agentRepository.Update(result);
                message = "操作成功";
                return true;
            }
            catch (Exception ex)
            {
                message = "操作失败";
                return false;
            }
        }

        private void sendSysMail(int player_id, string content)
        {
            int intMsgID = 0;
            var chvTitle = "代理审核通知";
            var intSort = 1;

            var msgID = new SqlParameter
            {
                ParameterName = "@intMsgID",
                Value = intMsgID,
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            SqlParameter[] paras = {
                new SqlParameter("@intUserID",(object)player_id??DBNull.Value),
                new SqlParameter("@chvTitle",(object)chvTitle??DBNull.Value),
                new SqlParameter("@chvContent",(object)content??DBNull.Value),
                new SqlParameter("@intSort",(object)intSort??DBNull.Value),
                msgID
                };

            try
            {
                var fff = _repoFactory.GetDatabase("CenterDB").ExecuteCommand(DbHelper.GenerateCommandText("PrPs_SendSysMail", paras), paras);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public AgentOrder GetAgentUserMoney(int id)
        {
            var agentMoneyUserRepository = _repoFactory.CreateRepository<Repository.Entity.agent_money_user>();
            var agentMoneyUserModel = agentMoneyUserRepository.GetAll().Where(c => c.id == id
             && c.status == 1
             && (c.type == 0 || c.type == 2)).FirstOrDefault();

            if (agentMoneyUserModel == null)
            {
                return new AgentOrder();
            }

            var result = new AgentOrder();
            SqlParameter[] paras = {
                new SqlParameter("id",(object)id??DBNull.Value)
                };

            if (agentMoneyUserModel.type == 2)
            {

                result = _repoFactory.GetDatabase("adminConnStr").ExecuteSqlQuery<AgentOrder>("SELECT M.id, M.money, M.agent_id, M.user_id, U.money AS balance, M.created_at, M.type"
                + " FROM[AdminSys].[dbo].[agent_money_user] M"
                + " JOIN[AdminSys].[dbo].[agent_user] U ON M.agent_id = U.id"
                + " WHERE @id = M.id", paras).FirstOrDefault();
            }
            else if (agentMoneyUserModel.type == 0)
            {
                result = _repoFactory.GetDatabase("adminConnStr").ExecuteSqlQuery<AgentOrder>("SELECT M.id, M.money, M.agent_id, M.user_id"
                + " , CAST((C.Amount + B.Amount) AS INT) AS balance, M.created_at, M.type"
                + " FROM[AdminSys].[dbo].[agent_money_user] M"
                + " LEFT JOIN[CenterDB].[Users].[UserInfo] U ON M.user_id = U.UserID"
                + " LEFT JOIN[CenterDB].[Money].[Cash] C ON C.UserID = U.UserID"
                + " LEFT JOIN[CenterDB].[Money].[Bank]B ON B.UserID = U.UserID"
                + " WHERE @id = M.id", paras).FirstOrDefault();
            }
            return result;
        }
        public List<Repository.Entity.agent_login_total> GetLoginLog(int totalAgentId)
        {
            var result = _repoFactory.CreateRepository<Repository.Entity.agent_login_total>().GetAll().Where(c => (totalAgentId == c.agent_id));
            return result.ToList();
        }

        public List<Repository.Entity.agent_login_user> GetAgentLoginLog(int totalAgentId, int? agent_id, string ip)
        {
            SqlParameter[] paras = {
                new SqlParameter("parent_id",(object)totalAgentId??DBNull.Value),
                new SqlParameter("agent_id",(object)agent_id??DBNull.Value),
                new SqlParameter("ip",(object)ip??DBNull.Value)
                };

            var result = _repoFactory.GetDatabase("adminConnStr").ExecuteSqlQuery<Repository.Entity.agent_login_user>("SELECT L.* FROM agent_login_user L"
              + " JOIN agent_user U ON L.agent_id = U.id"
              + " WHERE @parent_id =  U.parent_id AND"
              + " (@agent_id IS NULL OR @agent_id = L.agent_id) AND"
              + " (@ip IS NULL OR @ip = L.ip)", paras).ToList();
            return result.ToList();
        }

        public UserInfo GetPlayerInfo(int player_id)
        {
            SqlParameter[] paras = {
                new SqlParameter("player_id",(object)player_id??DBNull.Value)
                };
            var result = _repoFactory.GetDatabase("CenterDB").ExecuteSqlQuery<UserInfo>("SELECT U.UserID, C.Amount AS cash, B.Amount AS bank FROM [CenterDB].[Users].[UserInfo] U"
                + " JOIN [CenterDB].Money.Cash C ON C.UserID = U.UserID"
                + " JOIN [CenterDB].Money.Bank B ON B.UserID = U.UserID"
                + " WHERE @player_id = U.UserID", paras).FirstOrDefault();
            return result;
        }

        public static string GetMd5Hash(MD5 md5Hash, string input)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
    }
}
