using game.Entity.Pay;
using game.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    public class jiuhe
    {
        public static string post(CDPayOrder order, string guid, string tradeCode)
        {
            string key = Jiuhe_Key;

            var p0_Cmd = "Buy";
            var p1_MerId = Jiuhe_mchid;
            var p2_Order = order.Id.ToString();
            var p3_Amt = (order.Amount * 100).ToString();
            var p4_Cur = "CNY";
            var p5_Pid = string.Empty;
            var p6_Pcat = string.Empty;
            var p7_Pdesc = string.Empty;
            var p8_Url = Jiuhe_NotifyUrl;
            var p9_SAF = string.Empty;
            var pa_MP = string.Empty;
            var pd_FrpId = tradeCode;
            var pr_NeedResponse = "1";
            var hmac = string.Empty;


            string sbOld = string.Concat(p0_Cmd, p1_MerId, p2_Order, p3_Amt, p4_Cur, p5_Pid, p7_Pdesc, p8_Url, p9_SAF, pa_MP, pd_FrpId, pr_NeedResponse);
            hmac = Encrypt.HmacSign(sbOld, key);

            Dictionary<string, string> param = new Dictionary<string, string>
            {
                {"p0_Cmd",p0_Cmd},
                {"p1_MerId",p1_MerId},
                {"p2_Order",HttpUtility.UrlEncode(p2_Order, Encoding.GetEncoding("gb2312"))},
                {"p3_Amt",p3_Amt},
                {"p4_Cur",p4_Cur },
                {"p5_Pid",HttpUtility.UrlEncode(p5_Pid, Encoding.GetEncoding("gb2312"))},
                {"p6_Pcat",HttpUtility.UrlEncode(p6_Pcat, Encoding.GetEncoding("gb2312")) },
                {"p7_Pdesc",HttpUtility.UrlEncode(p7_Pdesc, Encoding.GetEncoding("gb2312"))},
                {"p8_Url",p8_Url},
                {"p9_SAF",p9_SAF},
                {"pa_MP",HttpUtility.UrlEncode(pa_MP, Encoding.GetEncoding("gb2312"))},
                {"pd_FrpId",pd_FrpId},
                {"pr_NeedResponse",pr_NeedResponse}
            };

            StringBuilder sb = new StringBuilder();
            ArrayList akeys = new ArrayList(param.Keys);
            akeys.Sort();

            var entries = param.Select(d =>
                string.Format("\"{0}\": [{1}]", d.Key, string.Join(",", d.Value)));

            SiteHelper.Log(guid, string.Format("充值请求: orderid={0}&data={1}", p2_Order, "{" + string.Join(",", entries) + "}"), Jiuhe_logFile);

            foreach (string k in akeys)
            {
                string v = (string)param[k];
                if (null != v)
                {
                    sb.Append(k + "=" + v + "&");
                }

            }
            string p = sb.ToString();
            p = p + "hmac=" + hmac;

            string submiturl = Jiuhe_PayURL + p;

            return submiturl;
        }


        public static PayState call(string guid)
        {

            try
            {
                string key = Jiuhe_Key;
                string r0_Cmd = HttpContext.Current.Request["r0_Cmd"];
                string r1_Code = HttpContext.Current.Request["r1_Code"];
                string r2_TrxId = HttpContext.Current.Request["r2_TrxId"];
                string r3_Amt = HttpContext.Current.Request["r3_Amt"];
                string r4_Cur = HttpContext.Current.Request["r4_Cur"];
                string r5_Pid = HttpContext.Current.Request["r5_Pid"];
                string r6_Order = HttpContext.Current.Request["r6_Order"];
                string r7_Uid = HttpContext.Current.Request["r7_Uid"];
                string r8_MP = HttpContext.Current.Request["r8_MP"];
                string r9_BType = HttpContext.Current.Request["r9_BType"];
                string rp_PayDate = HttpContext.Current.Request["rp_PayDate"];


                Dictionary<string, string> param = new Dictionary<string, string>
                {
                    {"r0_Cmd",r0_Cmd},
                    {"r1_Code",r1_Code},
                    {"r2_TrxId",r2_TrxId},
                    {"r3_Amt",r3_Amt },
                    {"r4_Cur",r4_Cur},
                    {"r5_Pid",r5_Pid},
                    {"r6_Order",r6_Order },
                    {"r7_Uid",r7_Uid },
                    {"r8_MP",r8_MP },
                    {"r9_BType",r9_BType },
                    {"rp_PayDate",rp_PayDate }
                };

                var entries = param.Select(d =>
                  string.Format("\"{0}\": [{1}]", d.Key, string.Join(",", d.Value)));

                //紀錄callback資料
                SiteHelper.Log(guid,
                     string.Format("支付回调 [FormData] {0}", "{" + string.Join(",", entries) + "}"),
                     Jiuhe_logFile);


                //取得訂單資料
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(r6_Order));

                //檢查訂單狀態
                if (order.Status == CDOrderStatus.Paied || order.Status == CDOrderStatus.fail)
                {
                    SiteHelper.Log(guid, string.Format("訂單狀態已經結束,無法進行金幣充值"), Jiuhe_logFile);
                    return PayState.Success;
                }

                //檢查簽名是否正確
                if (checkSign(key))
                {
                    //簽名錯誤
                    SiteHelper.Log(guid, string.Format("支付回调非法签名"), Jiuhe_logFile);
                    return PayState.SignError;
                }


                if (r1_Code == "1")
                {
                    //進行金幣充值
                    string msg = "";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, r2_TrxId, string.Join(",", entries), ref msg) == true)
                    {
                        SiteHelper.Log(guid, string.Format("進行金幣充值 orderid={0} amount={1}", order.Id, order.Amount), Jiuhe_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(guid, string.Format("金幣充值错误 {0}", msg), Jiuhe_logFile);
                        return PayState.Error;
                    }
                }
                else
                {
                    //更新訂單狀態
                    Service.CDFailOrder(order.Id, r2_TrxId, "支付平台回覆失敗, " + "{" + string.Join(",", entries) + "}");
                    return PayState.Error;
                }
            }
            catch (Exception ex)
            {
                SiteHelper.Log(guid, string.Format("支付回调异常错误 {0}", ex.ToString()), Jiuhe_logFile);
                return PayState.Error;
            }
        }


        private static bool checkSign(string key)
        {
            string p1_MerId = Jiuhe_mchid;
            string r0_Cmd = HttpContext.Current.Request["r0_Cmd"];
            string r1_Code = HttpContext.Current.Request["r1_Code"];
            string r2_TrxId = HttpContext.Current.Request["r2_TrxId"];
            string r3_Amt = HttpContext.Current.Request["r3_Amt"];
            string r4_Cur = HttpContext.Current.Request["r4_Cur"];
            string r5_Pid = HttpContext.Current.Request["r5_Pid"];
            string r6_Order = HttpContext.Current.Request["r6_Order"];
            string r7_Uid = HttpContext.Current.Request["r7_Uid"];
            string r8_MP = HttpContext.Current.Request["r8_MP"];
            string r9_BType = HttpContext.Current.Request["r9_BType"];
            string rp_PayDate = HttpContext.Current.Request["rp_PayDate"];
            string hmac = HttpContext.Current.Request["hmac"];

            string sbOld = string.Concat(p1_MerId, r0_Cmd, r1_Code, r2_TrxId, r3_Amt, r4_Cur, r5_Pid, r6_Order, r7_Uid, r8_MP, r9_BType, rp_PayDate);
            string nhmac = Encrypt.HmacSign(sbOld, key);

            return hmac != nhmac;
        }
    }


    partial class Encrypt
    {
        /// <summary>
        /// MD5滲杅
        /// </summary>
        /// <param name="str">埻宎趼睫揹</param>
        /// <returns>MD5賦彆</returns>
        public static string md5(string str)
        {
            byte[] b = Encoding.Default.GetBytes(str);
            b = new MD5CryptoServiceProvider().ComputeHash(b);
            string ret = "";
            for (int i = 0; i < b.Length; i++)
                ret += b[i].ToString("x").PadLeft(2, '0');
            return ret;
        }

        /// <summary>
        /// SHA256滲杅
        /// </summary>
        /// /// <param name="str">埻宎趼睫揹</param>
        /// <returns>SHA256賦彆</returns>
        public static string SHA256(string str)
        {
            byte[] SHA256Data = Encoding.UTF8.GetBytes(str);
            SHA256Managed Sha256 = new SHA256Managed();
            byte[] Result = Sha256.ComputeHash(SHA256Data);
            return Convert.ToBase64String(Result);  //殿隙酗僅峈64趼誹腔趼睫揹
        }

        /// <summary>
        /// Hmac靡
        /// </summary>
        /// <param name="aValue">靡趼睫揹</param>
        /// <returns></returns>
        public static string HmacSign(string aValue)
        {
            return HmacSign(aValue, "");
        }
        /// <summary>
        /// Hmac靡
        /// </summary>
        /// <param name="aValue">靡趼睫揹</param>
        /// <param name="aKey">靡Key</param>
        /// <returns></returns>
        public static string HmacSign(string aValue, string aKey)
        {
            byte[] k_ipad = new byte[64];
            byte[] k_opad = new byte[64];
            byte[] keyb;
            byte[] Value;
            keyb = Encoding.UTF8.GetBytes(aKey);
            Value = Encoding.UTF8.GetBytes(aValue);

            for (int i = keyb.Length; i < 64; i++)
                k_ipad[i] = 54;

            for (int i = keyb.Length; i < 64; i++)
                k_opad[i] = 92;

            for (int i = 0; i < keyb.Length; i++)
            {
                k_ipad[i] = (byte)(keyb[i] ^ 0x36);
                k_opad[i] = (byte)(keyb[i] ^ 0x5c);
            }

            HmacMD5 md = new HmacMD5();
            md.update(k_ipad, (uint)k_ipad.Length);
            md.update(Value, (uint)Value.Length);
            byte[] dg = md.finalize();
            md.init();
            md.update(k_opad, (uint)k_opad.Length);
            md.update(dg, 16);
            dg = md.finalize();
            return toHex(dg);
        }

        private static string toHex(byte[] input)
        {
            if (input == null)
                return null;
            StringBuilder output = new StringBuilder(input.Length * 2);
            for (int i = 0; i < input.Length; i++)
            {
                int current = input[i] & 0xff;
                if (current < 16)
                    output.Append("0");
                output.Append(current.ToString("x"));
            }
            return output.ToString();
        }

        private class HmacMD5
        {
            private uint[] count;
            private uint[] state;
            private byte[] buffer;
            private byte[] Digest;

            public HmacMD5()
            {
                count = new uint[2];
                state = new uint[4];
                buffer = new byte[64];
                Digest = new byte[16];
                init();
            }

            public void init()
            {
                count[0] = 0;
                count[1] = 0;
                state[0] = 0x67452301;
                state[1] = 0xefcdab89;
                state[2] = 0x98badcfe;
                state[3] = 0x10325476;
            }

            public void update(byte[] data, uint length)
            {
                uint left = length;
                uint offset = (count[0] >> 3) & 0x3F;
                uint bit_length = (uint)(length << 3);
                uint index = 0;

                if (length <= 0)
                    return;

                count[0] += bit_length;
                count[1] += (length >> 29);
                if (count[0] < bit_length)
                    count[1]++;

                if (offset > 0)
                {
                    uint space = 64 - offset;
                    uint copy = (offset + length > 64 ? 64 - offset : length);
                    Buffer.BlockCopy(data, 0, buffer, (int)offset, (int)copy);

                    if (offset + copy < 64)
                        return;

                    transform(buffer);
                    index += copy;
                    left -= copy;
                }

                for (; left >= 64; index += 64, left -= 64)
                {
                    Buffer.BlockCopy(data, (int)index, buffer, 0, 64);
                    transform(buffer);
                }

                if (left > 0)
                    Buffer.BlockCopy(data, (int)index, buffer, 0, (int)left);

            }

            private static byte[] pad = new byte[64] {
                                                     0x80, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                                     0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                                     0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                                     0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
            public byte[] finalize()
            {
                byte[] bits = new byte[8];
                encode(ref bits, count, 8);
                uint index = (uint)((count[0] >> 3) & 0x3f);
                uint padLen = (index < 56) ? (56 - index) : (120 - index);
                update(pad, padLen);
                update(bits, 8);
                encode(ref Digest, state, 16);

                for (int i = 0; i < 64; i++)
                    buffer[i] = 0;

                return Digest;
            }

            public string md5String()
            {
                string s = "";

                for (int i = 0; i < Digest.Length; i++)
                    s += Digest[i].ToString("x2");

                return s;
            }

            #region Constants for MD5Transform routine.

            private const uint S11 = 7;
            private const uint S12 = 12;
            private const uint S13 = 17;
            private const uint S14 = 22;
            private const uint S21 = 5;
            private const uint S22 = 9;
            private const uint S23 = 14;
            private const uint S24 = 20;
            private const uint S31 = 4;
            private const uint S32 = 11;
            private const uint S33 = 16;
            private const uint S34 = 23;
            private const uint S41 = 6;
            private const uint S42 = 10;
            private const uint S43 = 15;
            private const uint S44 = 21;
            #endregion

            private void transform(byte[] data)
            {
                uint a = state[0];
                uint b = state[1];
                uint c = state[2];
                uint d = state[3];
                uint[] x = new uint[16];

                decode(ref x, data, 64);

                // Round 1
                FF(ref a, b, c, d, x[0], S11, 0xd76aa478); /* 1 */
                FF(ref d, a, b, c, x[1], S12, 0xe8c7b756); /* 2 */
                FF(ref c, d, a, b, x[2], S13, 0x242070db); /* 3 */
                FF(ref b, c, d, a, x[3], S14, 0xc1bdceee); /* 4 */
                FF(ref a, b, c, d, x[4], S11, 0xf57c0faf); /* 5 */
                FF(ref d, a, b, c, x[5], S12, 0x4787c62a); /* 6 */
                FF(ref c, d, a, b, x[6], S13, 0xa8304613); /* 7 */
                FF(ref b, c, d, a, x[7], S14, 0xfd469501); /* 8 */
                FF(ref a, b, c, d, x[8], S11, 0x698098d8); /* 9 */
                FF(ref d, a, b, c, x[9], S12, 0x8b44f7af); /* 10 */
                FF(ref c, d, a, b, x[10], S13, 0xffff5bb1); /* 11 */
                FF(ref b, c, d, a, x[11], S14, 0x895cd7be); /* 12 */
                FF(ref a, b, c, d, x[12], S11, 0x6b901122); /* 13 */
                FF(ref d, a, b, c, x[13], S12, 0xfd987193); /* 14 */
                FF(ref c, d, a, b, x[14], S13, 0xa679438e); /* 15 */
                FF(ref b, c, d, a, x[15], S14, 0x49b40821); /* 16 */

                // Round 2 
                GG(ref a, b, c, d, x[1], S21, 0xf61e2562); /* 17 */
                GG(ref d, a, b, c, x[6], S22, 0xc040b340); /* 18 */
                GG(ref c, d, a, b, x[11], S23, 0x265e5a51); /* 19 */
                GG(ref b, c, d, a, x[0], S24, 0xe9b6c7aa); /* 20 */
                GG(ref a, b, c, d, x[5], S21, 0xd62f105d); /* 21 */
                GG(ref d, a, b, c, x[10], S22, 0x2441453); /* 22 */
                GG(ref c, d, a, b, x[15], S23, 0xd8a1e681); /* 23 */
                GG(ref b, c, d, a, x[4], S24, 0xe7d3fbc8); /* 24 */
                GG(ref a, b, c, d, x[9], S21, 0x21e1cde6); /* 25 */
                GG(ref d, a, b, c, x[14], S22, 0xc33707d6); /* 26 */
                GG(ref c, d, a, b, x[3], S23, 0xf4d50d87); /* 27 */
                GG(ref b, c, d, a, x[8], S24, 0x455a14ed); /* 28 */
                GG(ref a, b, c, d, x[13], S21, 0xa9e3e905); /* 29 */
                GG(ref d, a, b, c, x[2], S22, 0xfcefa3f8); /* 30 */
                GG(ref c, d, a, b, x[7], S23, 0x676f02d9); /* 31 */
                GG(ref b, c, d, a, x[12], S24, 0x8d2a4c8a); /* 32 */

                // Round 3
                HH(ref a, b, c, d, x[5], S31, 0xfffa3942); /* 33 */
                HH(ref d, a, b, c, x[8], S32, 0x8771f681); /* 34 */
                HH(ref c, d, a, b, x[11], S33, 0x6d9d6122); /* 35 */
                HH(ref b, c, d, a, x[14], S34, 0xfde5380c); /* 36 */
                HH(ref a, b, c, d, x[1], S31, 0xa4beea44); /* 37 */
                HH(ref d, a, b, c, x[4], S32, 0x4bdecfa9); /* 38 */
                HH(ref c, d, a, b, x[7], S33, 0xf6bb4b60); /* 39 */
                HH(ref b, c, d, a, x[10], S34, 0xbebfbc70); /* 40 */
                HH(ref a, b, c, d, x[13], S31, 0x289b7ec6); /* 41 */
                HH(ref d, a, b, c, x[0], S32, 0xeaa127fa); /* 42 */
                HH(ref c, d, a, b, x[3], S33, 0xd4ef3085); /* 43 */
                HH(ref b, c, d, a, x[6], S34, 0x4881d05); /* 44 */
                HH(ref a, b, c, d, x[9], S31, 0xd9d4d039); /* 45 */
                HH(ref d, a, b, c, x[12], S32, 0xe6db99e5); /* 46 */
                HH(ref c, d, a, b, x[15], S33, 0x1fa27cf8); /* 47 */
                HH(ref b, c, d, a, x[2], S34, 0xc4ac5665); /* 48 */

                // Round 4
                II(ref a, b, c, d, x[0], S41, 0xf4292244); /* 49 */
                II(ref d, a, b, c, x[7], S42, 0x432aff97); /* 50 */
                II(ref c, d, a, b, x[14], S43, 0xab9423a7); /* 51 */
                II(ref b, c, d, a, x[5], S44, 0xfc93a039); /* 52 */
                II(ref a, b, c, d, x[12], S41, 0x655b59c3); /* 53 */
                II(ref d, a, b, c, x[3], S42, 0x8f0ccc92); /* 54 */
                II(ref c, d, a, b, x[10], S43, 0xffeff47d); /* 55 */
                II(ref b, c, d, a, x[1], S44, 0x85845dd1); /* 56 */
                II(ref a, b, c, d, x[8], S41, 0x6fa87e4f); /* 57 */
                II(ref d, a, b, c, x[15], S42, 0xfe2ce6e0); /* 58 */
                II(ref c, d, a, b, x[6], S43, 0xa3014314); /* 59 */
                II(ref b, c, d, a, x[13], S44, 0x4e0811a1); /* 60 */
                II(ref a, b, c, d, x[4], S41, 0xf7537e82); /* 61 */
                II(ref d, a, b, c, x[11], S42, 0xbd3af235); /* 62 */
                II(ref c, d, a, b, x[2], S43, 0x2ad7d2bb); /* 63 */
                II(ref b, c, d, a, x[9], S44, 0xeb86d391); /* 64 */

                state[0] += a;
                state[1] += b;
                state[2] += c;
                state[3] += d;

                for (int i = 0; i < 16; i++)
                    x[i] = 0;
            }

            #region encode - decode
            private void encode(ref byte[] output, uint[] input, uint len)
            {
                uint i, j;
                if (System.BitConverter.IsLittleEndian)
                {
                    for (i = 0, j = 0; j < len; i++, j += 4)
                    {
                        output[j] = (byte)(input[i] & 0xff);
                        output[j + 1] = (byte)((input[i] >> 8) & 0xff);
                        output[j + 2] = (byte)((input[i] >> 16) & 0xff);
                        output[j + 3] = (byte)((input[i] >> 24) & 0xff);
                    }
                }
                else
                {
                    for (i = 0, j = 0; j < len; i++, j += 4)
                    {
                        output[j + 3] = (byte)(input[i] & 0xff);
                        output[j + 2] = (byte)((input[i] >> 8) & 0xff);
                        output[j + 1] = (byte)((input[i] >> 16) & 0xff);
                        output[j] = (byte)((input[i] >> 24) & 0xff);
                    }
                }
            }

            private void decode(ref uint[] output, byte[] input, uint len)
            {
                uint i, j;
                if (System.BitConverter.IsLittleEndian)
                {
                    for (i = 0, j = 0; j < len; i++, j += 4)
                        output[i] = ((uint)input[j]) | (((uint)input[j + 1]) << 8) |
                            (((uint)input[j + 2]) << 16) | (((uint)input[j + 3]) << 24);
                }
                else
                {
                    for (i = 0, j = 0; j < len; i++, j += 4)
                        output[i] = ((uint)input[j + 3]) | (((uint)input[j + 2]) << 8) |
                            (((uint)input[j + 1]) << 16) | (((uint)input[j]) << 24);
                }
            }
            #endregion

            private uint rotate_left(uint x, uint n)
            {
                return (x << (int)n) | (x >> (int)(32 - n));
            }

            #region F, G, H and I are basic MD5 functions.
            private uint F(uint x, uint y, uint z)
            {
                return (x & y) | (~x & z);
            }

            private uint G(uint x, uint y, uint z)
            {
                return (x & z) | (y & ~z);
            }

            private uint H(uint x, uint y, uint z)
            {
                return x ^ y ^ z;
            }

            private uint I(uint x, uint y, uint z)
            {
                return y ^ (x | ~z);
            }
            #endregion

            #region  FF, GG, HH, and II transformations for rounds 1, 2, 3, and 4.
            private void FF(ref uint a, uint b, uint c, uint d, uint x, uint s, uint ac)
            {
                a += F(b, c, d) + x + ac;
                a = rotate_left(a, s) + b;
            }

            private void GG(ref uint a, uint b, uint c, uint d, uint x, uint s, uint ac)
            {
                a += G(b, c, d) + x + ac;
                a = rotate_left(a, s) + b;
            }

            private void HH(ref uint a, uint b, uint c, uint d, uint x, uint s, uint ac)
            {
                a += H(b, c, d) + x + ac;
                a = rotate_left(a, s) + b;
            }

            private void II(ref uint a, uint b, uint c, uint d, uint x, uint s, uint ac)
            {
                a += I(b, c, d) + x + ac;
                a = rotate_left(a, s) + b;
            }
            #endregion
        }
    }
}