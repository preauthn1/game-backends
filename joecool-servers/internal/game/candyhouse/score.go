package candyhouse

import "example.invalid/ServerTeam/Servers/pkg/gamelogic"

// UpdateUserScore 更新玩家分數
func (logic *Logic) UpdateUserScore(user *gamelogic.GameUser) {
	if user.IsRobot() {
		return
	}

	logic.GameLog.AddGameStep("更新玩家金幣, 下注金幣: %d, 贏得獎金: %d", logic.GamePayMoney, logic.GameWinMoney)

	logic.UpdateUserScoreFunc(user.UserID, logic.GameWinMoney-logic.GamePayMoney, 0)

}
