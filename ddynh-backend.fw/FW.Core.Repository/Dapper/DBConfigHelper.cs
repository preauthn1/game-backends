using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FW.Core.Helper
{
    public class DBConfigHelper
    {

        public static DatabaseSettingCollection GetDBConnValues()
        {
            var section = System.Configuration.ConfigurationManager.GetSection("dbConfig");
            return section as DatabaseSettingCollection;
        }


        public static DatabaseSetting GetDBConnByName(string dbCategory)
        {
            var section = GetDBConnValues();

            if (section == null)
                return null;

            var query = section.FirstOrDefault(x => x.DatabaseName == dbCategory);

            if (query == null)
                return null;

            return query;
        }


    }

}
