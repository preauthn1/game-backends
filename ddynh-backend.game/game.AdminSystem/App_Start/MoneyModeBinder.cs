using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using game.Entity;

namespace game.AdminSystem
{
    public class MoneyModeBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (value == null || string.IsNullOrWhiteSpace(value.AttemptedValue))
            {
                return null;
            }

            var money = Convert.ToDecimal(value.AttemptedValue) * ConfigData.CoinShowRate;
            return Convert.ToInt64(money.ToString("f0"));
        }
    }


    public class MoneyAttrModelBinder : DefaultModelBinder
    {
        protected override object GetPropertyValue(ControllerContext controllerContext, ModelBindingContext bindingContext, System.ComponentModel.PropertyDescriptor propertyDescriptor, IModelBinder propertyBinder)
        {
            if (propertyDescriptor.Attributes[typeof(MoneyIntAttribute)] != null)
            {
                var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
                if (value != null && !string.IsNullOrWhiteSpace(value.AttemptedValue))
                {
                    var money = Convert.ToDecimal(value.AttemptedValue) * ConfigData.CoinShowRate;
                    return Convert.ToInt64(money.ToString("f0"));
                }
            }
            return base.GetPropertyValue(controllerContext, bindingContext, propertyDescriptor, propertyBinder);

        }
    }
}