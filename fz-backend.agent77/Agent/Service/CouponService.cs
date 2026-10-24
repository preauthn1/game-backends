using Repository;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Service
{
    public class CouponService : BaseService
    {
        private IRepositoryFactory _repoFactory;
        public CouponService()
        {
            this._repoFactory = new AgentClubFactory();
        }

        public Tuple<bool, Coupon_DailyReportModel> Exec_PrWeb_Coupon_Stat_CouponData_Get(int? intAdminID, DateTime? dtStartTime, DateTime? dtEndTime)
        {
            var chvErrMsg = new SqlParameter
            {
                ParameterName = "chvMsg",
                Direction = ParameterDirection.Output,
                Size = 250
            };

            var intTotalCouponCount = new SqlParameter
            {
                ParameterName = "intTotalCouponCount",
                DbType = DbType.Int32,
                Direction = ParameterDirection.Output
            };

            var lngTotalCouponAmount = new SqlParameter
            {
                ParameterName = "lngTotalCouponAmount",
                DbType = DbType.Int64,
                Direction = ParameterDirection.Output
            };

            var lngTotalUsedCouponAmount = new SqlParameter
            {
                ParameterName = "lngTotalUsedCouponAmount",
                DbType = DbType.Int64,
                Direction = ParameterDirection.Output
            };

            SqlParameter[] paras = {
                new SqlParameter("intAdminID",(object)intAdminID ?? DBNull.Value),
                new SqlParameter("dtStartTime",(object)dtStartTime ?? DBNull.Value),
                new SqlParameter("dtEndTime",(object)dtEndTime ?? DBNull.Value),
                chvErrMsg,
                intTotalCouponCount,
                lngTotalCouponAmount,
                lngTotalUsedCouponAmount
            };
            var model = new Coupon_DailyReportModel();
            model.DailyItemList = this._repoFactory.Database().SqlQuery<Coupon_DailyItem>(@"EXEC PrWeb_Coupon_Stat_CouponData_Get 
                    @intAdminID,
                    @dtStartTime,
                    @dtEndTime,
                    @chvMsg output,
                    @intTotalCouponCount output,
                    @lngTotalCouponAmount output,
                    @lngTotalUsedCouponAmount output",
                    paras).ToList();
            model.CouponPullCount = (Int32)intTotalCouponCount.Value;
            model.CouponPullAmount = (Int64)lngTotalCouponAmount.Value;
            model.CouponUsedAmount = (Int64)lngTotalUsedCouponAmount.Value;
            return Tuple.Create(true, model);
        }
        public Tuple<bool, List<Coupon_SearchUser>> Exec_PrWeb_Coupon_CouponDataByUserID_Get(int? intAdminID, int? intUserID)
        {
            var chvErrMsg = new SqlParameter
            {
                ParameterName = "chvMsg",
                Direction = ParameterDirection.Output,
                Size = 250
            };

            SqlParameter[] paras = {
                new SqlParameter("intAdminID",(object)intAdminID ?? DBNull.Value),
                new SqlParameter("intUserID",(object)intUserID ?? DBNull.Value),
                chvErrMsg
            };

            var result = this._repoFactory.Database().SqlQuery<Coupon_SearchUser>(@"EXEC PrWeb_Coupon_CouponDataByUserID_Get 
                    @intAdminID,
                    @intUserID,
                    @chvMsg output",
                    paras).ToList();
            return Tuple.Create(true, result);
        }

        public Tuple<bool, List<Coupon_CouponDetail>> Exec_PrWeb_Coupon_CouponDataByCode_Get(int? intAdminID, string chvCouponCode)
        {
            var chvErrMsg = new SqlParameter
            {
                ParameterName = "chvMsg",
                Direction = ParameterDirection.Output,
                Size = 250
            };

            SqlParameter[] paras = {
                new SqlParameter("intAdminID",(object)intAdminID ?? DBNull.Value),
                new SqlParameter("chvCouponCode",(object)chvCouponCode ?? DBNull.Value),
                chvErrMsg
            };

            var result = this._repoFactory.Database().SqlQuery<Coupon_CouponDetail>(@"EXEC PrWeb_Coupon_CouponDataByCode_Get 
                    @intAdminID,
                    @chvCouponCode,
                    @chvMsg output",
                    paras).ToList();
            return Tuple.Create(true, result);
        }

        public Tuple<bool, List<Coupon_CouponGenerator>> Exec_PrWeb_Coupon_CreateCouponQuantity(int? intAdminID, int? intPlatformID, long? lngAmount, int? intCodeCounts)
        {
            var chvErrMsg = new SqlParameter
            {
                ParameterName = "chvMsg",
                Direction = ParameterDirection.Output,
                Size = 250
            };

            SqlParameter[] paras = {
                new SqlParameter("intAdminID",(object)intAdminID ?? DBNull.Value),
                new SqlParameter("intPlatformID",(object)intPlatformID ?? DBNull.Value),
                new SqlParameter("lngAmount",(object)lngAmount ?? DBNull.Value),
                new SqlParameter("intCodeCounts",(object)intCodeCounts ?? DBNull.Value),
                new SqlParameter("intMultiUser",(object)0),
                new SqlParameter("intMultiCounts",(object)1),
                chvErrMsg
            };

            var result = this._repoFactory.Database().SqlQuery<Coupon_CouponGenerator>(@"EXEC PrWeb_Coupon_CreateCouponQuantity 
                    @intAdminID,
                    @intPlatformID,
                    @lngAmount,
                    @intCodeCounts,
                    @intMultiUser,
                    @intMultiCounts,
                    @chvMsg output",
                    paras).ToList();
            return Tuple.Create(true, result);
        }
    }
}
