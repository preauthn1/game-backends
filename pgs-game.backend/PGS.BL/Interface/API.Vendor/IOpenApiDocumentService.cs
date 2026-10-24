using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;

namespace target.BL.Interface
{

    public interface IOpenApiDocumentService
    {
        string GenerateResponseHtmlForOpenApiDocument(string merchantGuid);

        QueryMerchantInfoForVendorApiDocumentPageViewModel GetMerchantInfoByGuid(string merchantGuid);

        bool HasIONSlotKeyword(string text);
    }
}
