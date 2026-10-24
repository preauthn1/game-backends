using Microsoft.AspNetCore.Mvc;
using target.Models.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using target.Utility.Extensions;
using target.Utility.Helper.Dependency;
using target.Models.Table;
using target.Utility;
using target.Models.Models;
using target.Models;
using target.Utility.Helper;
using System.Reflection;
using target.Web.Modules.Filter;
using target.Utility.Utility.ThirdParty;

namespace target.Utility.Extensions.Web
{
    public class MSTestHelper
    {
        public static HashSet<string> ListOfBetRecordNoPgs_MustMock = new HashSet<string>();

        public static HashSet<MSTestMockEnableFlag> MockEnableTags = new HashSet<MSTestMockEnableFlag>();


        public static void AddMockTag(MSTestMockEnableFlag tag)
        {
            MockEnableTags.Add(tag);
        }

        public static void RemoveMockTag(MSTestMockEnableFlag tag)
        {
            MockEnableTags.Remove(tag);
        }

        public static bool IsMockTagExist(MSTestMockEnableFlag tag)
        {
            if (SettingHelper.IsMSTest)
                return false;

            return MockEnableTags.Contains(tag);
        }

        public static void AddBetRecordNoPGS_ToMockList(string betRecordNo)
        {
            ListOfBetRecordNoPgs_MustMock.Add(betRecordNo);
        }

        public static bool IsBetRecordNoPGSMockRequired(string betRecordNo)
        {
            return ListOfBetRecordNoPgs_MustMock.Contains(betRecordNo);
        }

    }

    public enum MSTestMockEnableFlag
    {
        MockWhenSingleWalletMerchantRequest,
    }



}
