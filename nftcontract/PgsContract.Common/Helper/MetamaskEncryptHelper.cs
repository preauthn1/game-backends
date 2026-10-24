using Nethereum.ABI;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.TransactionManagers;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using target.Utility.Utility;
using PgsTokenContract.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgsTokenContract.Services
{
    public class MetamaskEncryptHelper
    {
        public static bool IsSignatureMatchAccount(string address , string message , string signature)
        {
            var signer = new Nethereum.Signer.EthereumMessageSigner();

            try
            {
                var expectedAccount = signer.EcRecover(Encoding.UTF8.GetBytes(message), signature)?.ToLower();
                if (expectedAccount.ToLower().Equals(address.ToLower()))
                {
                    return true;
                }
            }
            catch
            {

            }

            return false;
        }
    }
}
