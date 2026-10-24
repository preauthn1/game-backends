using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using target.Models.Extensions;

namespace target.Models
{
    public class NumberOrBooleanModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            var valueAsString = (valueProviderResult.FirstValue ?? "").Trim();

            if (string.IsNullOrEmpty(valueAsString))
            {
                return Task.CompletedTask;
            }

            if (bool.TryParse(valueAsString, out bool result))
            {
                bindingContext.Result = ModelBindingResult.Success(result);
            }
            else if (int.TryParse(valueAsString, out int intValue) && (valueAsString == "1" || valueAsString == "0"))
            {
                if(valueAsString == "1")
                {
                    bindingContext.Result = ModelBindingResult.Success(true);
                }
                else
                {
                    bindingContext.Result = ModelBindingResult.Success(false);
                }
            }
            else
            {
                bindingContext.ModelState.TryAddModelError(
                    bindingContext.ModelName, "Invalid boolean value");
            }

            return Task.CompletedTask;
        }
    }



}
