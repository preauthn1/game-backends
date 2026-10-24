using Dapper;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.Payment;
using game.Entity.SystemLog;
using game.Entity.ViewModels;
using game.Infrastructure;
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
    public class PlatformPayTypeSettingRepository : IDapperRepository<PlatformPayTypeSetting>
    {
        public override string ConnectionString
        {
            get {return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[Payment].[PlatformPayTypeSetting]"; }
        }

        public List<PayTypeManagementAmountListModel> GetManagementAmountListByPlatformId(int platformId)
        {
            //]n pܦh ĳdoSPb
            string spName = "PrWeb_GetPayTypeManagementAmountList";

            var para = new
            {
                intPlatformID = platformId
            };

            var query = ExecuteSPResultByDapper<PayTypeManagementAmountListModel>(spName , para);
            return query.ToList();
        }
        
        public void UpdatePrioirty(int payTypeID , int platformID , int priorityValue)
        {
            string spName = "PrWeb_Payment_UpdatePlatformPayTypePriority";

            var para = new
            {
                intPayTypeID = payTypeID,
                intPlatformID = platformID,
                intPriority = priorityValue,
            };

            ExecuteSPByDapper(spName, para);
            return;
        }

        /// <summary>
        /// oOGsYSPIs |b{XIs
        /// Lng椸
        /// </summary>
        /// <param name="payTypeByClient">eݶǨӪpayTypeA򥻤W</param>
        /// <param name="platformId"></param>
        /// <returns></returns>
        public RandomGatewayResultModel GetRandomGatewayByPriority(game.Entity.Pay.PayType patType , int platformId , long amount)
        {
            string spName = "PrPr_RndPayGateway";
            
            var para = new DynamicParameters();
            para.Add("intTypeID", patType);
            para.Add("intPlatformID", platformId);
            para.Add("intAmount", amount);
            para.Add("retIntGWID", dbType: DbType.Int32, direction: ParameterDirection.Output);
            para.Add("retIntTypeID", dbType: DbType.Int32, direction: ParameterDirection.Output);


            using(var conn = new SqlConnection(ConnectionString))
            {
                var result = conn.Query(spName, para , commandType: CommandType.StoredProcedure);

                var resultModel = new RandomGatewayResultModel();
                resultModel.GWID = para.Get<int>("retIntGWID");
                resultModel.TypeID = para.Get<int>("retIntTypeID");

                return resultModel;
            }
        }


    }


}
