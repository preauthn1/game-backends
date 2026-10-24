using System;
using System.Web;
using example.invalid;

namespace game.Infrastructure
{
    public static class BinFileHelper
    {

        public static DateTime? GetBinFileLastModifiedTime()
        {

            try
            {
                var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var time = File.GetLastWriteTime(location);
                return time;
            }
            catch
            {

            }

            return null;
        }

        public static string GetBinFileLastModifiedTimeStringFormat()
        {
            DateTime? date = GetBinFileLastModifiedTime();

            if (date == null)
                return string.Empty;

            return date.Value.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
