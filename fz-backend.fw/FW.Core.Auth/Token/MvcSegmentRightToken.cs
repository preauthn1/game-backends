using System;
using System.Collections.Generic;
using System.Linq;

namespace FW.Core.Auth.Token
{
    public class MvcSegmentRightToken : [REDACTED]
    {
        /// <summary>
        /// ctor
        /// </summary>
        public MvcSegmentRightToken()
        { }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="url"></param>
        public MvcSegmentRightToken(string url)
        {
            if (url == "*")
            {
                Area = "*";
                Controller = "*";
                Action = "*";
                Method = "*";

                return;
            }

            string[] arr = url.Split(new char[]{'_'});
            if (arr.Length != 4)
            {
                throw new AuthException(String.Format("参数url:{0}格式不正确,eg:Area_Controller_Action_Method",url));
            }
            
            Area = arr[0];
            Controller = arr[1];
            Action = arr[2];
            Method = arr[3];

            if (String.IsNullOrWhiteSpace(Area))
            {
                Area = "";
            }
            if (String.IsNullOrWhiteSpace(Controller))
            {
                Controller = "";
            }
            if (String.IsNullOrWhiteSpace(Action))
            {
                Action = "";
            }
            if (String.IsNullOrWhiteSpace(Method))
            {
                Method = "";
            }

        }

        /// <summary>
        /// 区域
        /// </summary>
        public string Area
        {
            get;
            set;
        }

        /// <summary>
        /// 控制器
        /// </summary>
        public string Controller
        {
            get;
            set;
        }

        /// <summary>
        /// Action
        /// </summary>
        public string Action
        {
            get;
            set;
        }

        /// <summary>
        /// HttpMethod
        /// </summary>
        public string Method
        {
            get;
            set;
        }

        public bool IsMatch(IRightToken token)
        {
            var segToken = [REDACTED] as MvcSegmentRightToken;
            if(segToken == null)
            {
                return false;
            }
            if (Area != "*" && String.Compare(Area, segToken.Area, true) != 0)
            {
                return false;
            }

            if (Controller != "*" && String.Compare(Controller, segToken.Controller, true) != 0)
            {
                return false;
            }

            if (Action != "*" && String.Compare(Action, segToken.Action, true) != 0)
            {
                return false;
            }

            if (Method != "*" && String.Compare(Method, segToken.Method, true) != 0)
            {
                return false;
            }
           

            return true;
        }


        public IRightToken ParseToken(string tokenStr)
        {
            return new MvcSegmentRightToken(tokenStr);
        }

        public override string ToString()
        {
            return String.Format("{0}_{1}_{2}_{3}", Area,Controller,Action, Method);
        }
    }
}
