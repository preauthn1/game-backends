using Nethereum.ABI;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.TransactionManagers;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using target.Utility.Helper;
using target.Utility.Utility;
using PgsTokenContract.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using target.Utility.Extensions;
using target.Models.Models;
using static PgsTokenContract.Services.TokenContractService;
using target.Utility;
using target.Models;
using target.Utility.Extensions;
using System.Text;
using System.Web;
using System.Collections.Specialized;
using target.Models.ViewModels.Base;

namespace PgsTokenContract.Services
{
    public class BetService : BaseService
    {

        private const string SALT_KEY = "e385027b060b";


        public BetPlaceBetAndTransactionIDModel PlaceBet(BetRequestModel model)
        {
            model.RuleFor(x => x.BetAmount).Rule_NotZeroNotMinus().Rule_NotTooSmall(10);
            model.RuleFor(x => x.TokenID).Rule_NotMinus();

            var config = ContractConfig.SlotsLogicContract;
            string extraRandomSeed = Guid.NewGuid().ToString().Replace("-", "");
            string pgsWagerID = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo();

            string methodName = "playerBet";
            List<object> parameters = new List<object>
            {
                model.TokenID,
                model.Address ,
                model.BetAmount ,
                extraRandomSeed,
                pgsWagerID
            };

            var signParam = new GenerateSignBundleParametersModel(config, methodName, parameters);
            signParam.MaxGasLimit = ContractConfig.PlaceBetGasLimit;

            var sign = GenerateSignAndNonceByQueue(signParam);
            var result = Eth_SendRawTransactionWihtDebugInfo(config.ProviderURL, sign);

            string txt = result.ResultObject?.result;

            if (result.ResultObject.IsError || string.IsNullOrWhiteSpace(txt))
            {
                ThrowTransactionFailException(result.ResultObject, sign , $" PlaceBet 失敗。{pgsWagerID} ");
            }

            return new BetPlaceBetAndTransactionIDModel
            {
                TransactionHash = txt,
                DebugInfo = result.ApiResult.ResponseText
            };
        }


        public BetResultModel GetBetResult(LazyCommonRequestModel model)
        {
            model.RuleFor(x => x.TransactionHash).Rule_NotEmptyText();

            int rndNumber = RandomHelper.GetRandIntegerNumbers(1, 10);

            string pgsWagerID = model.PgsWagerID;

            if (string.IsNullOrWhiteSpace(pgsWagerID))
            {
                var config = ContractConfig.SlotsLogicContract;

                var receipt = Eth_GetTransactionReceipt(config.ProviderURL, model.TransactionHash);

                if (receipt == null || receipt.result == null)
                {
                    throw new TerproException(ApiErrorCode.NotFoundReceipt);
                }

                if (receipt.result.status == SystemConstant.Hex_Value0)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny, $"result status is error! {model.TransactionHash}");
                }

                var topic = receipt.result
                    ?.logs
                    ?.FirstOrDefault(x => x.address.ToLower() == config.ContractAddress.ToLower())
                    ?.topics;

                if (topic.IsEmptyOrNull())
                    throw new TerproException(ApiErrorCode.NotFoundData, new ErrorDetailModel("topic"));

                pgsWagerID = ConvertByteStringToUtf8Text(topic[1]);
            }

            var queryBytestResult = Eth_CallMethod<JsonRPCStringResultModel>(ContractConfig.SlotsLogicContract, "getWagerDetail", new List<object>
            {
                pgsWagerID
            });

            var parseModel = ParseQueryStringTextToModel<WagerDetailModel>(queryBytestResult.result);

            //查無資料
            if (parseModel.PlayerAddress == SystemConstant.EmptyAddressHash && parseModel.BetAmount == 0)
            {
                throw new TerproException(ApiErrorCode.NotFoundRecord);
            }

            List<string> parsingResult = new List<string>();

           // var fakeSpinResult = GenerateFakeResultByDice();

