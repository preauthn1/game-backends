using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Game;
using game.Service.DAO;
using game.Infrastructure;
using game.Entity.ViewModels;
using game.Infrastructure.Extensions;
using game.Infrastructure.Helper;
using game.Entity;
using game.Infrastructure.Utility;
using game.Infrastructure.Extensions;
using System.Web;
using System.Text;
using System.Security.Cryptography;
using game.Entity.Money;
using game.Entity.Gamegame.Entity.Game;
using game.Entity.Enums;
using game.Entity.Customer;

namespace game.Service.Service
{
    public class ServerSideTriggerhNotifyService : BaseThirdPartyGameService, IServerSideTriggerhNotifyService
    {       
        public CashRepository CashRepository { get; set; }

        public AttrRepository AttrRepository { get; set; }


        /// <summary>
        /// 提示server那邊，刷新錢包金額
        /// </summary>
        /// <param name="userID"></param>
        public void TriggerRefreshWalletAmountEvent(int userID)
        {
            var queryWallet = CashRepository.GetById(userID);

            if (queryWallet == null)
                return;

            //觸發更新機制
            AttrRepository.SP_Add(userID, NotifyAttrTypeEnum.MoneyAmountChnaged, queryWallet.Amount);
        }

    }

}
