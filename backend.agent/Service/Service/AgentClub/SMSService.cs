using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Repository;

namespace Service.Service.AgentClub
{
    public class SMSService
    {
        private AgentClubFactory _repoFactory = new AgentClubFactory();

        public bool CresteSMS(string vCode, ref string msg)
        {
            try
            {
                var chvErrMsg = new SqlParameter
                {
                    ParameterName = "@chvErrMsg",
                    Direction = ParameterDirection.Output,
                    Size = 32
                };

                SqlParameter[] paras = {
                    new SqlParameter("@vCode",(object)vCode??DBNull.Value),
                    chvErrMsg
                };

                var returnValue = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForResult("PrWeb_SMS_Create", paras);
                msg = chvErrMsg.Value.ToString();
                return returnValue == 1;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public string GetPostData_submail(string vCode, string tel)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic["appid"] = "20829";
            dic["to"] = tel;
            dic["project"] = "NU25T";
            dic["signature"] = "bbae493b7c4f435219a63284362b7953";
            dic["vars"] = "{\"code\":\"" + vCode + "\"}";
            return string.Join("&", dic.Select(d => string.Format("{0}={1}", d.Key, d.Value)));
        }

        public string SubmailAPI
        {
            get { return "https://example.invalid/message/xsend"; }
        }
    }
}
