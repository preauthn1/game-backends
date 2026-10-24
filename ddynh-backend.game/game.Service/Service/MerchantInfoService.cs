using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.Event;
using game.Entity.Payment;
using game.Entity.Users;
using game.Infrastructure;
using game.Service.DAO;
using game.Infrastructure.Extensions;
using game.Entity.Game;
using game.Entity.Money;

namespace game.Service.Service
{
    public class MerchantInfoService : BaseService, IMerchantInfoService
    {

        public MerchantInfoRepository MerchantInfoRepository { get; set; }


        public MerchantInfoEntity GetByID(int id)
        {
            var query = MerchantInfoRepository.GetById(id);
            return query;
        }

        public List<MerchantInfoEntity> GetByIDs(List<int> ids)
        {
            var query = MerchantInfoRepository.GetByIds(ids);
            return query;
        }

        public List<MerchantInfoEntity> GetByFilter(QueryMerchantInfoRequestViewModel filter)
        {
            var query = MerchantInfoRepository.GetByFilter(filter);
            return query;
        }

    }

}
