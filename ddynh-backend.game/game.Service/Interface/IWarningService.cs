using FW.Core.Helper;
using game.Entity.Warning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Service
{
    /// <summary>
    /// 报警业务接口
    /// </summary>
    public interface IWarningService
    {
        /// <summary>
        /// 添加 用户关注报警
        /// </summary>
        /// <param name="adminUserID">后台用户ID</param>
        /// <param name="typeID"></param>
        /// <returns></returns>
        bool AddUserFocus(int adminUserID, int typeID,ref string message);
        /// <summary>
        /// 移除 用户关注报警
        /// </summary>
        /// <param name="adminUserID">后台用户ID</param>
        /// <param name="typeID"></param>
        /// <returns></returns>
        bool RemoveUserFocus(int adminUserID, int typeID, ref string message);
        /// <summary>
        /// 查询 用户关注报警
        /// </summary>
        /// <param name="adminUserID">后台用户ID</param>
        /// <param name="typeID"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<UserFocus> GetUserFocus(int? adminUserID, int? typeID, int pageIndex, int pageSize);

        /// <summary>
        /// 查询 报警日志
        /// </summary>
        /// <param name="adminUserID">后台用户ID</param>
        /// <param name="typeID">报警类型</param>
        /// <param name="begin">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<WarningLog> GetWarningLog(int? adminUserID, int? typeID, DateTime? begin, DateTime? end, int pageIndex, int pageSize);

        /// <summary>
        /// 查询 后台用户是否有报警日志
        /// </summary>
        /// <param name="adminUserID"></param>
        /// <returns></returns>
        bool ExistWarning(int adminUserID,ref string message);

        /// <summary>
        /// 后台用户已阅报警日志
        /// </summary>
        /// <param name="adminUserID"></param>
        void ReadedWarning(int adminUserID);

        /// <summary>
        /// 查询 报警类型
        /// </summary>
        /// <returns></returns>
        List<WarningType> GetWarningType();

        /// <summary>
        /// 查询 报警类型
        /// </summary>
        /// <param name="typeID"></param>
        /// <returns></returns>
        WarningType GetWarningTypeByID(int typeID);
        
        /// <summary>
        /// 修改报警类型
        /// </summary>
        /// <param name="site"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ModifyWarningType(WarningType warningType, ref string message);
    }
}
