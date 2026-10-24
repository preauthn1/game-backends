using target.DA.Repository;
using target.Utility;
using target.Models.Table;
using System;
using System.Linq;
using target.BL.Interface;
using target.Utility.Helper;
using System.Collections.Generic;
using target.Utility.Extensions;
using example.invalid;
using target.Utility.Utility.ThirdParty;
using System.Threading;
using target.Models.Models;
using Lck.Utility.Extensions.Helper;
using target.Models.Enums;
using target.Models;

namespace target.BL.Services
{
    public class EazyTokenValidateService : BaseService, IEazyTokenValidateService
    {
        public EazyTokenLogRepository EazyTokenLogRepository { get; set; }


        public void CheckIsValidToken(string token, EazyTokenValidateRule validateRule)
        {
            validateRule = validateRule ?? new EazyTokenValidateRule();

            //萬用token
            if(token == SystemConstant.PublicAPI_EazyToken_MagicToken)
            {
                return;
            }

            var recoverToken = [REDACTED];

            if (!recoverToken.IsPgsOrderNo)
                throw new TerproException(ApiErrorCode.Forbidden_NotValidDefineValue);

            var now = DateTimeHelper.GetCurrentTime();

            //利用解析單號的機制 此防呆可以減少查詢DB
            if (validateRule.CheckExpiredHourBeforeQueryDB.HasValue)
            {
                var estimateExpiredTime = recoverToken.Time.Value.AddHours(validateRule.CheckExpiredHourBeforeQueryDB.Value);

                if (now > estimateExpiredTime)
                {
                    throw new TerproException(ApiErrorCode.Authorize_TokenExpiredAndPleaseReturnToOriginalPlatform);
                }
            }

            var query = EazyTokenLogRepository.GetById(token, isLockQuery: false);

            if (query == null)
                throw new TerproException(ApiErrorCode.Authorize_TokenExpiredAndPleaseReturnToOriginalPlatform);

            if (query.ExpiredTime < now)
                throw new TerproException(ApiErrorCode.Authorize_TokenExpiredAndPleaseReturnToOriginalPlatform);

            if (!string.IsNullOrWhiteSpace(validateRule.ValidateValueMatch))
            {
                if (query.ReferenceValue?.Trim() != validateRule.ValidateValueMatch.Trim())
                {
                    throw new TerproException(ApiErrorCode.ThirdPartyApiFail_SignCheckNotMatch);
                }
            }
        }

    }



}
