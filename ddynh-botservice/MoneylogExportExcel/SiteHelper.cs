using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneylogExportExcel
{
    public static class SiteHelper
    {
        public static string ToCash(this long _this)
        {
            return (_this / 100 * 1m).ToString("N2");
        }

        public static int GetTotalPage(int pageSize, int count)
        {
            if (pageSize < 1) return 0;
            if (count <= pageSize) return 1;
            int pageCount = count / pageSize;
            if (count % pageSize > 0) pageCount++;
            return pageCount;
        }
    }
}
