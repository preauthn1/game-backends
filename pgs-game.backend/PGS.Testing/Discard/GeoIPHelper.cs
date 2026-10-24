//using MaxMind.GeoIP2;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using target.BL.Interface;
//using target.Core.Repository;
//using target.DA.Repository;
//using target.Models.CoreLibs;
//using target.Models.Enums;
//using target.Models.Models;
//using target.Models.ViewModels;
//using target.Utility.Extensions;
//using target.Utility.Helper;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.Linq;

//namespace target.Testing
//{
//    public class GeoIPHelper 
//    {
//        private static DatabaseReader _databaseReader { get; set; }

//        static GeoIPHelper()
//        {
//            //這個檔案可以去  https://example.invalid 下載 (需註冊)
//            _databaseReader = new DatabaseReader("GeoLite2-City-2023.mmdb");
//        }

//        public static (string ISOCode , string CountryName) GetIPAddressInfo(string ip)
//        {
//            var parseResult = _databaseReader.City(ip);

//            string isoCode = (parseResult?.Country?.IsoCode ?? "").ToUpper(); 

//            return (isoCode, parseResult?.Country?.Name);
//        }

//    }
//}

