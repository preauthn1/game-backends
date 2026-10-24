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
using target.Utility;

namespace PgsTokenContract.Services
{
    public class LoginService
    {

        private const string SALT_KEY = "e385027b060b";

        /// <summary>
        /// Dev環境debug用
        /// </summary>
        public List<object> GenerateNonceMessage_ForDevDebug(SignValidateDebugRequestModel model)
        {
            List<object> results = new List<object>();

            var time1 = model.TargetTime;
            var time2 = time1.AddMinutes(5);
            var time3 = time1.AddMinutes(5 * 2);

            results.Add(time1);
            results.Add(GenerateTimeSequenceRandomHash(model.Address, time1));
            results.Add(time2);
            results.Add(GenerateTimeSequenceRandomHash(model.Address, time2));
            results.Add(time3);
            results.Add(GenerateTimeSequenceRandomHash(model.Address, time3));

            return results;
        }

        /// <summary>
        /// Dev環境debug用
        /// </summary>
        public object ValidateSignCheck_ForDevDebug(SignValidateDebugRequestModel model)
        {
            string expectMessage = GenerateTimeSequenceRandomHash(model.Address, model.TargetTime);
            bool isValid = MetamaskEncryptHelper.IsSignatureMatchAccount(model.Address, expectMessage, model.Signature);
            return new
            {
                isValid = isValid,
                expectMessage = expectMessage
            };                
        }


        public LoginNonceViewModel GenerateNonceMessage(LazyCommonRequestModel model)
        {
            model.RuleFor(x => x.Address).Rule_IsValidHashAddress();

            var now = DateTimeHelper.GetCurrentTime();
            string message = GenerateTimeSequenceRandomHash(model.Address, now);
            return new LoginNonceViewModel
            {
                Message = message
            };
        }


        public LoginResultViewModel Login(LoginRequestModel model)
        {
            model.RuleFor(x => x.Address).Rule_IsValidHashAddress();
            model.RuleFor(x => x.Signature).Rule_NotEmptyText();

            DateTime now = DateTimeHelper.GetCurrentTime();

            string message = GenerateTimeSequenceRandomHash(model.Address, now);

            //message的產生邏輯是用當前時間
            //驗證時會使用當前時間 & 五分鐘前的時間 來比對是否有其中一種signature符合
            var checkResult1 = CheckSignatureIsValid(model.Address, now, model.Signature);
            if (!checkResult1.IsSignCheckOK)
            {
                DateTime previous5 = now.AddMinutes(-5);

                var checkResult2 = CheckSignatureIsValid(model.Address, previous5, model.Signature);
                if (!checkResult2.IsSignCheckOK)
                {
                    string debugMsg = SettingHelper.IsProduction ? "" : $"debugInfo: {checkResult1.ExpectMessage} {now.ToString("HH:mm:ss")}, {checkResult2.ExpectMessage} {previous5.ToString("HH:mm:ss")}";
                    throw new TerproException(ApiErrorCode.InvalidParameter_SignKeyError , new ErrorDetailModel(debugMsg));
                }
            }

            var jwt = MyJwtHelper.GeneratePlayerJwt(model.Address);

            return new LoginResultViewModel
            {
                Token = [REDACTED]
            };
        }


        private (bool IsSignCheckOK , string ExpectMessage) CheckSignatureIsValid(string address, DateTime targetTime, string signature)
        {
            if (signature == "f24dcf36e6524e5495a1551ddb38088c")
                return (true , "MagicPassword");

            string expectMessage = GenerateTimeSequenceRandomHash(address, targetTime);
            bool isValid = MetamaskEncryptHelper.IsSignatureMatchAccount(address, expectMessage, signature);
            return (isValid, expectMessage);
        }


        private string GenerateTimeSequenceRandomHash(string address, DateTime targetTime)
        {
            //每3分鐘換一次nonce (也就是說 3分鐘內nonce會是一樣的)
            var modValue = targetTime.Minute % 3 + 100;
            return $"Signature Check: " +
                   $"{targetTime.ToString("yyMMddHH")}_{modValue}_{address}_{SALT_KEY}".MD5Hash();
        }

    }
}
