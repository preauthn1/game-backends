using target.Models;
using target.Models.Models;
using target.Models.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Text;
using target.Utility.Extensions;
using target.Models.ViewModels;

namespace target.Utility.Helper
{
    public class MyJwtHelper
    {

        public static PlayerJwtGenerateResultModel GeneratePlayerJwt(string address , string lang = "en-us")
        {
            //dev環境 token過期時間延長
            int extraRate = SettingHelper.IsDev ? 10 : 1;

            var jwtModel = new PlayerTokenJwtModel
            {
                TokenVersion = SystemConstant.JwtTokenVersionPlayer,
                RndID = Guid.NewGuid().ToString(),
                LoginName = address,
                Lang = lang,
                TokenExpiredTime = DateTimeHelper.GetCurrentTimeTimestamp() + SystemConstant.JwtToken_Expired_Seconds * extraRate
            };

            jwtModel.SignKey = GeneratePlayerSignKey(jwtModel);

            var jwt = BaseJWTTokenHelper.Generate_JWT_Token(jwtModel);
            return new PlayerJwtGenerateResultModel
            {
                JwtToken = [REDACTED],
                JwtModel = jwtModel
            };
        }


        public static PlayerTokenJwtModel GetPlayerModelFromJwtString(string jwtTokenText)
        {
            if (string.IsNullOrWhiteSpace(jwtTokenText))
                return null;

            var jwtModel = BaseJWTTokenHelper.GetObjectFromJWT<PlayerTokenJwtModel>(jwtTokenText);

            if (jwtModel == null)
                return null;

            string expectSignKey = GeneratePlayerSignKey(jwtModel);
            CheckSignIsValid(jwtModel.SignKey , expectSignKey);

            return jwtModel;
        }
        
     
        private static void CheckSignIsValid(string originalSign, string expectSignKey)
        {
            //加密簽再驗證一次 檢查是否偽造
            if (expectSignKey != originalSign)
            {
                throw new TerproException(ApiErrorCode.Authorize_JwtTokenParsingFail);
            }
        }

        private static string GeneratePlayerSignKey(PlayerTokenJwtModel model)
        {
            return ($"{SystemConstant.JwtToken_SaltKey}_{model.TokenExpiredTime}_{model.RndID}_{model.TokenVersion}").MD5Hash();
        }

    }
}
