using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace game.AdminSystem
{
    public class PlatformModeBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            List<int> platformIDs = new List<int>();
            if (value == null || string.IsNullOrWhiteSpace(value.AttemptedValue))
            {
                platformIDs = SiteHelper.GetPlatformIDs();
            }
            else
            {
                platformIDs.Add(Convert.ToInt32(value.AttemptedValue));
            }
            platformIDs.RemoveAll(m => !SiteHelper.GetPlatformIDs().Contains(m));
            return "platformIDs=" + string.Join(",", platformIDs.Select(m => m));
        }
    }
}