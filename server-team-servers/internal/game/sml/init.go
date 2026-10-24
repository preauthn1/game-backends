package sml

import (
	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/app"
	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/robot/robotcomm"
	"example.invalid/ServerTeam/Servers/pkg/robot/robotlogic"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
)

// GameFunc 取得 LHD Game 方式
type GameFunc func(gamelogic.GameLogic) *Logic

// GameLogicCreator 遊戲邏輯產生器
var GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.Group) gamelogic.GameLogic {
	return NewLogic(t, tfManager, cfgs)
}

// RobotLogicCreator 機器人邏輯產生器
var RobotLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, rbtConf *robotcomm.RobotConfig, confs *config.Group, glc gamelogic.GameLogic, eChannl *event.Dispatcher) robotlogic.RobotLogic {
	return NewRobotLogic(t, tfManager, rbtConf, confs, glc, eChannl)
}

var SuperToolCreator = func(room *gamelogic.Room, tfm *event.TimerFuncManager, dbMsgQueue *rpcmsg.Queue) gamelogic.ISuperToolService {
	return gamelogic.NewBrGameSuperToolServiceBase(room, tfm, dbMsgQueue)
}

func init() {
	app.Context.Provide(func() GameFunc {
		return func(g gamelogic.GameLogic) *Logic { return g.(*Logic) }
	})

	msg.InstanceProvider.Register(int32(netproto.MessageBClassID_Game), int32(netproto.SML_GameMessageClassID_SMLBetID), func() proto.Message { return new(netproto.SML_BetRequest) })
	msg.InstanceProvider.Register(int32(netproto.MessageBClassID_Game), int32(netproto.SML_GameMessageClassID_SMLBetClearID), func() proto.Message { return new(netproto.SML_BetRequest) })
}
