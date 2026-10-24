using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.AdminSys.Provider
{
    public class Event
    {
        private static Action _loginFinshed;
        private static Func<string> _loginUrl;
        public static Action LoginFinshed
        {
            get
            {
                if (_loginFinshed == null)
                {
                    return () => { };
                }
                else
                {
                    return _loginFinshed;
                }
            }
            set
            {
                _loginFinshed = value;
            }
        }

        public static Func<string> GetLoginUrl
        {
            get
            {
                if (_loginUrl == null)
                {
                    return () => { return null; };
                }
                else
                {
                    return _loginUrl;
                }
            }
            set
            {
                _loginUrl = value;
            }
        }

    }
}