            return new BetResultModel
            {
                BetAmount = parseModel.BetAmount,
                PayoutAmount = parseModel.PayoutAmount,
                PayoutRate = parseModel.PayoutAmount / parseModel.BetAmount,
                WagerID = pgsWagerID,
                TransactionHash = model.TransactionHash,
                DebugRawInfo = new RawInfoViewModel
                {
                    LineResult = parseModel.WinlineText,
                    SpinResult = parseModel.SpinResult
                },

                SpinResult = ConvertToSpinResult(parseModel.SpinResult),
                LineResult = ConvertToLineResult(parseModel.WinlineText),
                PlayerName = parseModel.PlayerAddress,
            };
        }


        private List<List<string>> ConvertToSpinResult(string text)
        {
            List<List<string>> result = new List<List<string>>();

            try
            {
                var spliEach = text.Split('|')
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                foreach (var eachLine in spliEach)
                {
                    var splitByComma = eachLine.Split(',').ToList();

                    if (splitByComma.Count() == 5)
                    {
                        result.Add(splitByComma);
                    }
                }

            }
            catch
            {

            }

            return result;
        }



        private List<List<object>> ConvertToLineResult(string text)
        {
            List<List<object>> result = new List<List<object>>();

            try
            {
                var spliEach = text.Split('|')
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                foreach (var eachLine in spliEach)
                {
                    var splitByComma = eachLine.JsonConvertToModel<List<object>>();

                    if (splitByComma.Count() == 5)
                    {
                        result.Add(splitByComma);
                    }
                }

                foreach(var eachLineArray in result)
                {
                    //這欄存的文字有時候會多個逗號 要處理
                    eachLineArray[1] = ReformatWinlineSymbolText(eachLineArray[1]);
                }

                string qqq = "";
            }
            catch
            {

            }

            return result;
        }

        private string ReformatWinlineSymbolText(object text)
        {
            try
            {
                var availableTexts = text
                    .ToString()
                    .Split(',')
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                return string.Join("," , availableTexts);
            }
            catch
            {

            }

            return text?.ToString();
        }



        private string ConvertHexStringToUtf8Text(string hexString)
        {
            var debug1 = Encoding.UTF8.GetString(hexString.HexToByteArray());
            var debug2 = Encoding.UTF8.GetString(hexString.Replace("0x", "").HexToByteArray());

            //var hexToBytes = resultText.Replace("0x", "").HexToByteArray();
            //string rawText = Encoding.UTF8.GetString(hexToBytes).Replace("\0", "");
            //var splits = rawText.Split(new string[] { "wagerDetailBegin:" }, StringSplitOptions.None);




            return Encoding.UTF8.GetString(hexString.HexToByteArray());
        }

    
        public static class Symbol
        {
            public static string S88_Red = "s1";
            public static string S88_Purple = "s2";
            public static string S88_Blue = "s3";
            public static string S88_Green = "s4";
            public static string S1Bar = "s7";
            public static string S2Bar = "s6";
            public static string S3Bar = "s5";
            public static string SMoney = "b1";
        }


        public (List<List<string>> SpinResult, decimal payoutRate) GenerateFakeResultByDice()
        {
            int rndNumber = RandomHelper.GetRandIntegerNumbers(1, 10);

            List<string> allSymbols = new List<string>
            {
                Symbol.S88_Red,
                Symbol.S88_Purple,
                Symbol.S88_Blue,
                Symbol.S88_Green,
                Symbol.S1Bar,
                Symbol.S2Bar,
                Symbol.S3Bar,
                Symbol.SMoney,
            };

            List<List<string>> fakeResult = new List<List<string>>();

            //0倍
            var randomBar = allSymbols
                .Where(x => x != "b1")
                .ToList()
                .SortByRandom();


            decimal payoutRate = 0;

            switch (rndNumber)
            {
                case 1:
                    //23A802973295cc22427fb43c14b6bb95  1.5倍
                    payoutRate = 1.5M;
                    fakeResult = new List<List<string>>
                    {
                        Gen(Symbol.S88_Blue , Symbol.S88_Purple , Symbol.S88_Blue) ,
                        Gen(Symbol.S3Bar , Symbol.S88_Green , Symbol.S88_Blue) ,
                        Gen(Symbol.S88_Green , Symbol.S2Bar , Symbol.S88_Blue) ,
                        Gen(Symbol.S1Bar , Symbol.S1Bar , Symbol.S88_Green) ,
                        Gen(Symbol.S88_Green , Symbol.S2Bar , Symbol.S88_Red) ,
                    };
                    break;

                case 2:
                    //23A849b7f7a5c7fd42d390296d5d75c3  3倍
                    payoutRate = 3M;
                    fakeResult = new List<List<string>>
                    {
                        Gen(Symbol.S88_Green , Symbol.S2Bar , Symbol.S2Bar) ,
                        Gen(Symbol.S88_Green , Symbol.S88_Green , Symbol.S88_Blue) ,
                        Gen(Symbol.S2Bar , Symbol.S88_Green , Symbol.S1Bar) ,
                        Gen(Symbol.S88_Green , Symbol.SMoney , Symbol.S88_Red) ,
                        Gen(Symbol.S88_Green , Symbol.S1Bar , Symbol.S3Bar) ,
                    };
                    break;

                case 3:
                    //XXXXXXXXXXXXXXXXXXXXXXXXX 1.5倍  
                    //中獎線: 下排三個blue
                    payoutRate = 1.5M;
                    fakeResult = new List<List<string>>
                    {
                        Gen(Symbol.S88_Purple , Symbol.S88_Purple , Symbol.S88_Blue) ,
                        Gen(Symbol.S2Bar , Symbol.S88_Blue , Symbol.S88_Blue) ,
                        Gen(Symbol.S88_Purple , Symbol.S3Bar , Symbol.S88_Blue) ,
                        Gen(Symbol.S88_Green , Symbol.S88_Green , Symbol.S1Bar) ,
                        Gen(Symbol.S2Bar , Symbol.S88_Green , Symbol.S88_Red) ,
                    };
                    break;

                case 6:
                case 7:
                case 8:
                    {
                        var bar1 = randomBar[0];
                        var bar2 = randomBar[1];

                        var s88_1 = randomBar[3];
                        var s88_2 = randomBar[4];
                        var s88_3 = randomBar[5];
                        var s88_4 = randomBar[6];

                        fakeResult = new List<List<string>>
                        {
                            Gen(bar1 , s88_1 , s88_3) ,
                            Gen(s88_2 , s88_3 , s88_4) ,
                            Gen(s88_4 , bar2 , s88_1) ,
                            Gen(bar2 , bar1 , s88_1) ,
                            Gen(bar2 , bar1 , s88_3) ,
                        };

                        break;
                    }


                default:
                    {
                        var bar1 = randomBar[0];
                        var bar2 = randomBar[1];
                        var s88_1 = randomBar[2];
                        var s88_2 = randomBar[3];
                        var s88_3 = randomBar[4];

                        fakeResult = new List<List<string>>
                        {
                            Gen(s88_1 , s88_2 , bar2) ,
                            Gen(bar1 , bar1 , s88_3) ,
                            Gen(s88_1 , bar1 , bar1) ,
                            Gen(bar1 , bar2 , s88_2) ,
                            Gen(s88_2 , bar2 , s88_2) ,
                        };
                    }
                    break;
            }

            return (fakeResult, payoutRate);
        }

        private List<string> Gen(params string[] text)
        {
            return text.ToList();
        }


    }
}
