using target.Utility.Helper;
using target.Utility.Utility;
using PgsTokenContract.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using target.Utility.Extensions;
using target.Models.Models;

namespace PgsTokenContract.Services
{

    public class ContractJsonABI
    {
        public string contractName { get; set; }

        public object[] abi { get; set; }
    }

    public class JsonRPCStringResultModel : JsonRPCReceiptBaseModel
    {
        public string result { get; set; }

        public bool resultOfBoolean
        {
            get
            {
                return result == "0x0000000000000000000000000000000000000000000000000000000000000001";
            }
        }
    }

    public class JsonRPCObjectResultModel : JsonRPCReceiptBaseModel
    {
        public object result { get; set; }
    }


    public class JsonRPCReceiptResultModel : JsonRPCReceiptBaseModel
    {
        public TransactionReceiptDetailModel result { get; set; }
    }


    public class JsonRPCReceiptBaseModel
    {
        public string jsonrpc { get; set; }

        public int id { get; set; }

        public bool IsError
        {
            get
            {
                return this.error != null && this.error.code < 0;
            }
        }


        public JsonRPCErrorModel error { get; set; }
    }


    public class GetTrnsactionCountParameter
    {
        public string QueueGroup { get; set; }
        public string ProviderUrl { get; set; }
        public string Tag { get; set; }
        public string SenderAddress { get; set; }
    }


    public class JsonRPCErrorModel
    {
        public int code { get; set; }

        public string message { get; set; }
    }


    public class TransactionReceiptLogModel
    {
        public string address { get; set; }
        public List<string> topics { get; set; }
        public string data { get; set; }
        public string blockNumber { get; set; }
        public string transactionHash { get; set; }
        public string transactionIndex { get; set; }
        public string blockHash { get; set; }
        public string logIndex { get; set; }
        public bool removed { get; set; }
    }


    public class TransactionReceiptDetailModel
    {
        public string blockHash { get; set; }
        public string blockNumber { get; set; }
        public object contractAddress { get; set; }
        public string cumulativeGasUsed { get; set; }
        public string effectiveGasPrice { get; set; }
        public string from { get; set; }
        public string gasUsed { get; set; }
        public List<TransactionReceiptLogModel> logs { get; set; }
        public string logsBloom { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string status { get; set; }


        public string to { get; set; }
        public string transactionHash { get; set; }
        public string transactionIndex { get; set; }
        public string type { get; set; }
    }

}
