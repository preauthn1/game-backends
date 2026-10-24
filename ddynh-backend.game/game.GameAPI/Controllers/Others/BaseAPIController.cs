using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid;
using example.invalid.Http;
using System.Web.Http;

namespace game.GameAPI.Controllers
{
    public abstract class BaseAPIController : ApiController
    {

        protected List<int> ConvertTextToListValue(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<int>();

            var convert = text.Split(',').Where(x=> !string.IsNullOrEmpty(x)).Select(x => Int32.Parse(x)).ToList();
            return convert;
        }


    }
}
