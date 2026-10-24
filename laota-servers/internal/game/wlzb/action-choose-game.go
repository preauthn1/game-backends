package wlzb

import (
	"example.invalid/golang/protobuf/proto"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/slot"
)

// ActionChooseGame 玩家選擇獎勵模式
func (logic *Logic) ActionChooseGame(user *gamelogic.GameUser, gmsg *msg.Message) {
	if len(logic.SelectChances) == 0 {
		mlog.Error("[玩家選擇遊戲] 無選擇機會")
		return
	}

	if logic.FreeSpin != nil {
		mlog.Error("[玩家選擇遊戲] 還有免費遊戲沒有結束")
		return
	}

	data := &netproto.WLZB_MSG_ChooseGame{}
	if err := proto.Unmarshal(gmsg.MsgData.([]byte), data); err != nil {
		mlog.Error("[玩家選擇遊戲] 接收玩家封包解析錯誤. %v", err)
		return
	}

	sc := logic.SelectChances[0]

	opt := sc.ChooseByID(data.GetID())
	if opt == nil {
		mlog.Error("[玩家選擇遊戲] 查無選項 %d", data.GetID())
		return
	}

	logic.FreeSpin = slot.NewFreeSpin(opt, logic.SlotSymbolDistribution, sc.BetMoney, sc.BetRate)
	logic.UpdateFreeSpin(user)

	logic.SendLeftFreeGameToUser(user)

	logic.SelectChances = logic.SelectChances[1:]
	logic.RecordSelectGameTimes(user)

	logic.GameLog.AddGameStep("玩家選擇免費遊戲, 剩餘選擇機會 %d 次。免費遊戲(%d) - 次數: %d, 下注金額: %d, 下注倍率: %d", len(logic.SelectChances), opt.GetId(), logic.FreeSpin.Count, logic.FreeSpin.BetMoney, logic.FreeSpin.BetRate)

	logic.SendSelectChanceMessage(user)

	logic.GameEndLog()

	logic.GameEndFunc()
}
