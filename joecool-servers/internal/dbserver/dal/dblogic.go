package dal

import (
	"fmt"
	"strconv"
	"strings"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/internal/dbserver/dal/msghandler"
	slot "example.invalid/ServerTeam/Servers/pkg/slot/dbhandler"

	"example.invalid/ServerTeam/Servers/pkg/db/dbutil"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
)

// DBLogic DB Logic Module
type DBLogic struct {
	mouduleCtx *module.MouduleCtx
	msgHandler *module.SemaphoreHookMsgHandlerGroup
}

// NewDBLogicModule Create DBLogic
func NewDBLogicModule(n int) *DBLogic {
	db := new(DBLogic)
	db.msgHandler = module.NewSemaphoreHookMsgHandlerGroup(n)

	return db
}

// Name 模組名稱
func (dl *DBLogic) Name() string {
	return "DBLogic"
}

// Run Start DB Logic Module
func (dl *DBLogic) Run(ctx *module.MouduleCtx) error {
	// Setting DB Connection
	dbutil.ConnString = ctx.GetConfigData("db", "conn", "connstring")
	dbutil.ConnString = strings.Replace(dbutil.ConnString, "\"", "", -1)

	dbutil.DealLogConnString = ctx.GetConfigData("db", "conn", "deallogconnstring")
	dbutil.DealLogConnString = strings.Replace(dbutil.DealLogConnString, "\"", "", -1)
	dbutil.ClubConnString = ctx.GetConfigData("db", "conn", "clubconnstring")
	dbutil.ClubConnString = strings.Replace(dbutil.ClubConnString, "\"", "", -1)

	dbutil.SMSWebAPI = ctx.GetConfigData("db", "conn", "smswebapi")

	// Setting Connection Pool
	psize := ctx.GetConfigData("db", "conn", "pool")
	ipsize, _ := strconv.Atoi(psize)
	dbutil.PoolSize = ipsize

	dbutil.InitDatabase()

	dl.mouduleCtx = ctx

	dl.registerMsgParser(ctx)
	dl.registerMsgHandler(ctx)

	dl.registerEvtHandler(ctx)

	return nil
}

// Stop Stop DB Logic Module
func (dl *DBLogic) Stop() {
	mlog.NewDefaultLogger().SetData(fmt.Sprintf("[DBLogic]dbserver Stop")).Error("HallServer壓測時")
	dl.mouduleCtx.Server.Stop()
}

func (dl *DBLogic) registerMsgParser(ctx *module.MouduleCtx) {
	ctx.RegisterMsgParser(msg.AllMsgBClassID, msg.AllMsgSClassID, rpcmsg.NewRPCMessageParser())
}

func (dl *DBLogic) registerEvtHandler(ctx *module.MouduleCtx) {

	ctx.OnConnect(func(clt *network.ConnInfo) bool {
		example.invalid("client %s connnect...", clt)
		return true
	})
	ctx.OnDisConnect(func(clt *network.ConnInfo) bool {
		example.invalid("client %s disconnnect...", clt)
		return true
	})
	ctx.OnMessage(func(clt *network.ConnInfo, m *msg.Message) bool {
		example.invalid("client %s msg:%d_%d", clt, m.BClassID, m.SClassID)

		//填充RPC信息
		fromrpcmsg := m.MsgData.(*rpcmsg.RPCMessage)

		retmsgs := dl.msgHandler.OnMessage(ctx, clt, m)

		if retmsgs != nil {
			for _, retmsg := range retmsgs {
				rmsg := new(rpcmsg.RPCMessage)
				rmsg.RPCInfo = fromrpcmsg.RPCInfo
				rmsg.MsgData, _ = proto.Marshal(retmsg.MsgData.(proto.Message))

				retmsg.MsgData = rmsg
				example.invalid("return msg:%d_%d", retmsg.BClassID, retmsg.SClassID)
				ctx.Server.SendMessage(clt, retmsg)
			}
		}
		//example.invalid("client %s msg:%d_%d dealed...", clt, m.BClassID, m.SClassID)

		return true
	})
}

