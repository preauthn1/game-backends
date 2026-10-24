package robotaction

import (
	"command"
	"event"
	"gamelogic"
	"mlog"
	"module"
	"module/logicmodule"
	"msg"
	"netproto"
	"robot/robotcomm"
	"rpcmsg"
	"scorestorage"
)

import proto "example.invalid/golang/protobuf/proto"

type RobotKicker interface {
	LogoutRobotUser(user *gamelogic.GameUser)
}

//机器人加载器
type RobotLoader struct {
	rpcMsgQueue           *rpcmsg.RPCMsgQueue           //db消息队列
	robotManager          *robotcomm.RobotManager       //机器人管理器
	loadRobotInterval     int32                         //加载时间间隔
	timeFuncManager       *event.TimerFuncManager       //timer管理器
	minMoneyAmount        int64                         //机器人最小金币数
	maxMoneyAmount        int64                         //机器人最大金币数
	serverID              int32                         //服务器ID
	scoreStorage          scorestorage.UserScoreStorage //计分器
	robotActionController gamelogic.ActionController    //用户行为控制器
	serverStatus          logicmodule.ServerStatusProvider
}

//创建机器人加载器对象
func NewRobotLoader(serverStatus logicmodule.ServerStatusProvider) *RobotLoader {
	rloader := new(RobotLoader)
	rloader.serverStatus = serverStatus

	return rloader
}

//启动机器人加载器
func (rl *RobotLoader) Start(ctx *RobotActionHandlerCtx) {
	rl.rpcMsgQueue = ctx.RpcMsgQueue
	rl.robotManager = ctx.RobotManager
	rl.loadRobotInterval = ctx.RobotCfg.LogonInterval
	rl.timeFuncManager = ctx.TfManager
	rl.minMoneyAmount = ctx.RobotCfg.MinMoneyAmount
	rl.maxMoneyAmount = ctx.RobotCfg.MaxMoneyAmount
	rl.serverID = ctx.Room.ServerID
	rl.scoreStorage = ctx.ScoreStorage
	rl.robotActionController = ctx.ActionController
	mlog.System("加载机器人進程開始")
	//添加机器人命令
	command.AddCommand(NewRobotCmd(rl.robotManager, rl))

	rl.timeFuncManager.NewTicker(rl.loadRobotInterval, func() {
		if ctx.ServerStatus.IsServerRuning() {
			rl.balanceRobot()
		}
	})
}

//平衡机器人
func (rl *RobotLoader) balanceRobot() {
	//如果服务器已经停止
	if !rl.serverStatus.IsServerRuning() {
		rl.logoutRobot(int32(rl.robotManager.RobotCount()))

		return
	}

	crcount := int32(rl.robotManager.RobotCount())

	//example.invalid("开始加载机器人,当前机器人数%d", crcount)

	logonRobotMsg := new(msg.Message)
	logonRobotMsg.BClassID = int32(netproto.MessageBClassID_DBServer)
	logonRobotMsg.SClassID = int32(netproto.DBServerClassID_LogonRobotID)

	logonRobotMsgData := new(netproto.LogonRobot)
	logonRobotMsgData.CurrentRobotCount = proto.Int32(crcount)
	logonRobotMsgData.MinMoneyAmount = proto.Int64(rl.minMoneyAmount)
	logonRobotMsgData.MaxMoneyAmount = proto.Int64(rl.maxMoneyAmount)
	logonRobotMsgData.ServerID = proto.Int32(rl.serverID)
	logonRobotMsg.MsgData = logonRobotMsgData

	rl.rpcMsgQueue.PushMessage(netproto.CreateEmptyRPCInfo(rl.serverID), false, logonRobotMsg)
}

func (rl *RobotLoader) OnRPCMessage(m *msg.Message, ctx *module.MouduleCtx) {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.LogonRobotRet{}
		return mm
	})

	if rmm == nil {
		return
	}

	mm := rmm.(*netproto.LogonRobotRet)

	if mm == nil {
		return
	}
	//获取需要加载的机器人数量
	robotCount := mm.GetRobotCount()

	//不做处理
	if robotCount == 0 {
		mlog.Debug("收到机器人加载数据.但当前不需要登入登出机器人.")
		return
	}
	//登出机器人
	if robotCount < 0 {
		logoutCount := -1 * robotCount

		example.invalid("登出%d个机器人", logoutCount)

		rl.logoutRobot(logoutCount)
	} else { //登入机器人
		logonCount := robotCount
		//mlog.NewDefaultLogger().System("登入%d个机器人", logonCount)
		//example.invalid("登入%d个机器人", logonCount)

		rl.logonRobot(logonCount, mm.GetUserData())
	}

}

