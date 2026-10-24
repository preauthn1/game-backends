using FW.Core.Helper;
using game.Entity.Stat;
using game.Entity.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Service
{
    /// <summary>
    /// 用户统计业务
    /// </summary>
    public interface IUserStatService
    {
        /// <summary>
        /// 获取注册用户记录
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        List<RegUserLog> GetRegUserLog(int operatorID, int? platformID, DateTime? beginTime, DateTime? endTime);

        /// <summary>
        /// 获取登录用户记录
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        List<LogonUserLog> GetLogonUserLog(int operatorID, int? platformID, DateTime? beginTime, DateTime? endTime);

        /// <summary>
        /// 获取用户登录记录
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="userID"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<LogonLog> GetUserLogonLog(DateTime? beginTime, DateTime? endTime, int? userID, int pageIndex, int pageSize);

        /// <summary>
        /// 留存率
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        List<KeepLog> GetKeepLog(int adminId, int? platformID, DateTime? beginTime, DateTime? endTime);

        /// <summary>
        /// 付費用戶留存
        /// </summary>
        List<ChargeKeepLog> GetChargeKeepLog(int? hdType);
        /// <summary>
        /// 有效會員留存
        /// </summary>
        List<ActiveUserKeepLog> GetActiveUserKeepLog();
        /// <summary>
        ///  用户统计信息查询
        /// </summary>
        /// <param name="searchType">查询类型 1同IP注册数 2 同IP登录数 3同Mac注册数 4同Mac登录数</param>
        /// <param name="count">相同数量,只查大于等于此数量的数据</param>
        /// <returns></returns>
        Dictionary<string, string> SearchUserStatInfo(byte searchType, int count);

        /// <summary>
        /// 查询用户等级人数信息
        /// </summary>
        /// <returns></returns>
        List<GradeSetting> GetUserGradeCount();

        /// <summary>
        /// 获取用户等级配置列表
        /// </summary>
        /// <returns></returns>
        List<GradeSetting> GetGradeSetting();
    }
}
