package dfdclogic

import "gamelogic"

type dfdcuserdata struct {
	user            *gamelogic.GameUser //多福多財玩家資訊
	userbetmoney    int64               //玩家下注金額
	userbetsymbol   int32               //玩家下注黃金符號倍率
	userjackpotwin  int64               //玩家jackpot贏錢
	userbeginmoney  int64               //玩家起始金額(用於歷程紀錄)
	allmoney		int64				//玩家實際下注金額(freespin=0)
}
