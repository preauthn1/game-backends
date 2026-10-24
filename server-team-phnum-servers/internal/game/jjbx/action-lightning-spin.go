package jjbx

import (
	"strings"

	"example.invalid/golang/protobuf/proto"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/slot"
)

// ActionLightningSpin 玩家表演Lightning
func (logic *Logic) ActionLightningSpin(user *gamelogic.GameUser, gmsg *msg.Message) {
	if logic.LightningGame == nil {
		mlog.Error("[玩家演完Lightning] 沒有Lightning遊戲")
		return
	}

	data := new(netproto.Slot_MSG_LightningShow)

	if err := proto.Unmarshal(gmsg.MsgData.([]byte), data); err != nil {
		mlog.Error("[玩家演完Lightning] 接收玩家封包解析錯誤. %v", err)
		return
	}

	logic.GameLog.GameBegin()
	logic.Clean()

	if logic.LightningGame.GetLastStep().LastChance == 0 {
		mlog.Error("[玩家演完Lightning] Lightning 遊戲已結束")
		return
	}

	logic.LightningGame.ShowStep = int(data.GetLightningStep())

	if logic.LightningGame.ShowStep > len(logic.LightningGame.Steps) {
		mlog.Error("[玩家演完Lightning] 超出範圍的Step")
		return
	}

	logic.LightningGame.Spin()

	prev := logic.LightningGame.Steps[logic.LightningGame.ShowStep-1]
	step := logic.LightningGame.Steps[logic.LightningGame.ShowStep]

	// 紀錄開獎符號
	logic.GameLog.AddGameStep("Lightning輪軸結果, 使用SymbolGroupID: %d, 剩餘 %d 次", step.GameResultManager.SymbolGroupID, prev.LastChance)
	mlog.Debug("Lightning輪軸結果, 使用SymbolGroupID: %d, 剩餘 %d 次", step.GameResultManager.SymbolGroupID, prev.LastChance)

	for i, row := range step.GameResultManager.GetSymbolString() {
		logic.GameLog.AddGameStep("第 %d 行: %s", i+1, strings.Join(row, ", "))
		mlog.Debug("第 %d 行: %s", i+1, strings.Join(row, ", "))
	}

	if step.Rule != nil {
		logic.GameLog.AddGameStep("Lightning出現獎勵符號, 次數增加 %s 次", step.Rule.GetPayout())
		mlog.Debug("Lightning出現獎勵符號, 次數增加 %s 次", step.Rule.GetPayout())
	}

	// 紀錄中獎結果
	if len(step.Awards) > 0 {
		logic.GameLog.AddGameStep("出現新錢幣:")
		mlog.Debug("出現新錢幣:")
		step.PrintAward(logic)
		logic.GameLog.AddGameStep("累計金額: %d", logic.LightningGame.WinMoney)
		mlog.Debug("累計金額: %d", logic.LightningGame.WinMoney)
	}

	// 檢查是否表演結束
	if step.LastChance == 0 {
		logic.GameLog.AddGameStep("Lightning遊戲結束")
		mlog.Debug("Lightning遊戲結束")

		logic.GameWinMoney += logic.LightningGame.WinMoney

		// 計算流水
		logic.CalcFlowAmount(user)

		// 寫分
		logic.UpdateUserScore(user)

		step.GameResultManager.ReelMode()
		logic.AddSlotGameHistoryLog(user, slot.HistoryTypeLIGHTNING, slot.GameTypeREEL, step.GameResultManager.ToSymbolString())
		step.GameResultManager.RowMode()
	} else {
		step.GameResultManager.ReelMode()
		logic.AddSlotGameHistoryLog(user, slot.HistoryTypeRESPIN, slot.GameTypeREEL, step.GameResultManager.ToSymbolString())
		step.GameResultManager.RowMode()
	}

	logic.GameEndLog()

	logic.SendLightningResultMessage(user)

	// 檢查是否表演結束
	if step.LastChance == 0 {

		logic.ResetLightningData(user)

		// 調整狀態為等待
		logic.UpdateState(slot.GameStateWait)

		logic.LightningGame = nil
	} else {

		logic.RecordLightningData(user)

		// 調整狀態為等待Lightning
		logic.UpdateState(slot.GameStateUserAction)

		logic.SendLightningPrepareMessage(user)
	}

	logic.GameEndFunc()
}
