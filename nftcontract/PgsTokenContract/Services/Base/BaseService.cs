using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Nethereum.ABI;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.TransactionManagers;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using target.Models;
using target.Models.Models;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using target.Utility.Utility.ThirdParty;
using PgsTokenContract.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using example.invalid;
using System.Linq;
using example.invalid.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PgsTokenContract.Services
{
    public class BaseService
    {
        public static Dictionary<string, List<TransactionResultDebugInfo>> AddressTransactionRecordsMappings = new Dictionary<string, List<TransactionResultDebugInfo>>();

        public static string lastTransactionResult = "";

        private static string DefaultTransactionTag = ContractConfig.DefaultTransactionTag;

        public static ConcurrentQueue<string> DebugHistoryQueue = new ConcurrentQueue<string>();

        private object lockObject = new object();


        public class TransactionResultDebugInfo
        {
            public DateTime Time { get; set; }

            public string TxtHash { get; set; }

            public long Nonce { get; set; }

            public string MethodName { get; set; }
        }


        protected void ThrowTransactionFailException(JsonRPCStringResultModel rpcResult , TransactionSingHexResultModel signHex , string extraInfo = "")
        {
            string exceptionMsg = $"{rpcResult?.error?.message} , {signHex.SenderInfo.SenderAddress} {ConvertHexIntToNumber(signHex.SenderInfo.Nonce)}";
            string finalMsg = $"{extraInfo}，{exceptionMsg} ";

            MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
            {
                Message = finalMsg,
                TelegramChannelEnum = TelegramChannelEnum.pgs_contract_notify
            });

            throw new TerproException(ApiErrorCode.ThirdPartyApiFail_BlockChainTransactionFail, new ErrorDetailModel(finalMsg));
        }

        protected string RemoveUselessPrefix000000(string text)
        {
            text = text.Replace("00000000000000000000000000", "");
            text = text.Replace("0000000000000000000000000", "");
            text = text.Replace("000000000000000000000000", "");
            text = text.Replace("00000000000000000000000", "");
            text = text.Replace("0000000000000000000000", "");
            text = text.Replace("000000000000000000000", "");
            text = text.Replace("00000000000000000000", "");
            return text;
        }

        protected List<string> RemovePrefix0x_SplitByChunksize(string str, int chunkSize)
        {
            str = str.Replace("0x", "");
            return SplitByChunksize(str , chunkSize);
        }

        protected List<string> SplitByChunksize(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize))
                .ToList();
        }

        protected T ParseQueryStringTextToModel<T>(string resultText)
        {
            if (string.IsNullOrWhiteSpace(resultText))
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, new ErrorDetailModel(resultText));

            Dictionary<string, string> dic = new Dictionary<string, string>();

            var hexToBytes = resultText.Replace("0x", "").HexToByteArray();

            try
            {
                string rawText = Encoding.UTF8.GetString(hexToBytes).Replace("\0", "");
                var splits = rawText.Split(new string[] { "begin:" }, StringSplitOptions.None);

                if (splits.Length <= 1)
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny, "error! fail to parse model.");

                string querystring = splits[1];

                querystring = querystring.ReplaceFirstWordIfMatch('&' , "?");
                NameValueCollection keyValues = HttpUtility.ParseQueryString(querystring);

                foreach (var key in keyValues)
                {
                    string keyText = key.ToString();
                    dic.Add(keyText, keyValues[keyText]);
                }
                string jsonText = dic.ConvertToJsonString();

                return jsonText.JsonConvertToModel<T>();
            }
            catch (Exception ex)
            {
                throw;
            }
        }



        protected string ReadABIFromJsonFile(string fileName)
        {
            //var fileText = example.invalid.File.ReadAllText(ABI_FILE_PATH);
            //var convert = Newtonsoft.Json.JsonConvert.DeserializeObject<ContractJsonABI>(fileText);
            //return Newtonsoft.Json.JsonConvert.SerializeObject(convert.abi);

            var basePath = Directory.GetCurrentDirectory();

            //AppData
            var fileText = example.invalid.File.ReadAllText(basePath + $"\\AppData\\" + fileName);
            var convert = Newtonsoft.Json.JsonConvert.DeserializeObject<List<object>>(fileText);
            return Newtonsoft.Json.JsonConvert.SerializeObject(convert);
        }

        protected JsonRPCReceiptResultModel Eth_GetTransactionXXXXXXXXXXX(string providerURL, string txHash)
        {
            // 準備寫入的 data
            var postData = new
            {
                id = 1,
                jsonrpc = "2.0",
                method = "eth_getTransactionByHash",
                @params = new List<object> { txHash }
            };

            var result = APIRequestHelper.RequestApiResult<JsonRPCReceiptResultModel>(new RequestParameters
            {
                BaseUrl = providerURL,
                ApiUrl = APIRequestHelper.No_Url_Keyword,
                Method = HttpMethodType.HttpPost,

            }, postData);

            return result.ResultObject;
        }



        protected JsonRPCReceiptResultModel Eth_GetTransactionReceipt(string providerURL, string txHash)
        {
            // 準備寫入的 data
            var postData = new
            {
                id = 1,
                jsonrpc = "2.0",
                method = "eth_getTransactionReceipt",
                @params = new List<object> { txHash }
            };

            var result = APIRequestHelper.RequestApiResult<JsonRPCReceiptResultModel>(new RequestParameters
            {
                BaseUrl = providerURL,
                ApiUrl = APIRequestHelper.No_Url_Keyword,
                Method = HttpMethodType.HttpPost,

            }, postData);

            return result.ResultObject;
        }


        protected object Eth_GetLogs(string providerURL, string txHash)
        {
            // 準備寫入的 data
            var postData = new
            {
                id = 1,
                jsonrpc = "2.0",
                method = "eth_getLogs",
                //@params = new { address = txHash }

                @params = new List<object>
                {
                    new
                    {
                    //    fromBlock = "earliest", 
                    //    toBlock = "latest",

                        address = "0x68bdefa15a5eef381aa8223a32f23cd7e4bfc566",
                        topics = new List<string>
                        {
                           // "0x2233d5baa53efc9e07d738bcba2c7c6336d88dcc527e2a1a24bdd92f92b364a8"
                        }
                        //blockhash = "0x3f86ab75361bbbc7e21ddefbed5cd969dfea4982bc5357e5f1e80e25dace5998",
                    }
                }
            };

            var result = APIRequestHelper.RequestApiResult<string>(new RequestParameters
            {
                BaseUrl = providerURL,
                ApiUrl = APIRequestHelper.No_Url_Keyword,
                Method = HttpMethodType.HttpPost,

            }, postData);

            return result.ResultObject;
        }


        protected JsonRPCStringResultModel Eth_SendRawTransaction(string providerURL, string signMessageData)
        {
            var result = Eth_SendRawTransactionWihtDebugInfo(providerURL, new TransactionSingHexResultModel
            {
                SignHex = signMessageData
            });

            return result.ResultObject;
        }

        protected JsonRPCStringResultModel Eth_SendRawTransaction(string providerURL, TransactionSingHexResultModel signResult)
        {
            var result = Eth_SendRawTransactionWihtDebugInfo(providerURL, signResult);
            return result.ResultObject;
        }

        protected (ApiRequestResultModel<JsonRPCStringResultModel> ApiResult, JsonRPCStringResultModel ResultObject) Eth_SendRawTransactionWihtDebugInfo(string providerURL, TransactionSingHexResultModel signHexResult)
        {
            // 準備寫入的 data
            var postData = new
            {
                id = 1,
                jsonrpc = "2.0",
                method = "eth_sendRawTransaction",
                @params = new List<object> { signHexResult.SignHex }
            };

            var result = APIRequestHelper.RequestApiResult<JsonRPCStringResultModel>(new RequestParameters
            {
                BaseUrl = providerURL,
                ApiUrl = APIRequestHelper.No_Url_Keyword,
                Method = HttpMethodType.HttpPost,

            }, postData);

            //幫助debug的一些資訊
            if(signHexResult.SenderInfo != null && 
                result.ResultObject != null && 
                result.ResultObject.result != null &&
                result.ResultObject.error == null)
            {
                AddTransactionSuccessDebugInfo(signHexResult.SenderInfo, result.ResultObject.result);
            }

            // 如果是 replacement transaction underpriced 錯誤 
            if (result.ResponseText.Contains("underpriced") && result.ResponseText.Contains("replacement"))
            {
                CheckLastTransactionHashStatus(providerURL , signHexResult);
            }

            Enqueue($"時間:{DateTimeHelper.GetCurrentTimeYYYY_MM_DD_HH_MM_SS(8)} , {signHexResult.SenderInfo.MethodName} , SenderInfo: {signHexResult.SenderInfo.SenderAddress} , {ConvertHexIntToNumber(signHexResult.SenderInfo.Nonce)} , " + result.ResponseText);
            return (result, result.ResultObject);
        }


        private void CheckLastTransactionHashStatus(string providerURL, TransactionSingHexResultModel nonceResult)
        {
            if (nonceResult.SenderInfo == null)
                return;

            string senderAddress = nonceResult.SenderInfo.SenderAddress;

            if (AddressTransactionRecordsMappings.ContainsKey(senderAddress))
            {
                long nonceValueThisTime = ConvertHexIntToNumber(nonceResult.SenderInfo.Nonce);

                var lastTxt = AddressTransactionRecordsMappings[senderAddress].Where(x => x.Nonce == nonceValueThisTime).FirstOrDefault();

                if(lastTxt != null)
                {
                    var findReceipt = Eth_GetTransactionReceipt(providerURL, lastTxt.TxtHash);

                    if(findReceipt != null && findReceipt.result != null)
                    {
                        bool isSuccessStatus = findReceipt.result.status == SystemConstant.Hex_Value1;

                        if(!isSuccessStatus)
                        {
                            string msg = $"平台錢包: {senderAddress} 交易失敗 {nonceResult.SenderInfo.MethodName} , 因最後一筆交易: {lastTxt.TxtHash} , nonce: {ConvertHexIntToNumber(nonceResult.SenderInfo.Nonce)} 狀態還卡在未完成。可能交易緩慢或鏈不穩定。建議60秒後再做嘗試。";

                            SendNotifyToChannel(msg);

                            throw new TerproException(ApiErrorCode.BusinessLogicDeny, msg);
                        }
                    }

                    string myMsg = $"平台錢包:{senderAddress} 交易失敗 {nonceResult.SenderInfo.MethodName} , 最後一筆交易尚未處理完成 {lastTxt.TxtHash}。建議60秒後再做嘗試。";
                    SendNotifyToChannel(myMsg);
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny, myMsg);
                }
            }
        }



        protected ApiRequestResultModel<T> Eth_CallMethod_ForDebug<T>(ContractConfigModel config, string methodName, List<object> parameters)
        {
            string abiJson = ReadABIFromJsonFile(config.ABI_FILE_Name);

            var privateKey = ContractConfig.AuthorizeSender[ContractConfig.DefaultSenderAddress].SenderPrivateKey;
            var account = new Nethereum.Web3.Accounts.Account(privateKey, config.ChainID);
            var web3 = new Web3(account, config.ProviderURL);
            var contract = web3.Eth.GetContract(abiJson, config.ContractAddress);

            var fucntion = contract.GetFunction(methodName);
            string encodeData = fucntion.GetData(parameters.ToArray());

            var result = Eth_CallMethod<T>(config.ProviderURL, encodeData, config.ContractAddress, account.Address);
            return result;
        }


        protected T Eth_CallMethod<T>(ContractConfigModel config, string methodName, List<object> parameters)
        {
            string abiJson = ReadABIFromJsonFile(config.ABI_FILE_Name);

            var privateKey = ContractConfig.AuthorizeSender[ContractConfig.DefaultSenderAddress].SenderPrivateKey;
            var account = new Nethereum.Web3.Accounts.Account(privateKey, config.ChainID);
            var web3 = new Web3(account, config.ProviderURL);
            var contract = web3.Eth.GetContract(abiJson, config.ContractAddress);

            var fucntion = contract.GetFunction(methodName);
            string encodeData = fucntion.GetData(parameters.ToArray());

            var result = Eth_CallMethod<T>(config.ProviderURL, encodeData, config.ContractAddress , account.Address);
            return result.ResultObject;
        }


        protected ApiRequestResultModel<T> Eth_CallMethod<T>(string providerURL, string data, string toAddress , string fromAddress = "")
        {
            // 準備寫入的 data
            var postData = new
            {
                id = 1,
                jsonrpc = "2.0",
                method = "eth_call",
                @params = new List<object>
                {
                    new
                    {
                        data = data,
                        from = fromAddress,
                        to = toAddress
                    },
                    "latest"
                }
            };

            var result = APIRequestHelper.RequestApiResult<T>(new RequestParameters
            {
                BaseUrl = providerURL,
                ApiUrl = APIRequestHelper.No_Url_Keyword,
                Method = HttpMethodType.HttpPost,

            }, postData);

            return result;
        }

        protected JsonRPCStringResultModel Eth_GetTrnsactionCount(GetTrnsactionCountParameter param)
        {


            #region v1版本寫法

            //v1版本寫法
            //v1版本寫法
            //v1版本寫法
            //v1版本寫法
            //v1版本寫法
            //v1版本寫法
            //v1版本寫法
            //v1版本寫法

            //var accountInfo = ContractConfig.AuthorizeSender[param.SenderAddress];
            //var account = new Nethereum.Web3.Accounts.Account(accountInfo.SenderPrivateKey, ContractConfig.PgsTokenContract.ChainID);
            //var web3 = new Web3(account, param.ProviderUrl);

            //var txCount = web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(param.SenderAddress).Result;

            //return new JsonRPCStringResultModel
            //{
            //    result = txCount.ToString()
            //};

            #endregion


            //v2版本的寫法
            //v2版本的寫法
            //v2版本的寫法
            //v2版本的寫法
            //v2版本的寫法
            //v2版本的寫法
            //v2版本的寫法
            if (string.IsNullOrWhiteSpace(param.Tag))
            {
                param.Tag = "latest";
            }

            // 準備寫入的 data
            var postData = new
            {
                id = 1,
                jsonrpc = "2.0",
                method = "eth_getTransactionCount",
                @params = new List<object>
                {
                    param.SenderAddress, param.Tag
                }
            };

            var result = APIRequestHelper.RequestApiResult<JsonRPCStringResultModel>(new RequestParameters
            {
                BaseUrl = param.ProviderUrl,
                ApiUrl = APIRequestHelper.No_Url_Keyword,
                Method = HttpMethodType.HttpPost,

            }, postData);


            if(result.ResultObject == null)
            {
                SendNotifyToChannel($"Eth_GetTrnsactionCount 失敗，{param.SenderAddress} {result.ResponseText}");
            }

            return result.ResultObject;
        }


        protected HexBigInteger GetTrnsactionCountBySpecificAddress(GetTrnsactionCountParameter param)
        {
            var nonceResult = Eth_GetTrnsactionCount(param);
            return new HexBigInteger(nonceResult.result);
        }


        protected TransactionSingHexResultModel GenerateSignAndNonceByQueue(GenerateSignBundleParametersModel model)
        {
            string providerUrl = model.ContractConfig.ProviderURL;

            //先從queue取出其中一個地址來用
            var countParam = new GetTrnsactionCountParameter
            {
                ProviderUrl = providerUrl,
                Tag = DefaultTransactionTag,
                QueueGroup = model.TargetQueueGroup,
            };

            TransactionSenderResultModel queueResult = AuthorizeSenderQueueFactory.TakeAddressFromQueueAndGetTransactionCount(countParam, GetTrnsactionCountBySpecificAddress);
            queueResult.MethodName = model.MethodName;

            //如果取queue失敗 改成使用預設的地址 (demo的乘載量 用pending模式應該還是能撐)
            if (string.IsNullOrWhiteSpace(queueResult.SenderAddress))
            {
                return GenerateSignAndNonceByDefaultAddress(model);
            }

            return GenerateSignHexBySpecificAddress(model, queueResult);
        }


        protected TransactionSingHexResultModel GenerateSignAndNonceByDefaultAddress(GenerateSignBundleParametersModel model)
        {
            string defaultAddeess = ContractConfig.DefaultSenderAddress;
            var result = GenerateSignAndNonceBySpecificAddress(model , defaultAddeess);
            return result;
        }


        protected TransactionSingHexResultModel GenerateSignAndNonceBySpecificAddress(GenerateSignBundleParametersModel model , string senderAddress)
        {
            string providerUrl = model.ContractConfig.ProviderURL;

            var countParam = new GetTrnsactionCountParameter
            {
                ProviderUrl = providerUrl,
                Tag = DefaultTransactionTag,
                SenderAddress = senderAddress
            };

            var nonce = GetTrnsactionCountBySpecificAddress(countParam);
            var queueResult = new TransactionSenderResultModel
            {
                SenderAddress = senderAddress,
                MethodName = model.MethodName,
                Nonce = nonce
            };

            return GenerateSignHexBySpecificAddress(model, queueResult);
        }


        private TransactionSingHexResultModel GenerateSignHexBySpecificAddress(GenerateSignBundleParametersModel model , TransactionSenderResultModel nonceResult)
        {
            var config = model.ContractConfig;

            string providerUrl = config.ProviderURL;
            var accountInfo = ContractConfig.AuthorizeSender[nonceResult.SenderAddress];
            var account = new Nethereum.Web3.Accounts.Account(accountInfo.SenderPrivateKey, model.ContractConfig.ChainID);
            var web3 = new Web3(account, providerUrl);

            string abiJson = ReadABIFromJsonFile(config.ABI_FILE_Name);
            var contract = web3.Eth.GetContract(abiJson, config.ContractAddress);
            var fucntion = contract.GetFunction(model.MethodName);

            string encodeData = fucntion.GetData(model.Parameters.ToArray());
            var gasLimit = new HexBigInteger(model.MaxGasLimit.HasValue ? model.MaxGasLimit.Value : config.Gas);

            var input = new TransactionInput
            {
                From = accountInfo.SenderAddress,
                To = config.ContractAddress,
                Gas = gasLimit, //limit的意思
                GasPrice = new HexBigInteger((long)(config.GasPrice * ContractConfig.ExtraRate)),
                Nonce = nonceResult.Nonce,
                Value = new HexBigInteger(0),
                Data = encodeData
            };

            var sign = GenerateSignHex(account, input, config.ChainID); 
            
            return new TransactionSingHexResultModel
            {
                SignHex = sign,
                SenderInfo = nonceResult
            };
        }


        protected string GenerateSignHex(Account account, TransactionInput input, int chainID)
        {
            var signHelper = new Nethereum.Web3.Accounts.AccountOfflineTransactionSigner();
            var sign = signHelper.SignTransaction(account, input, chainID);
            return "0x" + sign;
        }

        protected long Convert64LengthHexToBigint(string length64_Hex)
        {
            return Convert.ToInt64(length64_Hex, 16);
        }

        protected string ConvertByteStringToUtf8Text(string byte32Text)
        {
            return byte32Text.HexToUTF8String().Replace("\0", "");
        }

        protected string ConvertNumberToHexString(long number)
        {
            return "0x" + number.ToString("X64");
        }

        protected long ConvertHexIntToNumber(HexBigInteger hexBigInt)
        {
            return ConvertHexToNumber(hexBigInt.ToString());
        }

        protected long ConvertHexToNumber(string hexString)
        {
            return Int64.Parse(hexString.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
        }


        /// <summary>
        /// 紀錄最後一筆成功的紀錄
        /// </summary>
        private void AddTransactionSuccessDebugInfo(TransactionSenderResultModel signSenderInfo , string txtHash)
        {
            try
            {
                string address = signSenderInfo.SenderAddress;

                if (AddressTransactionRecordsMappings == null)
                {
                    AddressTransactionRecordsMappings = new Dictionary<string, List<TransactionResultDebugInfo>>();
                }

                var newRecord = new TransactionResultDebugInfo
                {
                    Time = DateTime.Now,
                    TxtHash = txtHash,
                    MethodName = signSenderInfo.MethodName,
                    Nonce = ConvertHexIntToNumber(signSenderInfo.Nonce)
                };

                if (AddressTransactionRecordsMappings.ContainsKey(address))
                {
                    var originalValues = AddressTransactionRecordsMappings[address] ?? new List<TransactionResultDebugInfo>();
                    originalValues.Add(newRecord);
                }
                else
                {
                    var newList = new List<TransactionResultDebugInfo>();
                    newList.Add(newRecord);

                    AddressTransactionRecordsMappings.Add(address, newList);
                }
            }
            catch
            {

            }

        }

        public void Enqueue(string obj)
        {
            try
            {
                DebugHistoryQueue.Enqueue(obj);
                lock (lockObject)
                {
                    string overflow;
                    while (DebugHistoryQueue.Count > 5 && DebugHistoryQueue.TryDequeue(out overflow)) ;
                }
            }
            catch
            {

            }
        }

        public void SendNotifyToChannel(string msg , string skipNotifyKeyword = "")
        {
            //如果訊息包含指定關鍵字 就不發通知 (通常是已知情況、無意義的錯誤訊息)
            if(!string.IsNullOrWhiteSpace(skipNotifyKeyword) && msg.Contains(skipNotifyKeyword))
            {
                return;
            }

            MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
            {
                Message = msg,
                TelegramChannelEnum = TelegramChannelEnum.pgs_contract_notify
            });
        }

    }
}
