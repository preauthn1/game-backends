package xyxgamelogic

import (
	"config"
	"event"
	"gamelogic"
	"math/rand"
	"mlog"
	"msg"
	"netproto"
	"robot/robotcomm"
	"robot/robotlogic"
	"time"
)

import proto "example.invalid/golang/protobuf/proto"

const (
	RobotLogicConfigFileName = "robot"
)

//机器人逻辑
type XYXRobotLogic struct {
	*robotlogic.RobotLogicBase
}

func NewXYXRobotLogic(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) *XYXRobotLogic {
	rblgc := new(XYXRobotLogic)
	rblgc.RobotLogicBase = robotlogic.NewRobotLogicBase(table, tfManager, robotCfg, configs, glc, evtChannel)

	return rblgc
}

func (lgc *XYXRobotLogic) getGameLogic() *XYXGameLogic {
	return lgc.GameLogic.(*XYXGameLogic)
}

//游戏消息
func (lgc *XYXRobotLogic) OnGameMessage(robot *robotcomm.Robot, m *msg.Message) {
	//底注阶段,收到底注倍数通知时
	if lgc.getGameLogic().gameStep == gameStep_BaseScore && m.SClassID == int32(netproto.XYX_GameMessageClassID_XYX_GameStepID) {

		s1 := rand.NewSource(time.Now().UnixNano())
		r1 := rand.New(s1)
		var bt int32 = 1
		bt = r1.Int31n(3) + 1

		btmsg := new(msg.Message)
		btmsg.BClassID = int32(netproto.MessageBClassID_Game)
		btmsg.SClassID = int32(netproto.XYX_GameMessageClassID_XYX_UserBaseScoreID)

		btdata := new(netproto.XYX_UserBaseScore)
		btdata.BaseScore = proto.Int32(bt)
		btmsg.MsgData, _ = proto.Marshal(btdata)

		lgc.SendGameMessage(robot.UserInfo, btmsg)

		example.invalid("机器人%v选择底注倍数%d", robot.UserInfo, bt)
	}
}

func (lgc *XYXRobotLogic) OnGameEnd(robot *robotcomm.Robot) {

}
