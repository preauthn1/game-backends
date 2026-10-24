using System;
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
using target.Models.Models;
using target.Models.CultureText;
using target.Models.Enums;
using target.Utility.Utility;

namespace target.Utility.Helper
{

    public static class MySwaggerApiDocumentHelper
    {
       
        public static MySwaggerApiDocumentJsonModel GetAllApis(string apiDomain, string apiUrl)
        {
            var res = APIRequestHelper.RequestApiResult<MySwaggerApiDocumentJsonModel>(new RequestParameters
            {
                BaseUrl = apiDomain,
                ApiUrl = apiUrl,
                Method = HttpMethodType.HttpGet
            });

            return res.ResultObject;
        }

    }


    public class MySwaggerApiDocumentJsonModel
    {
        public Dictionary<string , MySwaggerApiDocumentDetailPathModel> paths { get; set; }
    }


    public class MySwaggerApiDocumentDetailPathModel
    {

        public bool IsHttpGet
        {
            get
            {
                return this.get != null;
            }
        }

        public bool IsHttpPost
        {
            get
            {
                return this.post != null;
            }
        }

        public MySwaggerApiDocumentDetailHttpMethodModel get { get; set; }

        public MySwaggerApiDocumentDetailHttpMethodModel post { get; set; }
    }


    public class MySwaggerApiDocumentDetailHttpMethodModel
    {
        public string summary { get; set; }

        public string description { get; set; }
    }

}
