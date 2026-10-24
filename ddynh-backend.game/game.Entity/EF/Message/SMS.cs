using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Message
{
    public class SMS
    {
        public int ID { get; set; }
        public string Vender { get; set; }
        public int Status { get; set; }
        private string _Name;
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = (String.IsNullOrEmpty(value)) ? "" : value;
            }
        }
        private string _Url;
        public string Url
        {
            get
            {
                return _Url;
            }
            set
            {
                _Url = (String.IsNullOrEmpty(value)) ? "" : value;
            }
        }
        private string _Method;
        public string Method
        {
            get
            {
                return _Method;
            }
            set
            {
                _Method = (String.IsNullOrEmpty(value)) ? "GET" : value;
            }
        }
        /*
        private Dictionary<string, string> _Content;
        public Dictionary<string, string> Content
        {
            get
            {
                return _Content;
            }
            set
            {
                _Content = (value is null) ? new Dictionary<string, string>() : value;
            }
        }
        */
    }
}
