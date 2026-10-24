using FW.Core.Helper;
using game.Entity.ForbidUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game.Entity.Time;

namespace game.Service
{
    /// <summary>
    /// 封号业务
    /// </summary>
    public interface IForbidUserService
    {
        /// <summary>
        /// 封号
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ForbidUser(int userID, string memo, string ipAddress, ref string message);

        /// <summary>
        /// 解封账号
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool UnForbidUser(int userID, string memo, string ipAddress, ref string message);

        /// <summary>
        /// 获取封号用户
        /// </summary>
        /// <param name="memo"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<ForbidUser> GetForbidUser(int? userID, string memo, int pageIndex, int pageSize);

        /// <summary>
        /// 根据ID获取封号用户信息
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        ForbidUser GetForbidUserByID(int userID);

        /// <summary>
        /// 获取封号用户记录
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="memo"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<ForbidUserLog> GetForbidUserLog(int? userID, string memo, TimeType timeType, DateTime? beginTime, DateTime? endTime, int pageIndex, int pageSize);

        /// <summary>
        /// 踢人
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="memo"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool TickUser(int userID,string memo,ref string message);

        /// <summary>
        /// 取得支付宝或手机号封锁清单
        /// </summary>
        /// <param name="accountID"></param>
        /// <param name="idType"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        List<ForbidAccount> GetForbidAccount(string accountID, int? idType, DateTime? startTime, DateTime? endTime);
        /// <summary>
        /// 新增电话或支付宝封号
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool AddForbidAccount(ForbidAccount model);
        /// <summary>
        /// 解除电话或支付宝封号
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool UnForbidAccount(ForbidAccount model);

    }
}
