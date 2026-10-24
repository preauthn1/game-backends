using System;

namespace game.WebSite.Services
{
    /// <summary>
    /// 自動產生訂單號
    /// </summary>
    public static class ThirdPartyOderIDService
    {
        /// <summary>
        /// 依照OrderID和第三方支付名稱自動產生訂單號碼
        /// </summary>
        public static string GetThirdPartyOderID(int orderID, string paymentName, int length = 10)
        {
            if (orderID == 0 || string.IsNullOrEmpty(paymentName) || length == 0)
                return string.Empty;

            int paymentNameLength = paymentName.Length;
            int lengthDiff = (length - paymentNameLength);
            string orderIdStr = orderID.ToString().PadLeft((lengthDiff > 0 ? lengthDiff : 0), '0');
            string returnString = paymentName + orderIdStr;

            if (returnString.Length > length)
                return returnString.Substring(returnString.Length - length, length);

            return returnString;
        }

        public static int GetOrderID(string thirdPartyOderID, string paymentName)
        {
            if (string.IsNullOrEmpty(paymentName) || string.IsNullOrEmpty(thirdPartyOderID))
                return 0;

            string tempID = thirdPartyOderID.Replace(paymentName, "");
            return Convert.ToInt32(tempID);
        }
    }
}