package halllogic

import (
	"time"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/internal/hallserver/halllogic/hallutil"
	"example.invalid/ServerTeam/Servers/internal/hallserver/halllogic/msghandler"
	"example.invalid/ServerTeam/Servers/internal/hallserver/halllogic/notifyhandler"
	"example.invalid/ServerTeam/Servers/internal/hallserver/halllogic/rpchandler"

	"example.invalid/ServerTeam/Servers/pkg/command"
	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/gameserver"
	"example.invalid/ServerTeam/Servers/pkg/gameserver/serverhandler"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/module/logicmodule"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/player"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

//大厅逻辑
type HallLogic struct {
	*logicmodule.GameServerLogicModule
	serverCtx       *serverhandler.GameServerCtx
	gameserverList  *msghandler.GameServerList
	server          *gameserver.GameServer
	serverCfg       *hallutil.HallServerConfig
	hallverProvider hallutil.HallVerProvider
	hallVerLoader   hallutil.HallVerLoader
	gserverProvider hallutil.GameServerProvider
}

//创建大厅逻辑对象
func NewHallLogicModule(gs *gameserver.GameServer, cfg *hallutil.HallServerConfig) *HallLogic {
	hl := new(HallLogic)
	hl.serverCtx = gs.GameServerRunCtx
	hl.server = gs
	hl.serverCfg = cfg

	hl.GameServerLogicModule = logicmodule.NewGameServerLogicModule()
	hl.gameserverList = msghandler.NewGameServerList()

	return hl
}

func (hl *HallLogic) Name() string {
	return "halllogic"
}

func (hl *HallLogic) Run(ctx *module.MouduleCtx) error {

	hl.Init(ctx, hl.server.GameServerRunCtx.PlayerManager, hl.server.GameServerRunCtx.DBMsgQueue, nil)

	hl.registerEvtHandler(ctx)

	hl.registerRCPMsgHandler(ctx)

	hl.registerMsgHandler(ctx)

	hl.registerNotifyMsgHandler(ctx)

	hl.startUpdateHallUserListService()
	return nil
}

func (hl *HallLogic) Stop() {

}

func (hl *HallLogic) registerEvtHandler(ctx *module.MouduleCtx) {

	ctx.OnConnect(func(clt *network.ConnInfo) bool {
		example.invalid("client %s connnect...", clt)

		if clt.IsServerConn(conf.TCPSERVERNAME_HALL_CLIENT) {
			//通知用户管理器用户连接
			hl.PlayerManager.SetPlayer(player.NewGuest(clt))
		}
		return true
	})
	ctx.OnDisConnect(func(clt *network.ConnInfo) bool {
		example.invalid("client %s disconnnect...", clt)

		//客户端连接
		if clt.IsServerConn(conf.TCPSERVERNAME_HALL_CLIENT) {
			pr := hl.PlayerManager.GetPlayerByConnID(clt.ConnID())

			//登录用户退出
			if pr != nil && !pr.IsGuest() {
				uid := pr.UserID
				cer := pr.Cer

				hl.onUserLogout(uid, cer)
			}

			//通知用户管理器用户断开连接
			hl.PlayerManager.RemovePlayer(clt.ConnID())
		}

		//gameserver连接
		if clt.IsServerConn(conf.TCPSERVERNAME_HALL_GAMESERVER) {
			hl.gameserverList.RemoveGameServerByConn(clt)
		}

		return true
	})
	ctx.OnMessage(func(clt *network.ConnInfo, m *msg.Message) bool {
		//检查服务器状态
		if clt.IsServerConn(conf.TCPSERVERNAME_HALL_CLIENT) {
			if hl.ServerStatus.GetServerStatus() == conf.ServerStatus_Close && !hl.ServerStatus.IsWhiteIP(clt.IPAddress) {
				hl.kickUser(clt.ConnID(), hl.ServerStatus.GetStopServerTipMsg())

				return false
			}
		}

		return true
	})
}

func (hl *HallLogic) registerMsgHandler(ctx *module.MouduleCtx) {
	//平台逻辑转发给dbserver进行处理
	msgid := []*msg.RPCMessageID{
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GuestLoginID), false),                          //游客登录
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UserLoginID), false),                           //账号登录
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_BindZhifubaoID), true),                         //绑定支付宝
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_BindBankCardID), true),                         //绑定銀行卡
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_SendPhoneVCodeID), false),                      //发送验证码
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_ModifyPasswordID), true),                       //修改密码
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_ModifyFaceID), true),                           //修改头像
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_DepositMoneyID), true),                         //存取款
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_BindGuestAccountID), false),                    //绑定游客账号
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_RequestUserHallInfoID), true),                  //请求用户大厅信息
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_FindSetPwdByPhoneID), false),                   //请求找回账号密码
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_ModifyBankPasswordID), true),                   //请求修改银行密码
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_FindSetBankPwdByPhoneID), true),                //请求找回银行密码
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetMailListID), true),                          //请求邮件列表
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetMailDetailID), true),                        //请求邮件详情
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_ContactServiceID), false),                      //请求联系客服
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_ConvertMoneyID), true),                         //兑换金币
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetAgentListID), true),                         //请求代理列表
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_ApplyAgentID), true),                           //请求申请代理
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_ReportAgentID), true),                          //举报代理
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_CreatePayOrderID), true),                       //请求创建充值订单
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetFAQID), true),                               //请求FAQ信息
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetConvertLogID), true),                        //查詢兌換記錄
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetBankListID), true),                          //查詢銀行列表
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_RequestLoginBonusID), true),                    //查詢是否可領取登入獎勵
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_ApplyLoginBonusID), true),                      //領取登入獎勵
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_RequestAgentListID), true),                     //查詢代理資訊
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_JoinAgentListID), true),                        //申請代理下線
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_ApplyAgentListID), true),                       //申請成為代理
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_CheckAgentID), true),                           //查詢申請狀態
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_CheckCSNotifyID), true),                        //未讀客服消息數量
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_ProxyRequestID), true),                         //代理訪問地址
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_ProxyPOSTRequestID), true),                     //代理訪問地址
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UpdateUserNotifyIdxC2HID), true),               //代理訪問地址
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GameCompetitionID), true),                      //查詢查詢比賽活動公告
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GameCompetioniInfoRuleID), true),               //查詢查詢比賽活動規則
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_BuyTicketID), true),                            //買比賽活動票
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetJackpotPoolID), true),                       //取得獎金池
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetUserQACodeID), true),                        //查詢全民代理QRCode
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetUserAgentExecID), true),                     //取得推廣員
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetUserAgentTaxLogID), true),                   //取得今日稅收
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetUserAgentListID), true),                     //推廣人數
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetAgentTaxBonusLogID), true),                  //全民推廣 每日收益
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetAgentTaxListID), true),                      //全民推廣 收益規則
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetAgentUserLevelInfoID), true),                //取得推廣等級
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetShanFuID), true),                            //取得閃付
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetShanFuChatID), true),                        //取得聊天
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetWithdrawListID), true),                      //領取收益
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_NetProfitAgentGetAgentInfoID), true),           //玩家/代理信息(淨利潤)
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_NetProfitAgentGetSubUserListID), true),         //新增玩家(淨利潤)
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_NetProfitAgentGetAgentWithdrawAmountID), true), //代理提現紀錄(淨利潤)
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_NetProfitAgentGetWithdrawAmountID), true),      //提現紀錄(淨利潤)
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_NetProfitAgentGetBonusAmountID), true),         //可提現金額查詢(淨利潤)
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_NetProfitAgentWithDrawOrderID), true),          //提現(淨利潤)
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_NetProfitAgentApplyListID), true),              //審核代理申請列表(淨利潤)
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_NetProfitAgentGetAgentDetailID), true),         // USER 推廣信息(淨利潤)
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_NetProfitAgentGetAgentReportLogID), true),      // USER 新增代理(淨利潤)
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_NetProfitAgentGetWithdrawDetailID), true),      // USER 新增代理(淨利潤)
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UserAgentUserModeGetID), true),                 // USER 取得已開的模式
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetWithdrawOrderID), true),                     // 馬上提現
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UserAgentGetWithAmountID), true),               // 申請提現(全民代理)
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UserAgentGetWithAmountListID), true),           // 申請提現例表(全民代理)
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UserAgentGetWithdrawAmountListID), true),       // 申請提現例表(我的提現)
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UserAgentResultListID), true),                  // 我的業績(全民代理)
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UserAgentBorrowListID), true),                  // 我的預提(全民代理)
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UserAgentWinAmountListID), true),               // 我的獎勵(全民代理)
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetMyPromotionID), true),                       // 無限級QRCODE
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetRewardTipsID), true),                        // 無限級獎勵說明
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UnlimitConvertOrderID), true),                  // 無限級按鈕1
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UnlimitPreConvertOrderID), true),               // 無限級按鈕2
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_SplitGetMyInfoID), true),                       // 裂變1推廣信息
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_SplitGetMyConvertID), true),                    // 裂變2提現紀錄
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_SplitGetRewardTipsID), true),                   // 裂變3說明
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_SplitGetMyRewardID), true),                     // 裂變4馬上提現
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_SplitConvertOrderID), true),                    // 裂變5提現紀錄送出提現金額
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_SplitBindingInvitedCodeID), true),              // 裂變6輸入推廣碼
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_EventChargeBonusAmountID), true),               // 充值返利活動  取得個人流水及限制
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UnlimitAddUserRelationID), true),               // USER 加入下线(无限级)
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_NetProfitAgentAddUserRelationID), true),        // USER 加入下线(净利润)
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_SplitAddUserRelationID), true),                 // USER 加入下线(裂变)
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UserAgentAddUserRelationID), true),             // USER 加入下线(全民代理)
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_ExchangeCouponID), true),                       // Exchange Coupon
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_ModifyAvatarFrameID), true),                    // 請求 修改VIP頭像框ID
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetGradeSettingDataID), true),                  // 請求 VIP 獎勵等級設定資料
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_CampaignGetListID), true),                      // 請求用戶活動清單
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_FirstDepositGetInfoID), true),                  // 取得首充返利活動與該用戶相關資料
		msg.NewRPCMessageID(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_FirstDepositCollectID), true),                  // 領取首充返利回饋金
	}

	rpchandler := logicmodule.NewRouteRPCMsgHandler(hl.DBMsgQueue, ctx, hl.PlayerManager, msgid, int32(hl.server.ServerID))
	hl.MsgHandler.AddHandler(rpchandler) //rpc 转发

	gsmgmhandler := msghandler.NewGsMgmHandler(hl.gameserverList, hl.serverCfg.GSMgmAddr) //gameserver 管理
	hl.MsgHandler.AddHandler(gsmgmhandler)
	//服务器集合信息
	hl.gserverProvider = msghandler.NewRemoteRegGameServerProvider(gsmgmhandler, msghandler.NewDefaultServerAllocStrategy(hl.serverCfg.StrategyList))

	//平台版本号
	hl.MsgHandler.AddHandler(msghandler.NewGetHallVerHandler(hl.hallverProvider))
	//服务器列表

	hl.MsgHandler.AddHandler(msghandler.NewGameServerListHandler(hl.gserverProvider, hl.hallverProvider))
	//服务器分配
	hl.MsgHandler.AddHandler(msghandler.NewAllocGameServerHandler(hl.gserverProvider))

	//代理訪問支付
	hl.MsgHandler.AddHandler(msghandler.NewProxyRequestHandler(hl.PlayerManager))
	hl.MsgHandler.AddHandler(msghandler.NewProxyPOSTRequestHandler(hl.PlayerManager))
	//更新用戶推播 idx
	hl.MsgHandler.AddHandler(msghandler.NewUpdateUserNotifyIdxHandler(hl.server, hl.PlayerManager))

}

