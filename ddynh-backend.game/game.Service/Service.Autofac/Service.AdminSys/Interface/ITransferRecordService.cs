using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.Game;
using game.Entity.ViewModels.Query;

namespace game.Service
{

    public interface ITransferRecordService
    {

        TransferRecordRawEntity GetByID(int id);


        List<TransferRecordRawEntity> GetByIDs(List<int> ids);


        List<TransferRecordRawEntity> GetByFilter(QueryTransferRecordRequestViewModel filter);


        PagedList<TransferRecordRawEntity> GetPagingByFilter(QueryTransferRecordRequestViewModel filter);



    }
}
