using DataTables.Mvc;
using FW.Core.Helper;
using game.Entity;
using game.Entity.Game;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace game.AdminSystem.Controllers.Game
{
    public class GameBRNNController : BaseController
    {
        /// <summary>
        /// 桌面列表
        /// </summary>
        /// <param name="serverID">房间编号</param>
        /// <returns></returns>
        public ActionResult TableList(int serverID)
        {
            var server = SiteHelper.GetAPIResult<GameServer>("Game/GetGameServerByID?serverID=" + serverID);
            if (server == null)
            {
                return Alert("房间不存在", Request.UrlReferrer.ToString());
            }
            ViewBag.ServerID = serverID;
            return View();
        }

        /// <summary>
        /// 获取桌面列表数据
        /// </summary>
        /// <param name="serverID">房间编号</param>
        /// <returns></returns>
        public ActionResult GetTableList(int serverID)
        {
            var server = SiteHelper.GetAPIResult<GameServer>("Game/GetGameServerByID?serverID=" + serverID);

            SortedDictionary<string, string> dicParam = new SortedDictionary<string, string>();
            dicParam.Add("action", "getserverinfo");
            dicParam.Add("tableno", "0");
            dicParam.Add("vcode", SiteHelper.ProtoBufSign(dicParam));

            var strRet = SystemHelper.PostData("http://" + server.HttpAddr + "/game", string.Join("&", dicParam.Select(m => m.Key + "=" + m.Value).ToArray()));
            if (!string.IsNullOrWhiteSpace(strRet))
            {
                var gameInfo = ProtoBufHelper.DeSerialize<netproto.Http_BRNN_GameInfo>(Convert.FromBase64String(strRet));
                return Json(gameInfo.TableList, JsonRequestBehavior.AllowGet);
            }

            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取桌面详细信息
        /// </summary>
        /// <param name="serverID">房间编号</param>
        /// <param name="tableNo">桌号</param>
        /// <returns></returns>
        public ActionResult TableDetail(int serverID, int tableNo)
        {
            var server = SiteHelper.GetAPIResult<GameServer>("Game/GetGameServerByID?serverID=" + serverID);
            if (server == null)
            {
                return Alert("房间不存在", Request.UrlReferrer.ToString());
            }
            ViewBag.ServerID = serverID;
            ViewBag.TableNO = tableNo;
            ViewBag.GameID = server.GameID;
            ViewBag.ShowName = server.ShowName;
            return View();
        }

        /// <summary>
        /// 获取桌面详细信息数据
        /// </summary>
        /// <param name="serverID">房间编号</param>
        /// <param name="tableNo">桌号</param>
        /// <returns></returns>
        public ActionResult GetTableDetail(int serverID, int tableNo)
        {
            var server = SiteHelper.GetAPIResult<GameServer>("Game/GetGameServerByID?serverID=" + serverID);

            SortedDictionary<string, string> dicParam = new SortedDictionary<string, string>();
            dicParam.Add("action", "gettableinfo");
            dicParam.Add("tableno", tableNo.ToString());
            dicParam.Add("vcode", SiteHelper.ProtoBufSign(dicParam));

            var strRet = SystemHelper.PostData("http://" + server.HttpAddr + "/game", string.Join("&", dicParam.Select(m => m.Key + "=" + m.Value).ToArray()));
            if (!string.IsNullOrWhiteSpace(strRet))
            {
                var tableDetail = ProtoBufHelper.DeSerialize<netproto.Http_BRNN_TableDetail>(Convert.FromBase64String(strRet));
                if (tableDetail == null) { return Json(new { }, JsonRequestBehavior.AllowGet); }

                var sum = new
                {
                    UserChingLong = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoChingLong),
                    UserBaiHu = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoBaiHu),
                    UserChuChueh = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoChuChueh),
                    UserHsuanWu = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoHsuanWu),
                    RobotChingLong = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoChingLong),
                    RobotBaiHu = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoBaiHu),
                    RobotChuChueh = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoChuChueh),
                    RobotHsuanWu = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoHsuanWu),
                    TotalChingLong = tableDetail.UserInfo.Sum(m => m.BetInfoChingLong),
                    TotalBaiHu = tableDetail.UserInfo.Sum(m => m.BetInfoBaiHu),
                    TotalChuChueh = tableDetail.UserInfo.Sum(m => m.BetInfoChuChueh),
                    TotalHsuanWu = tableDetail.UserInfo.Sum(m => m.BetInfoHsuanWu)
                };
                var info = SiteHelper.GetAPIResult<SuperToolDetailInfo>(string.Format("/Game/GetInfo?gameID={0}", server.GameID));
                
                return Json(new { Sum = sum, TableDetail = tableDetail, Info = info }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        [NoAuthorization]
        public ActionResult GetAssignRewardDetail(int gameID, int? serverID, int? tableNo)
        {
            var _assignRewardDetail = SiteHelper.GetAPIResult<List<SuperToolAssignRewardDetail>>(string.Format("/Game/GatAssignRewardDetail?gameID={0}", gameID));
            var _count = _assignRewardDetail.Count(x => !string.IsNullOrEmpty(x.Reward));
            var _assignDescList = _assignRewardDetail.Where(x => !string.IsNullOrEmpty(x.Reward)).Select(y => y.Reward.Split(',').Select(Int32.Parse).ToList()).ToList();
            var _chingLongWin = _assignDescList.Sum(y => y[0]);
            var _baiHuWin = _assignDescList.Sum(y => y[1]);
            var _chuChuehWin = _assignDescList.Sum(y => y[2]);
            var _hsuanWuWin = _assignDescList.Sum(y => y[3]);
            double _ChingLongRate = _count == 0 ? 0 : Math.Round((double)_chingLongWin / _count * 100, 0);
            double _BaiHuWinRate = _count == 0 ? 0 : Math.Round((double)_baiHuWin / _count * 100, 0);
            double _ChuChuehRate = _count == 0 ? 0 : Math.Round((double)_chuChuehWin / _count * 100, 0);
            double _HsuanWuWin = _count == 0 ? 0 : Math.Round((double)_hsuanWuWin / _count * 100, 0);

            var _rate = new
            {
                ChingLongRate = _ChingLongRate,
                BaiHuWinRate = _BaiHuWinRate,
                ChuChuehRate = _ChuChuehRate,
                HsuanWuWin = _HsuanWuWin,
                MainStatus = _assignRewardDetail.Count == 0 ? 99 : _assignRewardDetail.FirstOrDefault().MainStatus
            };

            List<netproto.BRNN_Poker> _history = new List<netproto.BRNN_Poker>();

            if (serverID != null && tableNo != null && _rate.MainStatus != 1)
            {
                var server = SiteHelper.GetAPIResult<GameServer>("Game/GetGameServerByID?serverID=" + serverID);
                SortedDictionary<string, string> dicParam = new SortedDictionary<string, string>();
                dicParam.Add("action", "history");
                dicParam.Add("tableno", tableNo.ToString());
                dicParam.Add("vcode", SiteHelper.ProtoBufSign(dicParam));

                var history = SystemHelper.PostData("http://" + server.HttpAddr + "/game", string.Join("&", dicParam.Select(m => m.Key + "=" + m.Value).ToArray()));

                if (!string.IsNullOrWhiteSpace(history))
                {
                    var resultCard = ProtoBufHelper.DeSerialize<netproto.BRNN_ResultCardList>(Convert.FromBase64String(history));
                    _history = resultCard.PokerList;
                    var ret = from pro in resultCard.PokerList
                              select new
                              {
                                  Reward = pro.RetType
                              };

                }
            }

            //string.Join(",", pro.RetType)

            return Json(new
            {
                Rate = _rate,
                AssignRewardDetail = _assignRewardDetail,
                History = from pro in _history
                          select new
                          {
                              Reward = string.Join(",", pro.RetType)
                          }
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 九局指定開獎
        /// </summary>
        /// <param name="serverID"></param>
        /// <param name="tableNo"></param>
        /// <returns></returns>
        public ActionResult AssignReward(int serverID, int gameID, int tableNo)
        {
            ViewBag.ServerID = serverID;
            ViewBag.TableNo = tableNo;
            ViewBag.GameID = gameID;
            return View();
        }


        /// <summary>
        /// 修改注水率
        /// </summary>
        /// <param name="serverID">房间编号</param>
        /// <param name="tableNo">桌号</param>
        /// <param name="popWaterRate">注水率</param>
        /// <returns></returns>
        public ActionResult EditWaterRate(int serverID, int tableNo, int popWaterRate)
        {
            var server = SiteHelper.GetAPIResult<GameServer>("Game/GetGameServerByID?serverID=" + serverID);

            SortedDictionary<string, string> dicParam = new SortedDictionary<string, string>();
            dicParam.Add("action", "modpopwaterrate");
            dicParam.Add("tableno", tableNo.ToString());
            dicParam.Add("popwaterrate", popWaterRate.ToString());
            dicParam.Add("vcode", SiteHelper.ProtoBufSign(dicParam));

            var strRet = SystemHelper.PostData("http://" + server.HttpAddr + "/game", string.Join("&", dicParam.Select(m => m.Key + "=" + m.Value).ToArray()));
            if (!string.IsNullOrWhiteSpace(strRet))
            {
                var result = ProtoBufHelper.DeSerialize<netproto.Http_Result>(Convert.FromBase64String(strRet));
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Json(new netproto.Http_Result() { result = false, message = "修改失败！" }, JsonRequestBehavior.AllowGet);
        }
    }
}