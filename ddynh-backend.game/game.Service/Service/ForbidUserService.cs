using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using game.Entity.Time;
using game.Entity.Game;
using game.Entity.ForbidUser;
using System.Data.SqlClient;
using game.Infrastructure;

namespace game.Service
{
    public class ForbidUserService : IForbidUserService
    {
        private IRepositoryFactory _repoFactory;
        private IDatabaseFactory _dbFactory;
        public ForbidUserService(IRepositoryFactory repoFactory, IDatabaseFactory dbFactory)
        {
            _repoFactory = repoFactory;
            _dbFactory = dbFactory;
        }


        public bool ForbidUser(int userID, string memo, string ipAddress, ref string message)
        {
            //用户存储过程
            SqlParameter[] paras = {
                                   new SqlParameter("@intUserID",userID),
                                   new SqlParameter("@chvIPAddress",ipAddress),
                                   new SqlParameter("@chvMemo",memo),
                                   DBHelper.GetProcedureErrorMsg()
                                   };
            int returnVal = _dbFactory.GetDatabase(game.Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Game_ForbidUser", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);
            return returnVal == 1;
        }

        public bool UnForbidUser(int userID, string memo, string ipAddress, ref string message)
        {
            //用户存储过程
            SqlParameter[] paras = {
                                   new SqlParameter("@intUserID",userID),
                                   new SqlParameter("@chvIPAddress",ipAddress),
                                   new SqlParameter("@chvMemo",memo),
                                   DBHelper.GetProcedureErrorMsg()
                                   };
            int returnVal = _dbFactory.GetDatabase(game.Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Game_UnForbidUser", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);
            return returnVal == 1;
        }

        public FW.Core.Helper.PagedList<Entity.ForbidUser.ForbidUser> GetForbidUser(int? userID, string memo, int pageIndex, int pageSize)
        {
            IQueryable<ForbidUser> list = _repoFactory.CreateRepository<ForbidUser>()
                .Entities
                .Where(m => (userID == null || m.Id == userID) &&
                    (memo == null || m.Memo.Contains(memo))
                )
                .OrderByDescending(m => m.Id);

            return new FW.Core.Helper.PagedList<ForbidUser>(list, pageIndex, pageSize);
        }

        public Entity.ForbidUser.ForbidUser GetForbidUserByID(int userID)
        {
            return _repoFactory.CreateRepository<ForbidUser>().GetById(userID);
        }

        public FW.Core.Helper.PagedList<Entity.ForbidUser.ForbidUserLog> GetForbidUserLog(int? userID, string memo, TimeType timeType, DateTime? beginTime, DateTime? endTime, int pageIndex, int pageSize)
        {
            IQueryable<ForbidUserLog> list = _repoFactory.CreateRepository<ForbidUserLog>()
                .Entities
                .Where(m => (userID == null || m.UserID == userID)
                            && (string.IsNullOrEmpty(memo) || m.Memo.Contains(memo))
                )
                .OrderByDescending(m => m.Id);


            if (timeType == TimeType.ForbidTime)
            {
                list = list.Where(m => (beginTime == null || m.CreateTime >= beginTime) && (endTime == null || m.CreateTime <= endTime)).Select(m => m);
            }
            else
            {
                list = list.Where(m => (beginTime == null || m.UserInfo.CreateTime >= beginTime) && (endTime == null || m.UserInfo.CreateTime <= endTime)).Select(m => m);
            }
            return new FW.Core.Helper.PagedList<ForbidUserLog>(list, pageIndex, pageSize);
        }


        public bool TickUser(int userID, string memo, ref string message)
        {
            if (_repoFactory.CreateRepository<ForbidUser>().Entities.Where(m => m.Id == userID).Count() > 0)
            {
                message = "此用户已被封号";
                return false;
            }
            ForbidUser fu = new Entity.ForbidUser.ForbidUser();
            //fu.BankAmount=0;
            fu.CashAmount = 0;
            fu.CreateTime = DateTime.Now;
            fu.Memo = memo;
            fu.Id = userID;
            _repoFactory.CreateRepository<ForbidUser>().Insert(fu);
            var certification = _repoFactory.CreateRepository<Certification>().Entities.Where(m => m.Id == userID).FirstOrDefault();
            game.Entity.Notify.Attr attr = new Entity.Notify.Attr();
            attr.CreateTime = DateTime.Now;
            attr.LastPostTime = DateTime.Now;
            attr.Param = 1;
            attr.SParam = certification == null ? string.Empty : certification.CerCode;
            attr.Status = 0;
            attr.TypeID = 5;
            attr.UserID = userID;
            _repoFactory.CreateRepository<game.Entity.Notify.Attr>().Insert(attr);

            message = "操作成功！";
            return true;
        }

        public List<ForbidAccount> GetForbidAccount(string accountID, int? idType, DateTime? startTime, DateTime? endTime)
        {
            SqlParameter[] paras = {
                                   new SqlParameter("@chvAccountID",accountID),
                                   new SqlParameter("@tnyIdType",idType),
                                   new SqlParameter("@dtStartTime",startTime),
                                    new SqlParameter("@dtEndTime",endTime)
                                   };
            var _list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<ForbidAccount>("dbo.PrWeb_Game_GetForbidAccount", paras).ToList();
            return _list;
        }

        public bool AddForbidAccount(ForbidAccount model)
        {
            SqlParameter[] paras = {
                                   new SqlParameter("@chvAccountID",model.AccountID),
                                   new SqlParameter("@tnyIdType",model.IdType),
                                    new SqlParameter("@chvMemo",model.Memo)
                                   };

             _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Game_ForbidAccount", paras);
            return true;
        }

        public bool UnForbidAccount(ForbidAccount model)
        {
            SqlParameter[] paras = {
                                   new SqlParameter("@chvAccountID",model.AccountID),
                                   new SqlParameter("@tnyIdType",model.IdType)
                                   };

            _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Game_UnForbidAccount", paras);
            return true;
        }
    }
}
