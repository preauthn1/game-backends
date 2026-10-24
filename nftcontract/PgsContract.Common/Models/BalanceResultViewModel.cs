using System;

namespace PgsTokenContract.Models
{
    public class BalanceResultViewModel
    {
        public string Address { get; set; }

        public long Balance { get; set; }

        public long BalanceOfMatic { get; set; }
    }

    public class LoginNonceViewModel
    {
        public string Message { get; set; }
    }

    public class LoginResultViewModel
    {
        public string Token { get; set; }
    }
}