func (hl *HallLogic) registerRCPMsgHandler(ctx *module.MouduleCtx) {
	//dbserver处理平台逻辑结果转发给客户端
	msgid := []*msg.MessageID{
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_LoginRetID)},                                //登录结果
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_BindZhifubaoRetID)},                         //绑定支付宝结果
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_SendPhoneVCodeRetID)},                       //发送验证码结果
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_ModifyPasswordRetID)},                       //修改密码结果
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_SendPhoneVCodeRetID)},                       //发送验证码结果
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_ModifyFaceRetID)},                           //修改头像结果
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_DepositMoneyRetID)},                         //存取款结果
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_BindGuestAccountRetID)},                     //绑定游客账号结果
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UserHallInfoID)},                            //返回用户大厅信息
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_FindSetPwdByPhoneRetID)},                    //返回找回密码信息
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_ModifyBankPasswordRetID)},                   //返回找回密码信息
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_FindSetBankPwdByPhoneRetID)},                //返回找回银行密码
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetMailListRetID)},                          //返回邮件列表
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetMailDetailRetID)},                        //返回邮件详情
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_ContactServiceRetID)},                       //返回联系客服
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_ConvertMoneyRetID)},                         //返回兑换金币
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetAgentListRetID)},                         //返回代理列表
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_ApplyAgentRetID)},                           //返回申请代理
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_ReportAgentRetID)},                          //返回举报代理
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_CreatePayOrderRetID)},                       //返回创建充值订单结果
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetFAQRetID)},                               //返回FAQ信息
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetBankListRetID)},                          //返回銀行列表
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetConvertLogRetID)},                        //返回兌換記錄
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_BindBankCardRetID)},                         //綁定銀行卡結果
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_RequestLoginBonusRetID)},                    //返回登入獎勵結果
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_RequestAgentListRetID)},                     //回傳代理資訊
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_JoinAgentListRetID)},                        //回傳申請代理結果
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_ApplyAgentListRetID)},                       //回傳是否成功
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_CheckAgentRetID)},                           //回傳查詢狀態
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_CheckCSNotifyRetID)},                        //回傳未讀客服消息數量
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GameCompetitionRetID)},                      //回傳比賽活動公告
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GameCompetioniInfoRuleRetID)},               //回傳比賽規則
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_BuyTicketRetID)},                            //回傳買票結果
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetJackpotPoolRetID)},                       //回傳獎金池
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetUserQACodeRetID)},                        //回傳全民代理QRCode
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetUserAgentExecRetID)},                     //回傳推廣員
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetUserAgentTaxLogRetID)},                   //回傳今日稅收
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetUserAgentListRetID)},                     //回傳推廣人數
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetAgentTaxBonusLogRetID)},                  //回傳全民推廣 每日收益
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetAgentTaxListRetID)},                      //回傳全民推廣 收益規則
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetAgentUserLevelInfoRetID)},                //回傳推廣等級
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetShanFuRetID)},                            //回傳閃付
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetShanFuChatRetID)},                        //回傳閃付聊天
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetWithdrawListRetID)},                      //領取收益
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_NetProfitAgentGetAgentInfoRetID)},           //玩家/代理信息(淨利潤)
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_NetProfitAgentGetSubUserListRetID)},         ///新增玩家(淨利潤)
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_NetProfitAgentGetAgentWithdrawAmountRetID)}, //代理提現紀錄(淨利潤)
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_NetProfitAgentGetWithdrawAmountRetID)},      //提現紀錄(淨利潤)
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_NetProfitAgentGetBonusAmountRetID)},         //可提現金額查詢(淨利潤)
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_NetProfitAgentWithDrawOrderRetID)},          //提現(淨利潤)
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_NetProfitAgentApplyListRetID)},              //審核代理申請列表(淨利潤)
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_NetProfitAgentGetAgentDetailRetID)},         // USER 推廣信息(淨利潤)
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_NetProfitAgentGetAgentReportLogRetID)},      // USER 新增代理(淨利潤)
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_NetProfitAgentGetWithdrawDetailRetID)},      // USER 新增代理(淨利潤)
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UserAgentUserModeGetRetID)},                 // USER 取得已開的模式
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetWithdrawOrderRetID)},                     // 馬上提現
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UserAgentGetWithAmountRetID)},               // 申請提現(全民代理)
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UserAgentGetWithAmountListRetID)},           // 申請提現例表(全民代理)
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UserAgentGetWithdrawAmountListRetID)},       // 申請提現例表(我的提現)
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UserAgentResultListRetID)},                  // 我的業績(全民代理)
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UserAgentBorrowListRetID)},                  // 我的預提(全民代理)
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UserAgentWinAmountListRetID)},               // 我的獎勵(全民代理)
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetMyPromotionRetID)},                       // 無限級QRCODE
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetRewardTipsRetID)},                        // 無限級獎勵說明
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UnlimitConvertOrderRetID)},                  // 無限級按鈕1
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UnlimitPreConvertOrderRetID)},               // 無限級按鈕2
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_SplitGetMyInfoRetID)},                       // 裂變1推廣信息
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_SplitGetMyConvertRetID)},                    // 裂變2提現紀錄
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_SplitGetRewardTipsRetID)},                   // 裂變3說明
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_SplitGetMyRewardRetID)},                     // 裂變4馬上提現
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_SplitConvertOrderRetID)},                    // 裂變5提現紀錄送出提現金額
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_SplitBindingInvitedCodeRetID)},              // 裂變6輸入推廣碼
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_EventChargeBonusAmountRetID)},               // 充值返利活動  取得個人流水及限制
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UnlimitAddUserRelationRetID)},               // USER 加入下线(无限级)
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_NetProfitAgentAddUserRelationRetID)},        // USER 加入下线(净利润)
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_SplitAddUserRelationRetID)},                 // USER 加入下线(裂变)
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UserAgentAddUserRelationRetID)},             // USER 加入下线(全民代理)
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_ExchangeCouponRetID)},                       // Exchange Coupon
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_ModifyAvatarFrameRetID)},                    // 回傳 修改VIP頭像框ID
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetGradeSettingDataRetID)},                  // 回傳 VIP 獎勵等級設定資料
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_CampaignGetListRetID)},                      // 回傳用戶活動清單
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_FirstDepositGetInfoRetID)},                  // 回傳 取得首充返利活動與該用戶相關資料
		{int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_FirstDepositCollectRetID)},                  // 回傳 領取首充返利回饋金
	}
	hl.RPCMsgHandler.AddHandler(logicmodule.NewRPCRetHookHandler(msgid)) //转发客户端消息

	hl.RPCMsgHandler.AddHandler(rpchandler.NewLogonRetHandler(hl.PlayerManager, hl.onUserLogout, hl.kickUser)) //用户登录处理

	//大厅启动向dbserver注册
	hallreghandler := rpchandler.NewHallServerRegHandler(hl.serverCfg.HallAddr, hl.serverCfg.NotifyServerAddr, "", int32(hl.server.ServerID), hl.DBMsgQueue, hl.server.NotifyHandler)
	hl.RPCMsgHandler.AddHandler(hallreghandler)
	hallreghandler.SendDBServerStartMsg()
	//平台版本号管理
	hl.hallverProvider = rpchandler.NewRPCHallVerProvider(hallreghandler, hl.PlayerManager)
	//平台版本号加载器
	hl.hallVerLoader = rpchandler.NewRPCHallVerLoader(hallreghandler)
	command.AddCommand(rpchandler.NewHallVerCmd(hl.hallverProvider))

}

