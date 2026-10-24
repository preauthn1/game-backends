using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using game.Entity.Game;

namespace game.Service
{
    public class ForbidChatService : IForbidChatService
    {
        private IRepositoryFactory _repoFactory;
        public ForbidChatService(IRepositoryFactory repoFactory)
        {
            _repoFactory = repoFactory;
        }

        public bool AddForbidChatUser(ForbidChatUser user, ref string message)
        {
            var u = GetForbidChatUserByID(user.Id);
            if (u == null)
            {
                user.CreateTime = DateTime.Now;
                _repoFactory.CreateRepository<ForbidChatUser>().Insert(user);
            }
            else
            {
                user.CreateTime = DateTime.Now;
                _repoFactory.CreateRepository<ForbidChatUser>().Update(user);
            }
            message = "操作成功";
            return true;
        }

        public bool DeleteForbidChatUser(int userID, ref string message)
        {
            var u = GetForbidChatUserByID(userID);
            if (u != null)
            {
                _repoFactory.CreateRepository<ForbidChatUser>().Delete(u);
            }

            message = "操作成功";
            return true;
        }

        public ForbidChatUser GetForbidChatUserByID(int userID)
        {
            return _repoFactory.CreateRepository<ForbidChatUser>().GetById(userID);
        }

        public FW.Core.Helper.PagedList<ForbidChatUser> GetForbidChatUser(int? userID, string memo, DateTime? beginTime, DateTime? endTime, int pageIndex, int pageSize)
        {
            IQueryable<ForbidChatUser> list = _repoFactory.CreateRepository<ForbidChatUser>()
                .Entities
                .Where(m => (userID == null || m.Id == userID) &&
                        (memo == null || m.Memo.Contains(memo)) &&
                        (beginTime == null || m.CreateTime >= beginTime) &&
                        (endTime == null || m.CreateTime <= endTime)
                )
                .OrderByDescending(m => m.CreateTime);

            return new FW.Core.Helper.PagedList<ForbidChatUser>(list, pageIndex, pageSize);
        }
    }
}
