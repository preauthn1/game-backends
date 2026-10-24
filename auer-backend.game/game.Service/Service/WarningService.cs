using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Warning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Service.Service
{
    public class WarningService : IWarningService
    {
        private IRepositoryFactory _repoFactory;
        private IDatabaseFactory _dbFactory;
        public WarningService(IRepositoryFactory repoFactory, IDatabaseFactory dbFactory)
        {
            _repoFactory = repoFactory;
            _dbFactory = dbFactory;
        }

        public bool AddUserFocus(int adminUserID, int typeID, ref string message)
        {
            UserFocus userfocus = new UserFocus();
            userfocus.UserID = adminUserID;
            userfocus.TypeID = typeID;
            if (_repoFactory.CreateRepository<UserFocus>().Entities.Where(m => m.UserID == adminUserID && m.TypeID == typeID).Count() > 0)
            {
                message = "操作失败！已有该数据！";
                return false;
            }
            _repoFactory.CreateRepository<UserFocus>().Insert(userfocus);
            if (_repoFactory.CreateRepository<WarningStatus>().Entities.Where(m => m.Id == adminUserID).Count() == 0)
            {
                WarningStatus ws = new WarningStatus();
                ws.Status = WarningStatusStatus.No;
                ws.Id = adminUserID;
                _repoFactory.CreateRepository<WarningStatus>().Insert(ws);
            }
            message = "操作成功";
            return true;
        }

        public bool RemoveUserFocus(int adminUserID, int typeID, ref string message)
        {
            var userFocus = _repoFactory.CreateRepository<UserFocus>().Entities.Where(m => m.TypeID == typeID && m.UserID == adminUserID).FirstOrDefault();
            if (userFocus == null)
            {
                message = "操作失败！无该数据！";
                return false;
            }
            _repoFactory.CreateRepository<UserFocus>().Delete(userFocus);
            message = "操作成功！";
            return true;
        }

        public FW.Core.Helper.PagedList<Entity.Warning.UserFocus> GetUserFocus(int? adminUserID, int? typeID, int pageIndex, int pageSize)
        {
            return new PagedList<UserFocus>(_repoFactory.CreateRepository<UserFocus>()
                .Entities
                .Where(m => (adminUserID == null || m.UserID == adminUserID) &&
                (typeID == null || m.TypeID == typeID)
                ).OrderByDescending(m => m.Id), pageIndex, pageSize);
        }

        public FW.Core.Helper.PagedList<Entity.Warning.WarningLog> GetWarningLog(int? adminUserID, int? typeID, DateTime? begin, DateTime? end, int pageIndex, int pageSize)
        {
            var warningLog = _repoFactory.CreateRepository<WarningLog>()
                .Entities;
            var userFocus = _repoFactory.CreateRepository<UserFocus>()
                .Entities
                .Where(m=>(adminUserID==null||m.UserID==adminUserID))
                .ToList();
            List<int> typeIDList = new List<int>();
            foreach (var item in userFocus)
            {
                typeIDList.Add(item.TypeID);
            }
            warningLog = warningLog.Where(m=>(typeIDList.Contains(m.TypeID)&&
                (typeID==null||m.TypeID==typeID)&&
                (begin==null||m.CreateTime>begin)&&
                (end==null||m.CreateTime<end)))
                .OrderByDescending(m=>m.CreateTime);
            return new PagedList<WarningLog>(warningLog, pageIndex, pageSize);

        }

        public bool ExistWarning(int adminUserID, ref string messag)
        {
            var warningStatus = _repoFactory.CreateRepository<WarningStatus>()
                 .Entities
                 .Where(m => m.Id == adminUserID)
                 .FirstOrDefault();
            if (warningStatus == null || warningStatus.Status == WarningStatusStatus.No)
            {
                messag = "无报警日志！";
                return false;
            }
            else
            {
                messag = "有报警日志！";
                return true;
            }
        }

        public void ReadedWarning(int adminUserID)
        {
            var warningStatus = _repoFactory.CreateRepository<WarningStatus>()
                .Entities
                .Where(m => m.Id == adminUserID)
                .FirstOrDefault();
            if (warningStatus == null)
            {
                return;
            }
            if (warningStatus.Status == WarningStatusStatus.Yes)
            {
                warningStatus.Status = WarningStatusStatus.No;
                _repoFactory.CreateRepository<WarningStatus>().Update(warningStatus);
            }
        }

        public List<Entity.Warning.WarningType> GetWarningType()
        {
            return _repoFactory.CreateRepository<WarningType>().Entities.ToList();
        }

        public Entity.Warning.WarningType GetWarningTypeByID(int typeID)
        {
            return _repoFactory.CreateRepository<WarningType>().Entities.Where(m => m.Id == typeID).FirstOrDefault();
        }

        public bool ModifyWarningType(Entity.Warning.WarningType warningType, ref string message)
        {
            var model = _repoFactory.CreateRepository<WarningType>().Entities.Where(m => m.Id == warningType.Id).FirstOrDefault();
            if (model == null)
            {
                message = "操作失败！无该数据！";
                return false;
            }
            _repoFactory.CreateRepository<WarningType>().Update(warningType);
            message = "操作成功！";
            return true;
        }
    }
}
