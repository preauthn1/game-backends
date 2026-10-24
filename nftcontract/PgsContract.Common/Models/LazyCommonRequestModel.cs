using target.Utility;
using System;

namespace PgsTokenContract.Models
{
    public class LazyCommonRequestModel
    {
        public string Address { get; set; }

        public string TransactionHash { get; set; }

        public string PgsWagerID { get; set; }

        public int TokenID { get; set; }

        public int Seconds { get; set; }

        public bool IsIncludeFullInfo { get; set; }

        public bool IsThrowException { get; set; }

        public bool IsForceCreateRound { get; set; }

        public ContractConfigModel TargetContractConfig { get; set; }



    }

    public class LoginRequestModel
    {
        public string Address { get; set; }

        public string Signature { get; set; }
    }


    public class SignValidateDebugRequestModel
    {
        public string Address { get; set; }

        public string Signature { get; set; }

        public DateTime TargetTime { get; set; }
    }

}
