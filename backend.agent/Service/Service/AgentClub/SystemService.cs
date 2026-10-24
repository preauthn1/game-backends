using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Repository;
using Repository.Entity.agentclub;

namespace Service.Service.AgentClub
{
    public class SystemService
    {
        private AgentClubFactory _repoFactory = new AgentClubFactory();

        public bool CreateLogonLog(Guid UID, string IPAddress)
        {
            SqlParameter[] paras = {
                    new SqlParameter("@UID",(object)UID??DBNull.Value),
                    new SqlParameter("@chvIPAddress",(object)IPAddress??DBNull.Value),
                    new SqlParameter("@intYYYYMMDD",(object)DateTime.Now.ToString("yyyyMMdd")??DBNull.Value)

                };

            var returnValue = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForResult("PrWeb_LogonLog_Create", paras);
            return returnValue == 1;
        }

        public SystemConfig GetSystemConfig()
        {
            try
            {
                var result = _repoFactory.CreateRepository<SystemConfig>().GetAll().FirstOrDefault();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public bool UpdateSystemConfig(SystemConfig systemConfig, Guid operateUID, Guid targetUID, ref string message)
        {
            try
            {
                var result = _repoFactory.CreateRepository<SystemConfig>().GetAll().FirstOrDefault();

                var oldData = $"Class1ChargeFee: {result.Class1ChargeFee}, class2ChargeFee: {result.Class2ChargeFee}," +
                $"class3ChargeFee: {result.Class3ChargeFee}, class4ChargeFee: {result.Class4ChargeFee}" +
                $"OfficialUser1: {result.OfficialUser1}, OfficialUser2: {result.OfficialUser2}, OfficialUser3: {result.OfficialUser3}, OfficialSite: {result.OfficialSite}";

                var newData = $"Class1ChargeFee: {systemConfig.Class1ChargeFee}, class2ChargeFee: {systemConfig.Class2ChargeFee}," +
                    $"class3ChargeFee: {systemConfig.Class3ChargeFee}, class4ChargeFee: {systemConfig.Class4ChargeFee}" +
                    $"OfficialUser1: {systemConfig.OfficialUser1}, OfficialUser2: {systemConfig.OfficialUser2}, OfficialUser3: {systemConfig.OfficialUser3}, OfficialSite: {systemConfig.OfficialSite}";

                result.Class1ChargeFee = systemConfig.Class1ChargeFee * 100;
                result.Class2ChargeFee = systemConfig.Class2ChargeFee * 100;
                result.Class3ChargeFee = systemConfig.Class3ChargeFee * 100;
                result.Class4ChargeFee = systemConfig.Class4ChargeFee * 100;
                result.OfficialUser1 = systemConfig.OfficialUser1;
                result.OfficialUser2 = systemConfig.OfficialUser2;
                result.OfficialUser3 = systemConfig.OfficialUser3;
                result.User1Type = systemConfig.User1Type;
                result.User2Type = systemConfig.User2Type;
                result.User3Type = systemConfig.User3Type;
                result.OfficialSite = systemConfig.OfficialSite;

                _repoFactory.CreateRepository<SystemConfig>().Update(result);

                AddChangeLog(99, operateUID, operateUID, oldData, newData, "修改系统设定");
                message = "操作成功";
                return true;
            }
            catch (Exception ex)
            {
                message = "操作失败";
                return false; ;
            }
        }

        public bool UpdateDomainConfig(DomainConfig domainConfig, ref string message)
        {
            try
            {
                if (domainConfig.ID != 0)
                {
                    var result = _repoFactory.CreateRepository<DomainConfig>().GetAll().Where(x => x.ID.Equals(domainConfig.ID)).FirstOrDefault();
                    if (!result.Domain.Equals(domainConfig.Domain))
                    {
                        result.Domain = domainConfig.Domain;
                        result.Flag = 1;
                        _repoFactory.CreateRepository<DomainConfig>().Update(result);
                    }
                }
                else
                {
                    var result = _repoFactory.CreateRepository<DomainConfig>().GetAll().Where(x => x.Domain.Equals(domainConfig.Domain)).FirstOrDefault();

                    if (result == null)
                    {
                        _repoFactory.CreateRepository<DomainConfig>().Insert(domainConfig);
                    }
                    else
                    {
                        message = "名称重覆";
                        return false;
                    }
                }
                message = "操作成功";
                return true;
            }
            catch (Exception ex)
            {
                message = "操作失败";
                return false; ;
            }
        }


        public DomainConfig GetDomainConfig()
        {
            try
            {
                var result = _repoFactory.CreateRepository<DomainConfig>().GetAll().FirstOrDefault();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<DomainConfig> GetDomainConfig(string domain)
        {
            try
            {
                var result = _repoFactory.CreateRepository<DomainConfig>().GetAll().Where(c =>
                     (string.IsNullOrEmpty(domain) || c.Domain.Contains(domain))).ToList();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<Platform> GetPlatform()
        {
            var result = _repoFactory.GetDatabase("CenterDB").ExecuteSqlQuery<Platform>("SELECT PlatformID, Name FROM [CenterDB].[Area].[Platform]").ToList();
            return result;
        }

        public List<Area> GetArea(int platformID)
        {
            SqlParameter[] paras = {
                    new SqlParameter("@PlatformID",(object)platformID??DBNull.Value)
                };
            var result = _repoFactory.GetDatabase("CenterDB").ExecuteSqlQuery<Area>("SELECT AreaID, AreaName FROM [CenterDB].[Area].[Area] WHERE PlatformID = @PlatformID", paras).ToList();
            return result;
        }


        public bool AddChangeLog(int changeType, Guid operateUID, Guid targetUID, string oldData, string newData, string memo)
        {
            SqlParameter[] paras = {
                    new SqlParameter("@intChangeType",(object)changeType??DBNull.Value),
                    new SqlParameter("@OperateUID",(object)operateUID??DBNull.Value),
                    new SqlParameter("@TargetUID",(object)targetUID??DBNull.Value),
                    new SqlParameter("@chvOldData",(object)oldData??DBNull.Value),
                    new SqlParameter("@chvNewData",(object)newData??DBNull.Value),
                    new SqlParameter("@chvMemo",(object)memo??DBNull.Value)
                };

            var returnValue = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForResult("PrWeb_AddChangeLog", paras);
            return returnValue == 1;
        }

    }
}