//注册通知处理器
func (hl *HallLogic) registerNotifyMsgHandler(ctx *module.MouduleCtx) {
	//公告
	hl.NotifyMsgHandler.AddHandler(notifyhandler.NewAnnNotifyHandler())
	//捕魚公告
	hl.NotifyMsgHandler.AddHandler(notifyhandler.NewAnnNotifyFishHandler())
	//用户属性变化
	hl.NotifyMsgHandler.AddHandler(notifyhandler.NewAttrNotifyHandler())
	//强制刷新版本号
	hl.NotifyMsgHandler.AddHandler(notifyhandler.NewRefreshVerHandler(hl.hallVerLoader))
	//大廳用戶到帳通知
	hl.NotifyMsgHandler.AddHandler(notifyhandler.NewNotifyPayOrderHandler())
	//大廳用戶推播消息
	hl.NotifyMsgHandler.AddHandler(notifyhandler.NewUserNotifyHandler())
}

//用户登出大厅
func (hl *HallLogic) onUserLogout(uid int32, cer string) {
	logoutHallMsg := new(msg.Message)
	logoutHallMsg.BClassID = int32(netproto.MessageBClassID_Hall)
	logoutHallMsg.SClassID = int32(netproto.HallMsgClassID_UserLogoutID)

	logoutHallMsgData := new(netproto.UserLogout)
	logoutHallMsgData.UserID = proto.Int32(uid)
	logoutHallMsgData.Cer = proto.String(cer)
	logoutHallMsg.MsgData = logoutHallMsgData

	hl.DBMsgQueue.PushMessage(netproto.CreateEmptyRPCInfo(int32(hl.server.ServerID)), false, logoutHallMsg)
}

