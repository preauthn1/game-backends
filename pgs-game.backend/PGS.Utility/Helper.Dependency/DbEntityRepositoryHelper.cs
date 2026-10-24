using Microsoft.AspNetCore.Mvc;
using target.Models.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using target.Utility.Extensions;
using target.Utility.Helper.Dependency;
using target.Models.Table;
using target.Utility;
using target.Models.Models;
using target.Models;
using target.Utility.Helper;
using System.Reflection;
using target.Web.Modules.Filter;
using target.Utility.Utility.ThirdParty;
using target.Models.Enums;
using target.Models.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace target.Utility.Helper
{

    public class DbEntityRepositoryHelper
    {

        public static string GetPrimaryKeyColumnValueForDebug<T>(T entity) where T : class
        {
            if (entity == null)
                return "";

            try
            {
                var keyColumns = entity.GetType().GetProperties().Where(item =>
                {
                    var matchAttr = item.GetCustomAttributes<KeyAttribute>(true);

                    if (matchAttr == null || matchAttr.Count() == 0)
                        return false;

                    return true;
                })
                    .ToList();

                return $"Key={string.Join("、", keyColumns.Select(x => x.GetValue(entity)))}";
            }
            catch
            {

            }

            return "";
        }

    }




}
