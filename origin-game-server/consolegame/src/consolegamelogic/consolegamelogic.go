package consolegamelogic

import (
	"config"
	"event"
	"gamelogic"
	"mlog"
	"msg"
	"netproto"
)

import proto "example.invalid/golang/protobuf/proto"

type ConsoleGameLogic struct {
	*gamelogic.FixedGameLogicBase
}

//创建控制台游戏逻辑
func NewConsoleGameLogic(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) *ConsoleGameLogic {
	glc := new(ConsoleGameLogic)
	glc.FixedGameLogicBase = gamelogic.NewFixedGameLogicBase(t, tfManager, cfgs)

	return glc
}

func (glc *ConsoleGameLogic) GameMessage(user *gamelogic.GameUser, gmsg *msg.Message) {
	mlog.Debug("game message.%v", user)

	//聊天
	if gmsg.BClassID == int32(netproto.MessageBClassID_Game) && gmsg.SClassID == int32(netproto.GameMessageClassID_TestGameChatID) {
		f := func() {
			msgdata := gmsg.MsgData.([]byte)

			d := &netproto.TestGameChat{}
			proto.Unmarshal(msgdata, d)

			example.invalid("user %s:%d say:%s", user.NickName, user.UserID, d.GetMessage())

			glc.BoardcastMessage(gmsg)

		}

		glc.TimerFuncManager.NewTimer(10, f)

	}
	//写分
	if gmsg.BClassID == int32(netproto.MessageBClassID_Game) && gmsg.SClassID == int32(netproto.GameMessageClassID_TestGameScoreID) {
		msgdata := gmsg.MsgData.([]byte)

		d := &netproto.TestGameScore{}
		proto.Unmarshal(msgdata, d)

		example.invalid("user %v score event %d", user, d.GetScore())

		glc.UpdateUserScoreFunc(user.UserID, d.GetScore())

	}
	//游戏结束
	if gmsg.BClassID == int32(netproto.MessageBClassID_Game) && gmsg.SClassID == int32(netproto.GameMessageClassID_TestGameEndID) {
		example.invalid("user %v game end", user)

		glc.GameEndFunc()
	}
}
