using System;
using System.Web;
using example.invalid;

namespace game.Infrastructure
{
    public static class GameMerchantHelper
    {

        public static string ConvertToAPLTextFormat(object aplValue)
        {
            var text = aplValue.ToString();

            if (text == null)
                return string.Empty;

            if (text == "{}")
                return string.Empty;

            if (text == "null")
                return string.Empty;

            return text;
        }

    }
}
