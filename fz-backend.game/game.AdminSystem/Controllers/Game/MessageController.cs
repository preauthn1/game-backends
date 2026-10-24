using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DataTables.Mvc;
using FW.Core.Helper;
using FW.Core.Helper.Enum;
using game.Entity.Message;
using game.Entity.Notify;
using game.Infrastructure;

namespace game.AdminSystem.Controllers.Game
{
    /// <summary>
    /// 游戏管理-短消息管理
    /// </summary>
    public class MessageController : BaseController
    {
        /// <summary>
        /// 3-发送短消息
        /// </summary>
        /// <returns></returns>
        public ActionResult Send()
        {
            return View();
        }

        /// <summary>
        /// 3-发送短消息 > 4-发送 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Send(string platformid, string areaID, string title, string msgContent, int Sort, MessageType type, string userIDs = "")
        {

            if (CheckHelper.ContainsInvalidChar(title))
            {
                return Alert("操作失败：标题包含特殊字符！");
            }

            if (CheckHelper.ContainsInvalidChar(msgContent))
            {
                return Alert("操作失败：内容包含特殊字符！");
            }

            var adminId = SiteService.GetAdminUserID();

            List<int> UserIDs = new List<int>();
            if (!string.IsNullOrEmpty(userIDs))
            {
                foreach (var item in userIDs.Replace("，", ",").Split(','))
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(item))
                            UserIDs.Add(Convert.ToInt32(item));
                    }
                    catch (Exception)
                    {
                        return Alert("接收者UserID格式不正确！");
                    }
                }
            }

            var api = SiteHelper.GetAPIResult<APIResult>($"/Message/SendMessage?adminID={adminId}&platformID={platformid}&areaID={areaID}&users={userIDs}&msgContent={msgContent}&Sort={Sort}&title={title}&type={(int)type}");
            return Alert(api.Message);
        }


        /// <summary>
        /// 收件箱查询
        /// </summary>
        /// <returns></returns>
        public ActionResult Inbox()
        {
            return View();
        }

        /// <summary>
        /// 收件箱查询
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetInbox(int? draw, int? start, int length, int? userID = null, string loginName = "", string nickName = "", DateTime? sendTimeBegin = null, DateTime? sendTimeEnd = null)
        {
            if (userID == null && loginName.Length == 0 && nickName.Length == 0) return Json(new
            {
                draw = draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                success = true,
                data = new { }
            });

            userID = UserService.GetUserID(userID, loginName, nickName);
            if (userID == 0) return Json(new
            {
                draw = draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                success = true,
                data = new { }
            });

            if (sendTimeBegin == null) sendTimeBegin = DateTime.Now.AddDays(-1);

            var pageIndex = Math.Ceiling((decimal)start.Value / length) + 1;
            var api = SiteHelper.GetAPIResult<PagedList<ReceiveUser>>(string.Format("/Message/GetMessage?toUserID={0}&sendTimeBegin={1}&sendTimeEnd={2}&pageIndex={3}&pageSize={4}", userID, sendTimeBegin, sendTimeEnd, pageIndex, length));

            var query = from pro in api.PageData
                        select new
                        {
                            ReadTime = pro.ReadTime != null ? pro.ReadTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "--",
                            Platform = pro.User.Area.Platform.Name,
                            UserID = pro.UserID,
                            LoginName = pro.User.LoginName,
                            GradeID = pro.User.GradeID,
                            MsgID = pro.Message.Id,
                            MsgContent = pro.Message.MsgContent,
                            Status = pro.Status.GetDisplayName()
                        };

            return Json(new
            {
                draw = draw,
                recordsTotal = api.TotalCount,
                recordsFiltered = api.TotalCount,
                success = true,
                data = query
            });
        }

        /// <summary>
        /// 收件箱查询 4-删除
        /// </summary>
        /// <returns></returns>
        public ActionResult DelInbox(int msgID, int userID)
        {
            var message = string.Empty;
            var api = SiteHelper.GetAPIResult<APIResult>(string.Format("Message/DelReceiveMsg?msgID={0}&userID={1}", msgID, userID));

            var data = new
            {
                status = true,
                msg = string.IsNullOrEmpty(message) ? "操作成功" : message
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 3-发件箱查询
        /// </summary>
        /// <returns></returns>
        public ActionResult Outbox(int? userID = null, string loginName = "", string nickName = "", DateTime? sendTimeBegin = null, DateTime? sendTimeEnd = null, int p = 1)
        {
            if (sendTimeBegin == null) sendTimeBegin = DateTime.Now.AddDays(-1);
            userID = UserService.GetUserID(userID, loginName, nickName);
            if (userID == null) return View();

            var list = SiteHelper.GetAPIResult<PagedList<ReceiveUser>>(string.Format("/Message/GetMessage?fromUserID={0}&sendTimeBegin={1}&sendTimeEnd={2}&pageIndex={3}", userID, sendTimeBegin, sendTimeEnd, p));

            return View(list);
        }

        /// <summary>
        /// 3-发件箱查询 4-删除
        /// </summary>
        /// <returns></returns>
        public ActionResult DelOutbox(int msgID, int userID)
        {
            var result = SiteHelper.GetAPIResult<APIResult>(string.Format("Message/DelSendMsg?msgID={0}&userID={1}", msgID, userID));
            if (result.Result)
                return Back();
            else
                return Alert(result.Message);
        }
        /// <summary>
        /// 消息列表
        /// </summary>
        /// <returns></returns>
        public ActionResult List()
        {
            return View();
        }


        /// <summary>
        /// 消息列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetMessageList(int? draw, int? start, int length, string title, MessageType? type)
        {
            var pageIndex = Math.Ceiling((decimal)start.Value / length) + 1;
            var api = SiteHelper.GetAPIResult<PagedList<Message>>(string.Format("/Message/GetMessageList?title={0}&type={1}&pageIndex={2}&pageSize={3}", title, type, pageIndex, length));

            var query = from pro in api.PageData
                        select new
                        {
                            Id = pro.Id,
                            Title = pro.Title,
                            MsgContent = pro.MsgContent,
                            Type = pro.Type.GetDisplayName(),
                            FromUserID = pro.FromUserID,
                            Platform = pro.User != null ? pro.User.Area.Platform.Name : string.Empty,
                            CreateTime = pro.CreateTime.ToString("yyyy-MM-dd HH:mm")
                        };

            return Json(new
            {
                draw = draw,
                recordsTotal = api.TotalCount,
                recordsFiltered = api.TotalCount,
                success = true,
                data = query
            });
        }

        public ActionResult DelMsg(int id)
        {
            var message = string.Empty;
            var result = SiteHelper.GetAPIResult<APIResult>(string.Format("/Message/DelMessageAllByMsgID?msgID={0}", id));

            var data = new
            {
                status = true,
                msg = string.IsNullOrEmpty(message) ? "操作成功" : message
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 短讯纪录
        /// </summary>
        /// <returns></returns>
        public ActionResult SMSLog()
        {
            return View();
        }


        /// <summary>
        /// 发送讯息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetSMSLog([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string tel = null, int? userID = null, string loginName = "", string nickName = "", DateTime? beginTime = null, DateTime? endTime = null)
        {
            var draw = requestModel.Draw;
            var start = requestModel.Start;
            var length = requestModel.Length;
            var pageIndex = Math.Ceiling((decimal)start / length) + 1;
            userID = UserService.GetUserID(userID, loginName, nickName);
            var operatorID = CurrentUser.UserID;

            var api = SiteHelper.GetAPIResult<PagedList<SMSLog>>(string.Format("/Notify/GetSMSLog?operatorID={0}&tel={1}&UserID={2}&BeginTime={3}&EndTime={4}&pageIndex={5}&pageSize={6}"
                , operatorID, tel, userID, beginTime, endTime, pageIndex, length));

            var query = from pro in api.PageData
                        select new
                        {
                            CodeID = pro.CodeID,
                            Tel = pro.Tel,
                            VCode = pro.VCode,
                            Status = pro.status.GetDisplayName(),
                            CreateTime = pro.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            LastSendTime = pro.LastSendTime.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                            IPAddress = pro.IPAddress,
                            CodeType = pro.CodeType.GetDisplayName(),
                            UserID = pro.UserID,
                            SendResult = pro.SendResult,
                            Vender = pro.Vender
                        };

            return Json(new
            {
                draw = draw,
                recordsTotal = api.TotalCount,
                recordsFiltered = api.TotalCount,
                success = true,
                data = query
            });
        }


        /// <summary>
        /// 讯息厂商
        /// </summary>
        /// <returns></returns>
        public ActionResult GetSMSVender()
        {
            var result = SiteHelper.GetAPIResult<List<SMSVender>>("/Notify/GetSMSVender");

            return Json(new
            {
                success = true,
                result = result
            }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 修改开启厂商
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EnableSMSVender(byte id)
        {
            var result = SiteHelper.GetAPIResult<APIResult>(string.Format("/Notify/EnableSMSVender?id={0}", id));

            return Json(new
            {
                success = true,
                result = result
            });
        }
    }
}