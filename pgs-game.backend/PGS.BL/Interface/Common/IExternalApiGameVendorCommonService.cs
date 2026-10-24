using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using System;

namespace target.BL.Interface
{
    public interface IExternalApiGameVendorCommonService
    {
        bool CheckIsValidGameVendorIPAddress(string ip, GameVendorTypeEnum gameVendor, bool isThrowException = true);
    }

}
