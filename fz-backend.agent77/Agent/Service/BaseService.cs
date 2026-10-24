using Repository;
using Repository.ViewModel;
using Service.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Service
{
    public class BaseService
    {
        public SelectList GetLevelSelectList(int? key)
        {
            SelectList selectList = null;
            var dic = new Dictionary<string, string>()
            {
                { "1", "一级"},
                { "2", "二级"},
                { "3", "三级"}
            };

            if (!key.HasValue)
                selectList = new SelectList(dic, "Key", "Value");
            else
                selectList = new SelectList(dic, "Key", "Value", key.Value);

            return selectList;
        }

        /// <summary>
        /// 提现记录状态
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public SelectList GetStateSelectList(int? key)
        {
            SelectList selectList = null;
            var dic = new Dictionary<string, string>()
            {
                { "0", "審核中"},
                { "1", "已完成"},
                { "2", "拒絕兌換"}
            };

            if (!key.HasValue)
                selectList = new SelectList(dic, "Key", "Value");
            else
                selectList = new SelectList(dic, "Key", "Value", key.Value);

            return selectList;
        }

        public SelectList GetAgentApplySelectList(int? key)
        {
            SelectList selectList = null;
            var dic = new Dictionary<string, string>()
            {
                { "0", "審核中"},
                { "1", "已通過"},
                { "2", "已拒絕"}
            };

            if (!key.HasValue)
                selectList = new SelectList(dic, "Key", "Value");
            else
                selectList = new SelectList(dic, "Key", "Value", key.Value);

            return selectList;
        }

        /// <summary>
        /// 平台编号
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public MultiSelectList GetPlatformMultiSelectList(string key)
        {
            var repoFactory = new AgentClubFactory();

            MultiSelectList selectList = null;

            var li = new AgentClubFactory().Database().SqlQuery<PlatformInfo>("PrWeb_UserAgent_GetPlatformInfo").ToList();

            var dic = new Dictionary<int, string>();
           
            li.ForEach(x => dic.Add(x.PlatformID, $"[{x.PlatformID}]{x.PlatformName}"));

            if (string.IsNullOrEmpty(key))
                selectList = new MultiSelectList(dic, "Key", "Value");
            else
                selectList = new MultiSelectList(dic, "Key", "Value", key.Split(',').ToList());

            return selectList;
        }

        public SelectList GetPlatformSelectList(int? key)
        {
            var repoFactory = new AgentClubFactory();

            SelectList selectList = null;

            var li = new AgentClubFactory().Database().SqlQuery<PlatformInfo>("PrWeb_UserAgent_GetPlatformInfo").ToList();

            var dic = new Dictionary<int, string>();

            li.ForEach(x => dic.Add(x.PlatformID, $"[{x.PlatformID}]{x.PlatformName}"));

            if (key.HasValue)
                selectList = new SelectList(dic, "Key", "Value", key);
            else
                selectList = new SelectList(dic, "Key", "Value");

            return selectList;
        }

        /// <summary>
        /// 提領列表状态
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public SelectList GetWithdrawListStateSelectList(int? key)
        {
            SelectList selectList = null;
            var dic = new Dictionary<string, string>()
            {
                { "0", "審核中"},
                { "1", "已提領"},
                { "2", "已退還"}
            };

            if (!key.HasValue)
                selectList = new SelectList(dic, "Key", "Value");
            else
                selectList = new SelectList(dic, "Key", "Value", key.Value);

            return selectList;
        }

        public SelectList GetAdminTypeSelectList(int? key)
        {
            SelectList selectList = null;
            var dic = new Dictionary<string, string>()
            {               
                { "1", "财务"},
                { "2", "总代理"}
            };

            if (!key.HasValue)
                selectList = new SelectList(dic, "Key", "Value");
            else
                selectList = new SelectList(dic, "Key", "Value", key.Value);

            return selectList;
        }

        /// <summary>
        /// 提现记录状态
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public SelectList GetStageLevelSelectList(int? key)
        {
            SelectList selectList = null;
            var dic = new Dictionary<string, string>();

            for (int i = 1; i <= 10; i++)
                dic.Add(i.ToString(), i.ToString());

            if (!key.HasValue)
                selectList = new SelectList(dic, "Key", "Value");
            else
                selectList = new SelectList(dic, "Key", "Value", key.Value);

            return selectList;
        }

        public SelectList GetUserPlatformSelectList(int? key, string userID)
        {
            var li = (from p in CacheHelper.GetPlatFormArea(userID)
                      group p by new { p.PlatformID, p.PlatformName } into g
                      select new PlatformInfo
                      {
                          PlatformID = g.Key.PlatformID,
                          PlatformName = g.Key.PlatformName
                      }).ToList();       


            SelectList selectList = null;

            var dic = new Dictionary<int, string>();

            li.ForEach(x => dic.Add(x.PlatformID, $"[{x.PlatformID}]{x.PlatformName}"));

            if (key.HasValue)
                selectList = new SelectList(dic, "Key", "Value", key);
            else
                selectList = new SelectList(dic, "Key", "Value");

            return selectList;
        }

        /// <summary>
        /// 用戶身分
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public SelectList GetUserStatusSelectList(int? key)
        {
            SelectList selectList = null;
            var dic = new Dictionary<string, string>()
            {
                { "1", "玩家"},
                { "2", "代理"}
            };

            if (!key.HasValue)
                selectList = new SelectList(dic, "Key", "Value");
            else
                selectList = new SelectList(dic, "Key", "Value", key.Value);

            return selectList;
        }

        /// <summary>
        /// 活動狀態
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public SelectList GetEventSelectList(int? key)
        {
            SelectList selectList = null;
            var dic = new Dictionary<string, string>()
            {
                { "0", "关闭"},
                { "1", "开启"}
            };

            if (!key.HasValue)
                selectList = new SelectList(dic, "Key", "Value");
            else
                selectList = new SelectList(dic, "Key", "Value", key.Value);

            return selectList;
        }

        public SelectList GetBankCodeSelectList(string key)
        {
            var repoFactory = new AgentClubFactory();

            SelectList selectList = null;

            var li = new AgentClubFactory().Database().SqlQuery<UnionpayBankCode>("PrWeb_GetUnionpayBankCode").ToList();

            var dic = new Dictionary<string, string>();

            li.ForEach(x => dic.Add(x.Code, $"[{x.Code}]{x.Bankname}"));

            if (string.IsNullOrEmpty(key))
                selectList = new SelectList(dic, "Key", "Value");
            else
                selectList = new SelectList(dic, "Key", "Value", key);

            return selectList;
        }
    }
}
