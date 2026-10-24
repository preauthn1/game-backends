using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Web.Mvc.EmbeddedViews
{
    public class EmbeddedViewCollection : Collection<EmbeddedViewMetadata>
    {
        /// <summary>
        /// 是否包含视图
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns></returns>
        public bool Contains(string viewName)
        {
            return GetEmbeddedView(viewName) != null;
        }

        /// <summary>
        /// 查找
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns></returns>
        public EmbeddedViewMetadata GetEmbeddedView(string viewName)
        {
            if (String.IsNullOrWhiteSpace(viewName))
            {
                return null;
            }

            return this.ToList().Where(v => String.Compare(v.ViewName, viewName, true) == 0)
                .FirstOrDefault();
        }
    }

}
