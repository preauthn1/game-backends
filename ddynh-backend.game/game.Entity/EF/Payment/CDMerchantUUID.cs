using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using game.Entity.Users;
using System;

namespace game.Entity.Payment
{
    public class CDMerchantUUID : BaseEntity
    {
        public int GWID { get; set; }
        public string BankCode { get; set; }
        public string Currencies { get; set; }
        public string UUID { get; set; }
        public int Systems { get; set; }
        private string _Url;
        public string Url
        {
            get
            {
                return _Url;
            }
            set
            {
                _Url = (String.IsNullOrEmpty(value)) ? "" : value;
            }
        }
        private string _InfoData;
        public string InfoData
        {
            get
            {
                return _InfoData;
            }
            set
            {
                _InfoData = (String.IsNullOrEmpty(value)) ? "" : value;
            }
        }

        private string _TradeCode;
        public string TradeCode
        {
            get
            {
                return _TradeCode;
            }
            set
            {
                _TradeCode = (String.IsNullOrEmpty(value)) ? "" : value;
            }
        }
    }
}
