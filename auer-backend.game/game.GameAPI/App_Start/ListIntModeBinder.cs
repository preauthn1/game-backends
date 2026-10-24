using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.ModelBinding;

namespace game.GameAPI
{
    public class ListIntModeBinder : IModelBinder
    {
        public bool BindModel(System.Web.Http.Controllers.HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            List<int> platformIDs = new List<int>();
            if (value == null || string.IsNullOrWhiteSpace(value.AttemptedValue))
            {
                bindingContext.Model = null;
            }
            bindingContext.Model = value.AttemptedValue.Split(',').Select(m => Convert.ToInt32(m)).ToList();
            return true;
        }
    }
}