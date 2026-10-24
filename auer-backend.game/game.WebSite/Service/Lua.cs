using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace game.WebSite
{
    /// <summary>
    /// lua格式序列化与为json字符串
    /// </summary>
    public static class Lua
    {


        public static string LuaToJson(string str) {
            str =  System.Text.RegularExpressions.Regex.Replace(str, @"\[""(?<p>[^=]*)""\]\s*=\s*", @"""${p}"":");
            return System.Text.RegularExpressions.Regex.Replace(str, @"\[(?<p>\d+)\]\s*=", @"""A_${p}"":");

        }

        public static string JsonToLua(string str) {
            str =  System.Text.RegularExpressions.Regex.Replace(str, @"(?<p2>(\{|\,)\s*)""(?<p>[^:|^\{]*)""\s*:\s*", @"${p2}[""${p}""]=");
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\[""A_(?<p>\d+)""\]", @"[${p}]");
            return System.Text.RegularExpressions.Regex.Replace(str, @"""(?<p>-?\d+)""", @"${p}");

        }
    }
}