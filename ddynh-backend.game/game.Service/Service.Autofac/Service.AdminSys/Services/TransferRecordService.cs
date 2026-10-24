using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Service.DAO;
using game.Infrastructure;
using game.Entity.ViewModels.Query;
using game.Entity.Game;
using game.Entity;
using game.Infrastructure.Helper;
using game.Entity.Money;
using game.Infrastructure.Extensions;


namespace game.Service.Service
{
    public class TransferRecordService : BaseService, ITransferRecordService
    {

        public TransferRecordRepository TransferRecordRepository { get; set; }


        public TransferRecordRawEntity GetByID(int id)
        {
            var query = TransferRecordRepository.GetById(id);
            return query;
        }

        public List<TransferRecordRawEntity> GetByIDs(List<int> ids)
        {
            var query = TransferRecordRepository.GetByIds(ids);
            return query;
        }

        public List<TransferRecordRawEntity> GetByFilter(QueryTransferRecordRequestViewModel filter)
        {
            var query = TransferRecordRepository.GetByFilter(filter);
            return query;
        }


        public PagedList<TransferRecordRawEntity> GetPagingByFilter(QueryTransferRecordRequestViewModel filter)
        {
            var query = TransferRecordRepository.GetPagingByFilter(filter);
            return query;
        }

    }

}
