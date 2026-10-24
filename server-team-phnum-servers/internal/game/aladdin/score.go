package aladdin

import "example.invalid/ServerTeam/Servers/pkg/gamelogic"

// UpdateUserScore 更新玩家分數
func (logic *Logic) UpdateUserScore(user *gamelogic.GameUser, step *StepResult) {
	if user.IsRobot() {
		return
	}

	logic.GameLog.AddGameStep("更新玩家金幣, 下注金幣: %d, 贏得獎金: %d", step.GamePayMoney, step.GameWinMoney)
	// fmt.Printf("更新玩家金幣, 下注金幣: %d, 贏得獎金: %d\n", step.GamePayMoney, step.GameWinMoney)

	logic.UpdateUserScoreFunc(user.UserID, step.GameWinMoney-step.GamePayMoney, 0, 0)

}
