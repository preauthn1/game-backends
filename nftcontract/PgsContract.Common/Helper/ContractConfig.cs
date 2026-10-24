using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using target.Utility.Extensions;
using example.invalid;
using example.invalid;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.Runtime.Versioning;
using target.Utility.Helper;
using target.Models;
using System.Threading;
using System.Collections.Concurrent;

namespace target.Utility
{

    public static class ContractConfig
    {

        public static bool IsSystemEnable = true;

        public static string DefaultTransactionTag = "latest"; // latest 或 pending

        //其他注意事項...
        //pending 疑似會出現...nonce拿到0 ?


        public const decimal ExtraRate = 1.0M;

        public const long PlaceBetGasLimit = 3750000;

        private const long DefaultGasLimit = 1000000;

        private static long DefaultGasPrice
        {
            get
            {
                return 80000000000; //部分鏈會有最低限制 避免垃圾交易
            }
        }

        //250000000000
        //130000000000
        //100000000000
        //65000000000  65 wei
        //50000000000
        //2500000000  2.5 wei
        //qqqqqqqqqqqq


        static ContractConfig()
        {
            //88 slots
            SlotsLogicContract = new ContractConfigModel
            {
                ContractAddress = SettingHelper.ContractAddress_SlotGame,

                //不太會調整的
                ChainID = SettingHelper.Contract_ProviderChainID,
                ProviderURL = SettingHelper.Contract_ProviderUrl,
                ABI_FILE_Name = "betContractABI.json",
                Gas = DefaultGasLimit,
                GasPrice = DefaultGasPrice
            };

            //龍虎鬥
            //龍虎鬥
            //龍虎鬥
            //龍虎鬥
            // 0x0e7d2a20 = createRound
            // 0xb338559e = playerBet
            // 0x20c834fd = settle
            DragonTigerLogicContract = new ContractConfigModel
            {
                ContractAddress = SettingHelper.ContractAddress_DragonTiger,

                //不太會調整的
                ChainID = SettingHelper.Contract_ProviderChainID,
                ProviderURL = SettingHelper.Contract_ProviderUrl,
                ABI_FILE_Name = "dragonTigerContractABI.json",
                Gas = DefaultGasLimit,
                GasPrice = DefaultGasPrice
            };


            PgsTokenContract = new ContractConfigModel
            {
                ContractAddress = SettingHelper.ContractAddress_PgsToken,

                //不太會調整的
                ChainID = SettingHelper.Contract_ProviderChainID,
                ProviderURL = SettingHelper.Contract_ProviderUrl,
                ABI_FILE_Name = "tokenContractABI.json",
                Gas = DefaultGasLimit,
                GasPrice = DefaultGasPrice
            };

            NFTContract = new ContractConfigModel
            {
                ContractAddress = SettingHelper.ContractAddress_NFT,

                //不太會調整的
                ChainID = SettingHelper.Contract_ProviderChainID,
                ProviderURL = SettingHelper.Contract_ProviderUrl,
                ABI_FILE_Name = "nftContractABI.json",
                Gas = DefaultGasLimit,
                GasPrice = DefaultGasPrice
            };

            NFTContractDragonTiger = new ContractConfigModel
            {
                ContractAddress = SettingHelper.ContractAddress_NFTDragonTiger,

                //不太會調整的
                ChainID = SettingHelper.Contract_ProviderChainID,
                ProviderURL = SettingHelper.Contract_ProviderUrl,
                ABI_FILE_Name = "nftContractDragonTigerABI.json",
                Gas = DefaultGasLimit,
                GasPrice = DefaultGasPrice
            };

            AuthorizeSender = new Dictionary<string, AuthorizeSenderConfigModel>();


            AddAuthorizeWallet(
                "0x4042ec3d2f89963671100d384706c9Cb771DE4AF",
                "d67d4091a48dd2dfee77b41fbc6fc364d61c8699a85fc837eb484e843e7fcb36",
                SystemConstant.QueueGroup_Common
            );

            AddAuthorizeWallet(
                "0xD7365a50C63C65c2F53061c5C7e01ACEE9FB4949",
                "7511bfdfc7e0519663c9557c2e9d8d5f354685f4814c01a007781cf934275d3c",
                SystemConstant.QueueGroup_Common
            );

            AddAuthorizeWallet(
                "0x31835E7cc0b8269b6264933CA48cA1E85dEB92C8",
                "78fc097f079d673a2038876b8c1fef3868969036d92cb3ecc2397e44869bc88d",
                SystemConstant.QueueGroup_Common
            );

            AddAuthorizeWallet(
                "0x7b9c8e20d74229f5D484A4C131f06d36a48328eA",
                "351d47795dafc3e683cd0f79a590dfb06c35452634ac6593d667c019ced0d530",
                SystemConstant.QueueGroup_Common
            );



            // ----------------------------------
            // ----------------------------------
            // create round
            // ----------------------------------
            // ----------------------------------

            AddAuthorizeWallet(
               "0x6212a8a54F7Da4F2CdAF3416bAa1934BB306123C",
               "e19373b7b46b2c9a5bd78ed47f59715145c10200d1c90b6852d3d443be06fdea",
               SystemConstant.QueueGroup_CreateRound
            );

            AddAuthorizeWallet(
                 "0x9FE02798CBD057C1EF5846a75d7AAF01889F0e8B",
                 "bd1c9f0942c02a383deda0296a8a17b734f29daf9969a4c2aa5d556dff427b62",
                 SystemConstant.QueueGroup_CreateRound
            );

            AddAuthorizeWallet(
                 "0x53D4F3281C7b8ACAeAe4822851e2e404170f9Cde",
                 "e086bac4533d1bc3a9b034b928a7b7d6709c8713941a2a46bd9eec5b4cbf38be",
                 SystemConstant.QueueGroup_CreateRound
            );


            // ----------------------------------
            // ----------------------------------
            // settle round
            // ----------------------------------
            // ----------------------------------

            AddAuthorizeWallet(
                "0xFfc2098Cd51C058F83402A4AE13E4d46F30b30e7",
                "1cc4957bc2eaa6ba240cdf708164013a056d84281544e631e984cf25c41b373f",
                SystemConstant.QueueGroup_SettleRound
            );

            AddAuthorizeWallet(
               "0x08a38993D95db45adB592d6F77bE40BaF582bd0E",
               "bcb1d2709a8069f6b23f5e0f8e40afaae4c895cb2438573364bd0b024063acb5",
               SystemConstant.QueueGroup_SettleRound
            );

            AddAuthorizeWallet(
               "0x0999826a842a41B3ad4D6251cF234251086ad318",
               "27e693172201054dfb47a42927829108ea8619d43222922f1ee84b77a5322492",
               SystemConstant.QueueGroup_SettleRound
            );

            AuthorizeSenderQueueFactory.QueueDicMapping.Add(SystemConstant.QueueGroup_Common, new ConcurrentQueue<string>());
            AuthorizeSenderQueueFactory.QueueDicMapping.Add(SystemConstant.QueueGroup_CreateRound, new ConcurrentQueue<string>());
            AuthorizeSenderQueueFactory.QueueDicMapping.Add(SystemConstant.QueueGroup_SettleRound, new ConcurrentQueue<string>());

            //將所有地址加入queue工廠
            foreach (var item in AuthorizeSender)
            {
                var myValue = item.Value;
                AuthorizeSenderQueueFactory.QueueDicMapping[myValue.QueueGroup].Enqueue(item.Key);
            }
        }

        private static void AddAuthorizeWallet(string address, string privateKey, string group)
        {
            AuthorizeSender.Add(address, new AuthorizeSenderConfigModel
            {
                SenderAddress = address,
                SenderPrivateKey = privateKey,
                QueueGroup = group
            });
        }

        public static string DefaultSenderAddress = "0x4042ec3d2f89963671100d384706c9Cb771DE4AF";

        public static Dictionary<string, AuthorizeSenderConfigModel> AuthorizeSender;
        public static ContractConfigModel SlotsLogicContract;
        public static ContractConfigModel DragonTigerLogicContract;
        public static ContractConfigModel PgsTokenContract;
        public static ContractConfigModel NFTContract;
        public static ContractConfigModel NFTContractDragonTiger;
    }


}
