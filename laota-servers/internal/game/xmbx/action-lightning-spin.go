package xmbx

import (
	"example.invalid/golang/protobuf/proto"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/slot"
)

// ActionLightningSpin 玩家表演Lightning
func (logic *Logic) ActionLightningSpin(user *gamelogic.GameUser, gmsg *msg.Message) {
	logic.GameLog.GameBegin()
	logic.Clean()

	data := &netproto.XMBX_MSG_LightningShow{}
	if err := proto.Unmarshal(gmsg.MsgData.([]byte), data); err != nil {
		mlog.Error("[玩家演完Lightning] 接收玩家封包解析錯誤. %v", err)
		return
	}

	if logic.LightningGame == nil {
		mlog.Error("[玩家演完Lightning] 此遊戲目前無Lightning")
		return
	}

	logic.LightningGame.ShowStep = int(data.GetLightningStep())

	if logic.LightningGame.ShowStep >= len(logic.LightningGame.Steps) {
		mlog.Error("[玩家演完Lightning] 超出範圍的Step")
		return
	}

	prev := logic.LightningGame.Steps[logic.LightningGame.ShowStep-1]
	step := logic.LightningGame.Steps[logic.LightningGame.ShowStep]

	// 紀錄開獎符號
	step.GameResultManager.RowMode()
	logic.GameLog.AddGameStep("Lightning(%d), 剩餘 %d 次, %s", step.GameResultManager.SymbolGroupID, prev.LastChance, step.GameResultManager.ToSymbolString())

	if step.Rule != nil && step.Rule != prev.Rule {
		logic.GameLog.AddGameStep("Lightngin出現新的金球, 重置次數為 %d 次", step.Origin.GetRespinCount())
	}

	// 檢查是否贏得最大Jackpot
	// var isWinJackpot bool
	// var jpMoney int64
	for _, award := range step.Awards {

		if jackpot, ok := logic.Jackpots[award.JackpotType]; ok {
			if jackpot.Origin.GetIsLinked() == 1 {
				logic.CalcJackpot(jackpot)
			}

			if award.Symbol == "" {
				logic.GameLog.AddGameStep("贏得最大獎 Jackpot, 停止重轉")
			}
		}
	}

	// 檢查是否表演結束
	if logic.LightningGame.ShowStep == len(logic.LightningGame.Steps)-1 {

		logic.GameLog.AddGameStep("Lightning 遊戲結束，開始計算獎勵:")
		// 紀錄中獎結果
		for _, award := range step.Awards {
			switch award.Type {
			case slot.PayTypeChipXRate:
				logic.GameLog.AddGameStep("%d_%d:%s(%s)=%d", award.Position.Reel, award.Position.Row, award.Symbol, award.Type, award.WinMoney)
			case slot.PayTypeJackpot:
				logic.GameLog.AddGameStep("%d_%d:%s(Jackpot[%s])=%d", award.Position.Reel, award.Position.Row, award.Symbol, award.JackpotType.String(), award.WinMoney)
			}
		}

		logic.GameWinMoney = logic.LightningGame.GetLastStep().WinMoney
		logic.GamePayMoney = 0

		// 是否在免費遊戲中
		if logic.IsFreeSpin() {
			logic.FreeSpin.WinMoney += logic.LightningGame.GetLastStep().WinMoney
		}

		step.GameResultManager.ReelMode()
		logic.AddSlotGameHistoryLog(user, slot.HistoryTypeLIGHTNING, slot.LogTypeREEL, step.GameResultManager.ToSymbolString())
		step.GameResultManager.RowMode()

		// 計算流水
		logic.CalcFlowAmount(user)

		logic.UpdateUserScore(user)
	} else {
		step.GameResultManager.ReelMode()
		logic.AddSlotGameHistoryLog(user, slot.HistoryTypeRESPIN, slot.LogTypeREEL, step.GameResultManager.ToSymbolString())
		step.GameResultManager.RowMode()
	}

	logic.GameEndLog()

	logic.SendLightningResultMessage(user)

	logic.RecordLightningData(user)

	logic.UpdateState(slot.GameStateWait)

	logic.GameEndFunc()
}
