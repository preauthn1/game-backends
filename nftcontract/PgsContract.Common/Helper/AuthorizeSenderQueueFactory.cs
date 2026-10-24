using Nethereum.Hex.HexTypes;
using target.Models;
using PgsTokenContract.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace target.Utility
{
    public class AuthorizeSenderQueueFactory
    {
        public static Dictionary<string, ConcurrentQueue<string>> QueueDicMapping = new Dictionary<string, ConcurrentQueue<string>>();

        public static TransactionSenderResultModel TakeAddressFromQueueAndGetTransactionCount(GetTrnsactionCountParameter param , Func<GetTrnsactionCountParameter, HexBigInteger> fetchTransactionCount)
        {
            HexBigInteger resultNonce = new HexBigInteger(0);
            string takeAddress = string.Empty;

            //若沒指定要哪個群組的queue 使用預設的
            if(string.IsNullOrWhiteSpace(param.QueueGroup))
            {
                param.QueueGroup = SystemConstant.QueueGroup_Common;
            }

            var targetQueue = QueueDicMapping[param.QueueGroup];

            try
            {
                bool isOK = targetQueue.TryDequeue(out takeAddress);
                param.SenderAddress = takeAddress;
                resultNonce = fetchTransactionCount(param);
            }
            catch(Exception ex)
            {
                string msg = "";
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(takeAddress))
                {
                    targetQueue.Enqueue(takeAddress);
                }
            }

            return new TransactionSenderResultModel
            {
                Nonce = resultNonce,
                SenderAddress = takeAddress,
            };
        }
    }


    public class TransactionSingHexResultModel
    {
        public string SignHex { get; set; }

        public TransactionSenderResultModel SenderInfo { get; set; }
    }




    public class TransactionSenderResultModel
    {
        public string SenderAddress { get; set; }

        public string MethodName { get; set; }

        public HexBigInteger Nonce { get; set; }
    }


    public class GenerateSignBundleParametersModel
    {
        public GenerateSignBundleParametersModel()
        {

        }

        public GenerateSignBundleParametersModel(ContractConfigModel config , string method , List<object> p)
        {
            this.ContractConfig = config;
            this.MethodName = method;
            this.Parameters = p;
        }


        public ContractConfigModel ContractConfig { get; set; }


        public string TargetQueueGroup { get; set; }

        public string MethodName { get; set; }

        /// <summary>
        /// 若沒指定會使用預設值
        /// </summary>
        public long? MaxGasLimit { get; set; }

        public List<object> Parameters { get; set; }

    }


}