//踢出用户
func (hl *HallLogic) kickUser(connid string, msgstr string) {
	example.invalid("%s,断开客户端%s连接", msgstr, connid)

	hl.sendTipMsg(connid, msgstr)

	go func() {
		time.Sleep(time.Duration(1) * time.Second)

		hl.MsgServer.CloseConnByConnID(connid)
	}()

}

//发送提示消息
func (hl *HallLogic) sendTipMsg(connid string, msgstr string) {
	m, _ := hallutil.GetTipMessage(msgstr, hallutil.MsgTypeID_HallKick)

	hl.serverCtx.MsgServer.SendMessageByConnID(connid, m)
}

// 消息通知服務
// 定時向 NotifyServer 更新目前大廳在線的用戶
func (hl *HallLogic) startUpdateHallUserListService() {
	go func() {
		defer util.PanicErrStack()
		for {
			time.Sleep(time.Duration(5) * time.Second)
			HallUserList := new(netproto.HallUserList)
			HallUserList.ServerID = proto.Int32(hl.serverCfg.ServerID)
			hl.PlayerManager.Range(func(p *player.Player) {
				if p.IsOnline {
					HallUserList.UserIDList = append(HallUserList.UserIDList, int32(p.UserID))
				}
			})
			msg := new(msg.Message)
			msg.BClassID = int32(netproto.MessageBClassID_NotifyServer)
			msg.SClassID = int32(netproto.NotifyServerClassID_UpdateHallUserListID)
			msg.MsgData = HallUserList
			hl.server.NotifyHandler.TcpMsgClient.SendMessage(msg)
		}
		mlog.Error("---退出更新大廳用戶線程---")
	}()
}
