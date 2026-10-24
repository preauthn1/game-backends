using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Infrastructure.Helper
{
    public class CustomSqlHelper
    {

        public static DataTable ConvertListIntToDatableValueType(List<int> values)
        {
            //SP中有使用到自定義的Table Value type
            var dt = new DataTable("dbo.ListInt");
            dt.Columns.Add("[Value]", typeof(int));

            foreach (var id in values)
            {
                dt.Rows.Add(id);
            }
            return dt;
        }


    }
}
