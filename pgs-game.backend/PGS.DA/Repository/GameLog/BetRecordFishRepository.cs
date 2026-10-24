using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.DA.Repository
{
    public class BetRecordFishRepository : FormatBetRecordRepository<BetRecordFishEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.GameLogDB); }
        }

        protected override string TableName
        {
            get { return "[ThirdParty].[BetRecordFish]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "BetRecordNoPGS"; }
        }

        protected override GameVendorTypeEnum GameVendor
        {
            get
            {
                return GameVendorTypeEnum.CrazyFishing;
            }
        }

        //有部分實作都放在 FormatBetRecordRepository 共用了
        //有部分實作都放在 FormatBetRecordRepository 共用了
        //有部分實作都放在 FormatBetRecordRepository 共用了
        //有部分實作都放在 FormatBetRecordRepository 共用了
        //有部分實作都放在 FormatBetRecordRepository 共用了
        //有部分實作都放在 FormatBetRecordRepository 共用了

        public void UpdateToSettlementStatus(BetRecordFishEntity updateEntity)
        {
            base.ExecuteUpdateToSettlementStatus(updateEntity);
        }

    }


}
