using System;
using System.Text;

namespace Service.Helper
{
    public class Helper
    {
    }

    public class DemoRandom
    {
        public static int GetRandomMumber(int begin = 0, int end = 100)
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            int number = rnd.Next(begin, end);
            return number;
        }

        public static string GetRandomString(int length)
        {
            var str = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var next = new Random();
            var builder = new StringBuilder();
            for (var i = 0; i < 5; i++)
            {
                builder.Append(str[next.Next(0, str.Length)]);
            }
            return builder.ToString();
        }
    }
}
