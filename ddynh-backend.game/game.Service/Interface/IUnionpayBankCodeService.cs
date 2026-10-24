using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.Users;
using game.Entity.Game;

namespace game.Service
{

    public interface IUnionpayBankCodeService
    {

        UnionpayBankCodeEntity GetByID(int id);


        List<UnionpayBankCodeEntity> GetByIDs(List<int> ids);


        List<UnionpayBankCodeEntity> GetByFilter(QueryUnionpayBankCodeRequestViewModel filter);


        PagedList<UnionpayBankCodeEntity> GetPagingByFilter(QueryUnionpayBankCodeRequestViewModel filter);


        CRUDResultModel Create(UnionpayBankCodeEntity model);


        CRUDResultModel Edit(UnionpayBankCodeEntity model);



    }
}
