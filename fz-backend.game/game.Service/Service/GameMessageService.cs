using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Repository;
using game.Entity.Game;
using game.Infrastructure;

namespace game.Service
{
    public class GameMessageService : IGameMessageService
    {
        private IRepositoryFactory _repoFactory;
        private IDatabaseFactory _dbFactory;
        public GameMessageService(IRepositoryFactory repoFactory, IDatabaseFactory dbFactory)
        {
            _repoFactory = repoFactory;
            _dbFactory = dbFactory;
        }


        public List<GameMessage> GetGameMessage(int adminID, int? platformids)
        {
            SqlParameter[] paras = {
                                   new SqlParameter("@intAdminID", adminID),
                                   new SqlParameter("@intPlatformID", platformids)
                                   };
            var _list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<GameMessage>("dbo.PrWeb_Game_GetGameMessage", paras).ToList();
            return _list;
        }

        public bool AddGameMessage(GameMessage msg, int adminID, ref string message)
        {
            /*
            intAdminID		int,
            intPlatformID	int,
            chvMessageContent varchar(512),
            dtBeginTime	datetime,
            dtEndTime		datetime=null,
            intInverval	int,
            tnyStatus		tinyint,
            chvErrMsg varchar(128) OUTPUT
             */

            var chvErrMsg = new SqlParameter
            {
                ParameterName = "@chvErrMsg",
                Value = message,
                Direction = ParameterDirection.Output,
                Size = 32
            };

            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminID",adminID),
                                    new SqlParameter("@intPlatformID",msg.PlatformID),
                                    new SqlParameter("@chvMessageContent",msg.MsgContent),
                                    new SqlParameter("@dtBeginTime",msg.BeginTime),
                                    new SqlParameter("@dtEndTime",msg.EndTime),
                                    new SqlParameter("@intInverval",msg.Interval),
                                    new SqlParameter("@tnyStatus",msg.Status),
                                    new SqlParameter("@tnyAllPlatform",msg.AllPlatform),
                                    chvErrMsg
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Game_AddGameMessage", paras);
            message = chvErrMsg.Value.ToString();
            if (returnValue == 1) message = "操作成功";

            return returnValue == 1;
        }

        public bool DelGameMessage(int msgID, ref string message)
        {
            var msg = GetGameMessageByID(msgID);
            if (msg == null)
            {
                message = "操作失败:消息不存在";
                return false;
            }

            _repoFactory.CreateRepository<GameMessage>().Delete(msg);
            message = "操作成功";
            return true;
        }

        public GameMessage GetGameMessageByID(int msgID)
        {
            return _repoFactory.CreateRepository<GameMessage>().GetById(msgID);
        }

        public bool ModifyGameMessage(GameMessage msg, int adminID, ref string message)
        {
            if (GetGameMessageByID(msg.Id) == null)
            {
                message = "操作失败:消息不存在";
                return false;
            }


            var chvErrMsg = new SqlParameter
            {
                ParameterName = "@chvErrMsg",
                Value = message,
                Direction = ParameterDirection.Output,
                Size = 32
            };

            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminID",adminID),
                                    new SqlParameter("@intId",msg.Id),
                                    new SqlParameter("@intPlatformID",msg.PlatformID),
                                    new SqlParameter("@chvMessageContent",msg.MsgContent),
                                    new SqlParameter("@dtBeginTime",msg.BeginTime),
                                    new SqlParameter("@dtEndTime",msg.EndTime),
                                    new SqlParameter("@intInverval",msg.Interval),
                                    new SqlParameter("@tnyStatus",msg.Status),
                                    new SqlParameter("@tnyAllPlatform",msg.AllPlatform),
                                    chvErrMsg
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Game_UpdGameMessage", paras);
            message = chvErrMsg.Value.ToString();
            if (returnValue == 1) message = "操作成功";
            return returnValue == 1;
        }
    }
}
