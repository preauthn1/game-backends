using FW.Core.Helper;
using game.Entity.Game;
using game.Entity.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Service
{
    /// <summary>
    /// 禁言
    /// </summary>
    public interface IForbidChatService
    {
         /// <summary>
        /// 添加禁言用户
        /// </summary>
        /// <param name="user"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool AddForbidChatUser(ForbidChatUser user, ref string message);

        /// <summary>
        /// 删除禁言用户
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool DeleteForbidChatUser(int userID, ref string message);

        /// <summary>
        /// 根据Id获取禁言用户
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        ForbidChatUser GetForbidChatUserByID(int userID);

        /// <summary>
        /// 获取禁言用户
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="memo"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<ForbidChatUser> GetForbidChatUser(int? userID, string memo, DateTime? beginTime, DateTime? endTime, int pageIndex, int pageSize);
    }
}