//登入机器人
func (rl *RobotLoader) logonRobot(cn int32, users []*netproto.RobotData) {
	for i := 0; i < int(cn); i++ {
		if len(users) < i+1 {
			example.invalid("服务器返回需要登录%d个机器人,但机器人数量只有%d", cn, len(users))
			return
		}

		urdata := users[i]

		if rl.robotManager.GetRobot(urdata.GetUserID()) != nil {
			mlog.Warn("加载机器人%v,但机器人已经在当前服务器中", urdata)

			continue
		}

		nur := gamelogic.NewGameUser()
		nur.ConnInfo = nil
		nur.WinCount = urdata.GetWinCount()
		nur.DrawCount = urdata.GetDrawCount()
		nur.LoseCount = urdata.GetLoseCount()

		nur.HDCode = "000000"
		nur.HDType = 1

		nur.UserID = urdata.GetUserID()
		nur.NickName = urdata.GetNickName()
		nur.Score = urdata.GetCashAmount()
		nur.LoginScore = urdata.GetCashAmount()
		nur.UserSex = urdata.GetSex()
		//设定用户类型为机器人
		nur.UserType = int32(gamelogic.UserType_Rebot) // urdata.GetUserType()
		nur.FaceID = urdata.GetFaceID()
		nur.RobotLevel = urdata.GetRobotLevelID()
		nur.AvatarFrameID = urdata.GetAvatarFrameID()

		if urdata.GetUserType() != int32(gamelogic.UserType_Rebot) {
			mlog.Warn("加载机器人信息%v,但用户类型数据不正确", nur)
		}

		nur.ConnInfo = robotcomm.NewRobotConnInfo()

		robot := new(robotcomm.Robot)
		robot.UserInfo = nur
		robot.RebotLevel = urdata.GetRobotLevelID()

		rl.robotManager.AddRobot(robot)

		//example.invalid("机器人%v进入房间", robot)
	}
}

//登出机器人
func (rl *RobotLoader) logoutRobot(cn int32) {
	canLogoutRobot := rl.robotManager.GetNotGamingRobot()
	mlog.Debug("需登出的機器人數量 %v, 可被登出的機器人數量 %v", cn, len(canLogoutRobot))
	i := int32(0)
	for _, r := range canLogoutRobot {
		if r.UserInfo.IsFree() {
			continue
		}
		rl.robotActionController.UserFree(r.UserInfo)
		i++
		if i >= cn {
			break
		}
	}
	logoutStop := int32(0)
	for i := int32(0); i < cn; i++ {
		robot := rl.robotManager.GetFreeRobot(gamelogic.FreeTableNo)

		if robot == nil {
			logoutStop = i
			break
		}

		rl.LogoutRobotUser(robot.UserInfo)
	}

	robotTotal := rl.robotManager.RobotCount()
	needMore := cn - logoutStop
	if needMore > 0 && robotTotal > 0 {
		//example.invalid("目前機器人數量%d, 還需登出 %d 個機器人這些機器人將在遊戲結束後登出", robotTotal, needMore)
		for _, r := range rl.robotManager.GetAllRobot() {
			if needMore < 1 {
				break
			}
			r.SetLeave()
			needMore--
		}
	}
}

func (rl *RobotLoader) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_DBServer) && sclsid == int32(netproto.DBServerClassID_LogonRobotRetID)
}

//登出机器人
func (rl *RobotLoader) LogoutRobotUser(user *gamelogic.GameUser) {
	//机器热在游戏中,不登出
	if user.IsGameing() {
		return
	}

	//如果用户坐下状态,让用户站立
	if user.TableNo != gamelogic.FreeTableNo {
		rl.robotActionController.UserFree(user)
	}

	//移除机器人
	rl.robotManager.RemoveRobot(user.UserID)

	//登出机器人
	rl.scoreStorage.OnUserLeave(user)

	example.invalid("机器人%v离开房间", user)
}
