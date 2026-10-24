using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using target.Core.Helper;
using target.Models.Attributes;
using target.Models.CoreLibs;
using target.Models.Models;
using target.Models.Paging;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Helper.HttpContext;
using target.Utility.Utility.ThirdParty;

namespace target.Core.Repository
{
    public abstract class BaseMongoRepository<T> where T : class
    {
    }

}