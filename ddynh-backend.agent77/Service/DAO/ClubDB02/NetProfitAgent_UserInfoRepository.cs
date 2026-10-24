using FW.Core.Helper;
using FW.Core.Repository;
using game.Infrastructure;
using Repository.DTOModels;
using Repository.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace game.Service.DAO
{
    public class NetProfitAgent_UserInfoRepository : IDapperRepository<NetProfitAgent_UserInfoEFModel>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_Club02); }
        }

        public override string TableName
        {
            get { return "[dbo].[NetProfitAgent_UserInfo]"; }
        }
        
        public override string PrimaryKeyColumnName
        {
            get { return "UserID"; }
        }
        

    }


}
