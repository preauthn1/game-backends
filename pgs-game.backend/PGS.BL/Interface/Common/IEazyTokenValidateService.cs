using target.Models.Models;
using target.Models.Table;
using System;

namespace target.BL.Interface
{
    public interface IEazyTokenValidateService
    {
        void CheckIsValidToken(string token, EazyTokenValidateRule validateRule);
    }

}
