package dal

import (
	"strconv"
	"strings"
)

import proto "example.invalid/golang/protobuf/proto"

import (
	"dal/msghandler"
	"fmt"
	"mlog"
	"module"
	"msg"
	"network"
	"rpcmsg"
)

//大厅逻辑
type DBLogic struct {
	mouduleCtx *module.MouduleCtx
	msgHandler *module.SemaphoreHookMsgHandlerGroup
}

func NewDBLogicModule(n int) *DBLogic {
	db := new(DBLogic)
	db.msgHandler = module.NewSemaphoreHookMsgHandlerGroup(n)

	return db
}

func (dl *DBLogic) Name() string {
	return "DBLogic"
}

func (dl *DBLogic) Run(ctx *module.MouduleCtx) error {
	//设置数据库连接
	msghandler.ConnString = ctx.GetConfigData("db", "conn", "connstring")
	msghandler.ConnString = strings.Replace(msghandler.ConnString, "\"", "", -1)

	msghandler.DealLogConnString = ctx.GetConfigData("db", "conn", "deallogconnstring")
	msghandler.DealLogConnString = strings.Replace(msghandler.DealLogConnString, "\"", "", -1)
	msghandler.DealLogCompany1ConnStr = ctx.GetConfigData("db", "conn", "company1deallogstr")
	msghandler.DealLogCompany1ConnStr = strings.Replace(msghandler.DealLogCompany1ConnStr, "\"", "", -1)
	msghandler.ClubConnString = ctx.GetConfigData("db", "conn", "clubconnstring")
	msghandler.ClubConnString = strings.Replace(msghandler.ClubConnString, "\"", "", -1)
	msghandler.WildCatConnStr = ctx.GetConfigData("db", "conn", "wildcatconnstr")
	msghandler.WildCatConnStr = strings.Replace(msghandler.WildCatConnStr, "\"", "", -1)
	msghandler.CompanyStr = ctx.GetConfigData("db", "conn", "company1")
	msghandler.CompanyStr = strings.Replace(msghandler.CompanyStr, "\"", "", -1)

	msghandler.SMSWebAPI = ctx.GetConfigData("db", "conn", "smswebapi")

	//设置连接池大小
	psize := ctx.GetConfigData("db", "conn", "pool")
	ipsize, _ := strconv.Atoi(psize)
	msghandler.PoolSize = ipsize

	msghandler.InitDatabase()

	dl.mouduleCtx = ctx

	dl.registerMsgParser(ctx)
	dl.registerMsgHandler(ctx)

	dl.registerEvtHandler(ctx)

	return nil
}

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
		//example.invalid("client %s msg:%d_%d", clt, m.BClassID, m.SClassID)

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
	dl.msgHandler.AddHandler(new(msghandler.GuestLogonHandler))                     //游客登录
	dl.msgHandler.AddHandler(new(msghandler.UserLogonHandler))                      //用户登录
	dl.msgHandler.AddHandler(new(msghandler.UserLeaveHallHandler))                  //登出广场
	dl.msgHandler.AddHandler(new(msghandler.ServerRegHandler))                      //服务器注册
	dl.msgHandler.AddHandler(new(msghandler.RoomLogonHandler))                      //登录房间
	dl.msgHandler.AddHandler(new(msghandler.RoomLogoutHandler))                     //登出房间
	dl.msgHandler.AddHandler(new(msghandler.UpdateUserScoreHandler))                //即时写分
	dl.msgHandler.AddHandler(new(msghandler.UpdateExperienceScoreHandler))          //體驗金幣
	dl.msgHandler.AddHandler(new(msghandler.HallServerStartHandler))                //大厅启动
	dl.msgHandler.AddHandler(new(msghandler.GameServerStartHandler))                //房间启动
	dl.msgHandler.AddHandler(new(msghandler.BindZhifubaoHandler))                   //绑定支付宝
	dl.msgHandler.AddHandler(new(msghandler.BindBankCardHandler))                   //绑定銀行卡
	dl.msgHandler.AddHandler(new(msghandler.ModifyPasswordHandler))                 //修改密码
	dl.msgHandler.AddHandler(new(msghandler.SendPhoneVCodeHandler))                 //发送验证码
	dl.msgHandler.AddHandler(new(msghandler.ModifyFaceHandler))                     //修改头像
	dl.msgHandler.AddHandler(new(msghandler.MoneyDepositHandler))                   //存取款
	dl.msgHandler.AddHandler(new(msghandler.BindGuestAccountHandler))               //绑定游客账号
	dl.msgHandler.AddHandler(new(msghandler.RequestUserInfoHandler))                //请求用户大厅信息
	dl.msgHandler.AddHandler(new(msghandler.FindSetPwdByPhoneHandler))              //请求找回账号密码
	dl.msgHandler.AddHandler(new(msghandler.ModifyBankPasswordHandler))             //请求修改银行密码
	dl.msgHandler.AddHandler(new(msghandler.FindSetBankPwdByPhoneHandler))          //请求找回银行账号密码
	dl.msgHandler.AddHandler(new(msghandler.GetMailListHandler))                    //请求邮件列表
	dl.msgHandler.AddHandler(new(msghandler.GetMailDetailHandler))                  //请求邮件详情
	dl.msgHandler.AddHandler(new(msghandler.ContactServiceHandler))                 //发送联系客服信息
	dl.msgHandler.AddHandler(new(msghandler.ConvertMoneyHandler))                   //兑换金币
	dl.msgHandler.AddHandler(new(msghandler.GetAgentListHandler))                   //请求代理列表
	dl.msgHandler.AddHandler(new(msghandler.ApplyAgentHandler))                     //申请代理
	dl.msgHandler.AddHandler(new(msghandler.ReportAgentHandler))                    //举报代理
	dl.msgHandler.AddHandler(new(msghandler.CreatePayOrderHandler))                 //创建充值订单
	dl.msgHandler.AddHandler(new(msghandler.BlockIPHandler))                        //封锁IP
	dl.msgHandler.AddHandler(new(msghandler.LoadBlockIPListHandler))                //加载封锁IP列表
	dl.msgHandler.AddHandler(new(msghandler.AddPerGameLogHandler))                  //写每局记录
	dl.msgHandler.AddHandler(new(msghandler.RobotLogonHandler))                     //加载机器人
	dl.msgHandler.AddHandler(new(msghandler.GetFAQHandler))                         //加载机器人
	dl.msgHandler.AddHandler(new(msghandler.GetUserLuckScoreHandler))               //获取用户幸运值
	dl.msgHandler.AddHandler(new(msghandler.GetConvertLogHandler))                  //查詢兌換記錄
	dl.msgHandler.AddHandler(new(msghandler.GetBankListHandler))                    //查詢銀行列表
	dl.msgHandler.AddHandler(new(msghandler.GameVerHandler))                        //更新遊戲版本號
	dl.msgHandler.AddHandler(new(msghandler.RequestLoginBonusHandler))              //登入獎勵操作
	dl.msgHandler.AddHandler(new(msghandler.RequestAgentDetailHandler))             //取得代理資訊
	dl.msgHandler.AddHandler(new(msghandler.CSNotifyCountHandler))                  //未讀客服消息數量
	dl.msgHandler.AddHandler(new(msghandler.SuperToolAddAssignRewardHandler))       //新增指定多局開獎 <--- 多此一舉
	dl.msgHandler.AddHandler(new(msghandler.SuperToolGetAssignRewardDetailHandler)) //獲取指定開獎項目
	dl.msgHandler.AddHandler(new(msghandler.SuperToolUpdAssignRewardDetailHandler)) //更新指定開獎項目結果
	dl.msgHandler.AddHandler(new(msghandler.SuperToolAddChangeLogHandler))          //操作獎池及指定放分的記錄
	dl.msgHandler.AddHandler(new(msghandler.SuperToolGetVipUserHandler))            //獲取監控用戶列表
	dl.msgHandler.AddHandler(new(msghandler.SuperToolUpdVIPUserHandler))            //更新監控用戶數據
	dl.msgHandler.AddHandler(new(msghandler.SuperToolFishUpdSystemRewardHandler))   //捕魚更新系統放分和取得系統放分
	dl.msgHandler.AddHandler(new(msghandler.SuperToolFishUpdVIPUserHandler))        //捕魚個人放分
	dl.msgHandler.AddHandler(new(msghandler.SuperToolZjhGetVipUserHandler))         //ZJH獲取監控用戶列表
	dl.msgHandler.AddHandler(new(msghandler.SuperToolZjhUpdVIPUserHandler))         //ZJH更新監控用戶數據
	dl.msgHandler.AddHandler(new(msghandler.FishSysPoolHandler))                    //Fish 新增/更新系統水池输赢记录
	dl.msgHandler.AddHandler(new(msghandler.FishPlayerPoolHandler))                 //新增/更新玩家个人水池输赢记录
	dl.msgHandler.AddHandler(new(msghandler.GetBeginnerOfferHandler))               //取得玩家新手優惠
	dl.msgHandler.AddHandler(new(msghandler.UpdateBeginnerOfferHandler))            //更新玩家新手優惠
	dl.msgHandler.AddHandler(new(msghandler.GameCompetitionHandler))                //查詢比賽活動公告
	dl.msgHandler.AddHandler(new(msghandler.GameCompetitionRuleHandler))            //查詢比賽活動規則
	dl.msgHandler.AddHandler(new(msghandler.UpdatePiSaiScoreHandler))               //比賽積分写分
	dl.msgHandler.AddHandler(new(msghandler.GameCompetitionStartHandler))           //報名參加比賽活動
	dl.msgHandler.AddHandler(new(msghandler.GetContestUserDataHandler))             //取得比賽活動用戶資料
	dl.msgHandler.AddHandler(new(msghandler.UpdateContestUserDataHandler))          //更新比賽活動用戶資料
	dl.msgHandler.AddHandler(new(msghandler.GetJackpotPoolHandler))                 //取得Slot Jackpot 水池
	dl.msgHandler.AddHandler(new(msghandler.UpdateJackpotPoolHandler))              //更新Slot Jackpot 水池
	dl.msgHandler.AddHandler(new(msghandler.Cd21SysPoolHandler))                    //21點 新增/更新系統水池输赢记录
	dl.msgHandler.AddHandler(new(msghandler.GetGameJackpotPoolHandler))             //取得遊戲獎金池
	dl.msgHandler.AddHandler(new(msghandler.UpdateGameJackpotPoolHandler))          //更新遊戲獎金池
	dl.msgHandler.AddHandler(new(msghandler.UpdateGameJackpotPoolHandler))          //更新遊戲獎金池
	dl.msgHandler.AddHandler(new(msghandler.UpdateUserJackpotBoxHandler))                  //遊戲獎金寫入保險箱
}