func (dl *DBLogic) registerMsgHandler(ctx *module.MouduleCtx) {
	dl.msgHandler.AddHandler(new(msghandler.GuestLogonHandler))                      //游客登录
	dl.msgHandler.AddHandler(new(msghandler.UserLogonHandler))                       //用户登录
	dl.msgHandler.AddHandler(new(msghandler.UserLeaveHallHandler))                   //登出广场
	dl.msgHandler.AddHandler(new(msghandler.ServerRegHandler))                       //服务器注册
	dl.msgHandler.AddHandler(new(msghandler.RoomLogonHandler))                       //登录房间
	dl.msgHandler.AddHandler(new(msghandler.RoomLogoutHandler))                      //登出房间
	dl.msgHandler.AddHandler(new(msghandler.UpdateUserScoreHandler))                 //即时写分
	dl.msgHandler.AddHandler(new(msghandler.UpdateExperienceScoreHandler))           //體驗金幣
	dl.msgHandler.AddHandler(new(msghandler.HallServerStartHandler))                 //大厅启动
	dl.msgHandler.AddHandler(new(msghandler.GameServerStartHandler))                 //房间启动
	dl.msgHandler.AddHandler(new(msghandler.BindZhifubaoHandler))                    //绑定支付宝
	dl.msgHandler.AddHandler(new(msghandler.BindBankCardHandler))                    //绑定銀行卡
	dl.msgHandler.AddHandler(new(msghandler.ModifyPasswordHandler))                  //修改密码
	dl.msgHandler.AddHandler(new(msghandler.SendPhoneVCodeHandler))                  //发送验证码
	dl.msgHandler.AddHandler(new(msghandler.ModifyFaceHandler))                      //修改头像
	dl.msgHandler.AddHandler(new(msghandler.MoneyDepositHandler))                    //存取款
	dl.msgHandler.AddHandler(new(msghandler.BindGuestAccountHandler))                //绑定游客账号
	dl.msgHandler.AddHandler(new(msghandler.RequestUserInfoHandler))                 //请求用户大厅信息
	dl.msgHandler.AddHandler(new(msghandler.FindSetPwdByPhoneHandler))               //请求找回账号密码
	dl.msgHandler.AddHandler(new(msghandler.ModifyBankPasswordHandler))              //请求修改银行密码
	dl.msgHandler.AddHandler(new(msghandler.FindSetBankPwdByPhoneHandler))           //请求找回银行账号密码
	dl.msgHandler.AddHandler(new(msghandler.GetMailListHandler))                     //请求邮件列表
	dl.msgHandler.AddHandler(new(msghandler.GetMailDetailHandler))                   //请求邮件详情
	dl.msgHandler.AddHandler(new(msghandler.ContactServiceHandler))                  //发送联系客服信息
	dl.msgHandler.AddHandler(new(msghandler.ConvertMoneyHandler))                    //兑换金币
	dl.msgHandler.AddHandler(new(msghandler.GetAgentListHandler))                    //请求代理列表
	dl.msgHandler.AddHandler(new(msghandler.ApplyAgentHandler))                      //申请代理
	dl.msgHandler.AddHandler(new(msghandler.ReportAgentHandler))                     //举报代理
	dl.msgHandler.AddHandler(new(msghandler.CreatePayOrderHandler))                  //创建充值订单
	dl.msgHandler.AddHandler(new(msghandler.BlockIPHandler))                         //封锁IP
	dl.msgHandler.AddHandler(new(msghandler.LoadBlockIPListHandler))                 //加载封锁IP列表
	dl.msgHandler.AddHandler(new(msghandler.AddPerGameLogHandler))                   //写每局记录
	dl.msgHandler.AddHandler(new(msghandler.RobotLogonHandler))                      //加载机器人
	dl.msgHandler.AddHandler(new(msghandler.GetFAQHandler))                          //加载机器人
	dl.msgHandler.AddHandler(new(msghandler.GetUserLuckScoreHandler))                //获取用户幸运值
	dl.msgHandler.AddHandler(new(msghandler.GetConvertLogHandler))                   //查詢兌換記錄
	dl.msgHandler.AddHandler(new(msghandler.GetBankListHandler))                     //查詢銀行列表
	dl.msgHandler.AddHandler(new(msghandler.GameVerHandler))                         //更新遊戲版本號
	dl.msgHandler.AddHandler(new(msghandler.RequestLoginBonusHandler))               //登入獎勵操作
	dl.msgHandler.AddHandler(new(msghandler.RequestAgentDetailHandler))              //取得代理資訊
	dl.msgHandler.AddHandler(new(msghandler.CSNotifyCountHandler))                   //未讀客服消息數量
	dl.msgHandler.AddHandler(new(msghandler.SuperToolAddAssignRewardHandler))        //新增指定多局開獎 <--- 多此一舉
	dl.msgHandler.AddHandler(new(msghandler.SuperToolGetAssignRewardDetailHandler))  //獲取指定開獎項目
	dl.msgHandler.AddHandler(new(msghandler.SuperToolUpdAssignRewardDetailHandler))  //更新指定開獎項目結果
	dl.msgHandler.AddHandler(new(msghandler.SuperToolAddChangeLogHandler))           //操作獎池及指定放分的記錄
	dl.msgHandler.AddHandler(new(msghandler.SuperToolGetVipUserHandler))             //獲取監控用戶列表
	dl.msgHandler.AddHandler(new(msghandler.SuperToolUpdVIPUserHandler))             //更新監控用戶數據
	dl.msgHandler.AddHandler(new(msghandler.SuperToolFishUpdSystemRewardHandler))    //捕魚更新系統放分和取得系統放分
	dl.msgHandler.AddHandler(new(msghandler.SuperToolFishUpdVIPUserHandler))         //捕魚個人放分
	dl.msgHandler.AddHandler(new(msghandler.SuperToolZjhGetVipUserHandler))          //ZJH獲取監控用戶列表
	dl.msgHandler.AddHandler(new(msghandler.SuperToolZjhUpdVIPUserHandler))          //ZJH更新監控用戶數據
	dl.msgHandler.AddHandler(new(msghandler.FishSysPoolHandler))                     //Fish 新增/更新系統水池输赢记录
	dl.msgHandler.AddHandler(new(msghandler.FishPlayerPoolHandler))                  //新增/更新玩家个人水池输赢记录
	dl.msgHandler.AddHandler(new(msghandler.GetBeginnerOfferHandler))                //取得玩家新手優惠
	dl.msgHandler.AddHandler(new(msghandler.UpdateBeginnerOfferHandler))             //更新玩家新手優惠
	dl.msgHandler.AddHandler(new(msghandler.GameCompetitionHandler))                 //查詢比賽活動公告
	dl.msgHandler.AddHandler(new(msghandler.GameCompetitionRuleHandler))             //查詢比賽活動規則
	dl.msgHandler.AddHandler(new(msghandler.UpdatePiSaiScoreHandler))                //比賽積分写分
	dl.msgHandler.AddHandler(new(msghandler.GameCompetitionStartHandler))            //報名參加比賽活動
	dl.msgHandler.AddHandler(new(msghandler.GetContestUserDataHandler))              //取得比賽活動用戶資料
	dl.msgHandler.AddHandler(new(msghandler.UpdateContestUserDataHandler))           //更新比賽活動用戶資料
	dl.msgHandler.AddHandler(new(msghandler.Cd21SysPoolHandler))                     //21點 新增/更新系統水池输赢记录
	dl.msgHandler.AddHandler(new(msghandler.GetGameJackpotPoolHandler))              //取得遊戲獎金池
	dl.msgHandler.AddHandler(new(msghandler.UpdateGameJackpotPoolHandler))           //更新遊戲獎金池
	dl.msgHandler.AddHandler(new(msghandler.UpdateUserJackpotBoxHandler))            //遊戲獎金寫入保險箱
	dl.msgHandler.AddHandler(new(msghandler.GetUserQRCodeHandler))                   //全民推廣 我的推廣碼
	dl.msgHandler.AddHandler(new(msghandler.GetUserAngentExecInfoHandler))           //全民推廣員Agent Exec
	dl.msgHandler.AddHandler(new(msghandler.GetUserAgentListHandler))                //全民推廣 推廣人數
	dl.msgHandler.AddHandler(new(msghandler.GetAgentTaxBonusLogHandler))             //全民推廣 每日收益
	dl.msgHandler.AddHandler(new(msghandler.GetAgentTaxListHandler))                 //全民推廣 收益規則
	dl.msgHandler.AddHandler(new(msghandler.GetUserAngentTaxLogHandler))             //全民推廣今日稅收 Tax Log
	dl.msgHandler.AddHandler(new(msghandler.GetShanFuHandler))                       //閃付
	dl.msgHandler.AddHandler(new(msghandler.GetShanFuChatHandler))                   //閃付聊天
	dl.msgHandler.AddHandler(new(msghandler.GetUserAngentUserLevelInfoHandler))      //全民推廣今日稅收 Tax Log
	dl.msgHandler.AddHandler(new(msghandler.GetWithdrawListIHandler))                //全民推廣領取收益
	dl.msgHandler.AddHandler(new(msghandler.GetAgentInfoHandler))                    //玩家/代理信息(淨利潤)
	dl.msgHandler.AddHandler(new(msghandler.GetSubUserListRetHandler))               //新增玩家(淨利潤)
	dl.msgHandler.AddHandler(new(msghandler.GetAgentWithdrawAmountHandler))          //代理提現紀錄(淨利潤)
	dl.msgHandler.AddHandler(new(msghandler.GetWithdrawAmountHandler))               //提現紀錄(淨利潤)
	dl.msgHandler.AddHandler(new(msghandler.GetBonusAmountHandler))                  //可提現金額查詢(淨利潤)
	dl.msgHandler.AddHandler(new(msghandler.WithDrawOrderIDHandler))                 //可提現金額查詢(淨利潤)
	dl.msgHandler.AddHandler(new(msghandler.ApplyListHandler))                       //審核代理申請列表(淨利潤)
	dl.msgHandler.AddHandler(new(msghandler.NetProfitAgentGetAgentDetailRetHandler)) // USER 推廣信息(淨利潤)
	dl.msgHandler.AddHandler(new(msghandler.NetProfitAgentGetAgentReportLogHandler)) // USER 新增代理(淨利潤)
	dl.msgHandler.AddHandler(new(msghandler.NetProfitAgentGetWithdrawDetailHandler)) // USER 新增代理(淨利潤)
	dl.msgHandler.AddHandler(new(msghandler.UserAgentUserModeGetHandler))            // USER 取得已開的模式
	dl.msgHandler.AddHandler(new(msghandler.GetWithdrawOrderHandler))                // 馬上提現
	dl.msgHandler.AddHandler(new(msghandler.UserAgentGetWithAmountHandler))          // 申請提現(全民代理)
	dl.msgHandler.AddHandler(new(msghandler.UserAgentGetWithAmountListHandler))      // 申請提現例表(全民代理)
	dl.msgHandler.AddHandler(new(msghandler.UserAgentGetWithdrawAmountListHandler))  // 申請提現例表(我的提現)
	dl.msgHandler.AddHandler(new(msghandler.UserAgentResultListHandler))             // 我的業績(全民代理)
	dl.msgHandler.AddHandler(new(msghandler.UserAgentBorrowListHandler))             // 我的預提(全民代理)
	dl.msgHandler.AddHandler(new(msghandler.UserAgentWinAmountListHandler))          // 我的獎勵(全民代理)
	dl.msgHandler.AddHandler(new(msghandler.GetMyPromotionHandler))                  // 無限級QRCODE
	dl.msgHandler.AddHandler(new(msghandler.GetRewardTipsHandler))                   // 無限級獎勵說明
	dl.msgHandler.AddHandler(new(msghandler.UnlimitConvertOrderHandler))             // 無限級按鈕1
	dl.msgHandler.AddHandler(new(msghandler.UnlimitPreConvertOrderHandler))          // 無限級按鈕2
	dl.msgHandler.AddHandler(new(msghandler.SplitGetMyInfoHandler))                  // 裂變1推廣信息
	dl.msgHandler.AddHandler(new(msghandler.SplitGetMyConvertHandler))               // 裂變2提現紀錄
	dl.msgHandler.AddHandler(new(msghandler.SplitGetRewardTipsHandler))              // 裂變3說明
	dl.msgHandler.AddHandler(new(msghandler.SplitGetMyRewardHandler))                // 裂變4馬上提現
	dl.msgHandler.AddHandler(new(msghandler.SplitConvertOrderHandler))               // 裂變5提現紀錄送出提現金額
	dl.msgHandler.AddHandler(new(msghandler.SplitBindingInvitedCodeHandler))         // 裂變6輸入推廣碼
	dl.msgHandler.AddHandler(new(msghandler.SlotFreeGameHandler))                    // Slot FreeSpin紀錄寫入及讀取
	dl.msgHandler.AddHandler(new(msghandler.DfdcBonusGameHandler))                   // DFDC BonusGame紀錄寫入及讀取
	dl.msgHandler.AddHandler(new(msghandler.SlotJackpotNoLinkHandler))               // Slot 非連接型Jackpot紀錄寫入及讀取
	dl.msgHandler.AddHandler(new(msghandler.SlotJackpotLinkHandler))                 // Slot 連接型Jackpot紀錄寫入及讀取
	dl.msgHandler.AddHandler(new(msghandler.EventChargeBonusAmountHandler))          // 充值返利活動  取得個人流水及限制
	dl.msgHandler.AddHandler(new(msghandler.GetAreaConfigHandler))                   // Hall 取得AreaId的AreaConfig資訊
	dl.msgHandler.AddHandler(new(msghandler.SlotGetGameLogHandler))                  // Slot讀取紀錄
	dl.msgHandler.AddHandler(new(msghandler.SlotAddGameLogHandler))                  // Slot 紀錄寫入
	dl.msgHandler.AddHandler(new(msghandler.UnlimitAddUserRelationHandler))          // USER 加入下线(无限级)
	dl.msgHandler.AddHandler(new(msghandler.NetProfitAgentAddUserRelationHandler))   // USER 加入下线(净利润)
	dl.msgHandler.AddHandler(new(msghandler.SplitAddUserRelationHandler))            // USER 加入下线(裂变)
	dl.msgHandler.AddHandler(new(msghandler.UserAgentAddUserRelationHandler))        // USER 加入下线(全民代理)
	dl.msgHandler.AddHandler(new(msghandler.EventGetPromotionPopupHandler))          // 活动公告系统 首页弹出
	dl.msgHandler.AddHandler(new(msghandler.EventGetPromotionAnnouncementHandler))   // 活动公告系统 邮件公告页
	dl.msgHandler.AddHandler(new(msghandler.TGWBonusGameHandler))                    // 糖果屋BonusGame
	dl.msgHandler.AddHandler(new(msghandler.GetServerRelationsHandler))              // 遊戲渠道
	dl.msgHandler.AddHandler(new(msghandler.JackpotNoLinkHandler))                   // 非連接型Jackpot紀錄寫入及讀取
	dl.msgHandler.AddHandler(new(msghandler.JackpotLinkHandler))                     // 連接型Jackpot紀錄寫入及讀取
	dl.msgHandler.AddHandler(new(msghandler.GetGameCategoryListHandler))             // 游戏分类清单
	dl.msgHandler.AddHandler(new(msghandler.SuperToolJLBPKHandler))                  // 加勒比海撲克超端
	dl.msgHandler.AddHandler(new(msghandler.SystemPoolHandler))                      // 水線控制

	// Slot 專區
	dl.msgHandler.AddHandler(new(slot.SelectGameHander))     // 寫入及讀取選擇遊戲機會
	dl.msgHandler.AddHandler(new(slot.FreeSpinHander))       // 寫入及讀取免費遊戲
	dl.msgHandler.AddHandler(slot.NewJackpotNoLinkHandler()) // 寫入及讀取JackpotNoLink資訊
	dl.msgHandler.AddHandler(slot.NewGameLogHandler())       // 寫入及讀取遊戲歷程
	dl.msgHandler.AddHandler(slot.NewBonusGameHandler())     // 寫入及讀取翻牌遊戲資料
	dl.msgHandler.AddHandler(slot.NewDeferredAwardHandler()) // 寫入及讀取延遲性獎勵
	dl.msgHandler.AddHandler(slot.NewLightningHandler())     // 寫入及讀取Lightning遊戲資料

}
