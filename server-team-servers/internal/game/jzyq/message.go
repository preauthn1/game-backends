package jzyq

import (
	"sort"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
)

// 發送場景
func (logic *Logic) SendScene(u *gamelogic.GameUser) {

	// 遊戲設定
	logic.SendMessageToUser(u, logic.mSetting())
	// 本局資訊
	logic.SendMessageToUser(u, logic.mRound())
	// 歷史紀錄
	logic.SendMessageToUser(u, logic.mRecords())
	// 富豪榜
	logic.SendMessageToUser(u, logic.mRichestList())
	// 幸運星
	logic.SendMessageToUser(u, logic.mLuckyStarList())

	if logic.Game.Step == Prepare {
		return
	}

	// 遊戲階段
	logic.SendMessageToUser(u, logic.mStep())

	// 玩家下注資訊
	logic.SendMessageToUser(u, logic.mBetAreaInfo())

	// 玩家總下注金額
	logic.SendMessageToUser(u, logic.mBoardcastBetSummary(u.UserID))

	// 依照階段發送不同資訊
	switch logic.Game.Step {
	case ShowAward:
		logic.SendMessageToUser(u, logic.mGameResult())
	case Finish:
		user := logic.Game.Users[u.UserID]
		if user != nil && user.WinSum() != 0 {
			logic.SendMessageToUser(u, logic.mGameResultWin(user))
		}
	}
}

// 遊戲設定
func (logic *Logic) mSetting() *msg.Message {
	m := new(msg.Message)
	m.BClassID = int32(netproto.MessageBClassID_Game)
	m.SClassID = int32(netproto.JZYQ_GameMessageClassID_JZYQSettingID)

	setting := new(netproto.JZYQ_Setting)
	setting.BetMinCarry = proto.Int64(logic.Game.BetMinCarray)
	setting.UserMaxBet = proto.Int64(logic.Game.UserMaxBet)

	for n := range logic.Game.BetConfig.AllowAmounts {
		setting.BetCount = append(setting.BetCount, n)
	}
	sort.Slice(setting.BetCount, func(i, j int) bool { return setting.BetCount[i] < setting.BetCount[j] })

	var min int64 = 0
	if len(setting.BetCount) > 0 {
		min = setting.BetCount[0]
	}

	for t, area := range logic.Game.BetConfig.Area {
		a := new(netproto.JZYQ_BetArea)
		a.Type = TransforBetType(t).Enum()
		a.MinBet = proto.Int64(min)
		a.MaxBet = proto.Int64(area.MaxBet)
		var odd float32 = 1 + (float32(area.Odd)-1)*float32(100-logic.GetServerConfig().TaxScale)/100
		a.Odd = proto.Float64(float64(odd))

		setting.BetAreas = append(setting.BetAreas, a)
	}
	sort.Slice(setting.BetAreas, func(i, j int) bool { return setting.BetAreas[i].GetType() < setting.BetAreas[j].GetType() })

	m.MsgData = setting

	return m
}

// 遊戲階段
func (logic *Logic) mStep() *msg.Message {
	m := new(msg.Message)
	m.BClassID = int32(netproto.MessageBClassID_Game)
	m.SClassID = int32(netproto.JZYQ_GameMessageClassID_JZYQStepID)

	info := new(netproto.JZYQ_StepInfo)
	info.Step = TransforGameStep(logic.Game.Step).Enum()

	time := int32(0)
	timer := logic.GameStepTimer[logic.Game.Step]
	if timer != nil {
		time = timer.WaitSeconds - timer.ElapsedSeconds()
	}

	info.Time = proto.Int32(time)

	m.MsgData = info

	return m
}

// 本局資訊
func (logic *Logic) mRound() *msg.Message {
	m := new(msg.Message)
	m.BClassID = int32(netproto.MessageBClassID_Game)
	m.SClassID = int32(netproto.JZYQ_GameMessageClassID_JZYQRoundID)

	info := new(netproto.JZYQ_RoundInfo)
	info.RoundID = proto.Int32(int32(logic.Game.RoundID))
	info.Goal = proto.Int32(int32(logic.Game.AreaSummary[Goal] + logic.Game.AreaSummary[GoalInside] + logic.Game.AreaSummary[GoalOutside]))
	info.Lost = proto.Int32(int32(logic.Game.AreaSummary[Lost] + logic.Game.AreaSummary[LostInside] + logic.Game.AreaSummary[LostOutside]))

	m.MsgData = info

	return m
}

// 歷史紀錄
func (logic *Logic) mRecords() *msg.Message {
	m := new(msg.Message)
	m.BClassID = int32(netproto.MessageBClassID_Game)
	m.SClassID = int32(netproto.JZYQ_GameMessageClassID_JZYQRecordsID)

	data := new(netproto.JZYQ_GameRecords)

	for _, record := range logic.Game.Records {
		row := new(netproto.JZYQ_GameResult)
		row.RoundID = proto.Int32(int32(record.RoundID))
		row.Result = TransforBetType(record.Award).Enum()

		data.Results = append(data.Results, row)
	}

	m.MsgData = data

	return m
}

// 富豪榜
func (logic *Logic) mRichestList() *msg.Message {
	m := new(msg.Message)
	m.BClassID = int32(netproto.MessageBClassID_Game)
	m.SClassID = int32(netproto.JZYQ_GameMessageClassID_JZYQRichestListID)

	data := new(netproto.JZYQ_RichestList)

	for i, u := range logic.Richests {
		uInfo := TransforGameUser(u)
		uInfo.Ranking = proto.Int32(int32(i + 1))
		data.Users = append(data.Users, uInfo)
	}

	m.MsgData = data

	return m
}

// 幸運星
func (logic *Logic) mLuckyStarList() *msg.Message {
	m := new(msg.Message)
	m.BClassID = int32(netproto.MessageBClassID_Game)
	m.SClassID = int32(netproto.JZYQ_GameMessageClassID_JZYQLuckyStarListID)

	data := new(netproto.JZYQ_LuckyStarList)

	m.MsgData = data

	return m
}

// 玩家總下注金額
func (logic *Logic) mBoardcastBetSummary(uid int32) *msg.Message {
	m := new(msg.Message)
	m.BClassID = int32(netproto.MessageBClassID_Game)
	m.SClassID = int32(netproto.JZYQ_GameMessageClassID_JZYQBoardCastBetID)

	info := new(netproto.JZYQ_BoardCastBet)
	info.UserID = proto.Int32(uid)

	if u, ok := logic.Game.Users[uid]; ok {
		for t, n := range u.BetSummary {
			bet := new(netproto.JZYQ_Bet)
			bet.Type = TransforBetType(t).Enum()
			bet.Money = proto.Int64(n)
			info.Bets = append(info.Bets, bet)
		}
	}

	m.MsgData = info

	return m
}

// 玩家總下注金額
func (logic *Logic) mBoardcastBet(uid int32, bets ...*netproto.JZYQ_Bet) *msg.Message {
	m := new(msg.Message)
	m.BClassID = int32(netproto.MessageBClassID_Game)
	m.SClassID = int32(netproto.JZYQ_GameMessageClassID_JZYQBoardCastBetID)

	info := new(netproto.JZYQ_BoardCastBet)
	info.UserID = proto.Int32(uid)
	info.Bets = append(info.Bets, bets...)

	m.MsgData = info

	return m
}

// 當前單局結果
func (logic *Logic) mGameResult() *msg.Message {
	m := new(msg.Message)
	m.BClassID = int32(netproto.MessageBClassID_Game)
	m.SClassID = int32(netproto.JZYQ_GameMessageClassID_JZYQGameResultID)

	if len(logic.Game.Records) > 0 {
		latest := logic.Game.Records[len(logic.Game.Records)-1]

		data := new(netproto.JZYQ_GameResult)
		data.RoundID = proto.Int32(int32(latest.RoundID))
		data.Result = TransforBetType(latest.Award).Enum()

		m.MsgData = data
	}

	return m
}

// 玩家贏錢資訊
func (logic *Logic) mGameResultWin(u *User) *msg.Message {
	m := new(msg.Message)
	m.BClassID = int32(netproto.MessageBClassID_Game)
	m.SClassID = int32(netproto.JZYQ_GameMessageClassID_JZYQGameResultWinID)

	data := new(netproto.JZYQ_GameResultWin)
	data.Win = proto.Int64(u.WinSum() - u.BetSum())

	for _, user := range logic.Game.Users {
		info := TransforGameUser(user.GameUser)
		info.Win = proto.Int64(user.WinSum() - user.BetSum())
		if info.GetWin() > 0 {
			data.WinUserInfo = append(data.WinUserInfo, info)
		}
	}
	for _, richest := range logic.Richests {
		user := logic.Game.Users[richest.UserID]
		if user == nil {
			continue
		}
		info := TransforGameUser(user.GameUser)
		info.Win = proto.Int64(user.WinSum() - user.BetSum())
		if info.GetWin() != 0 {
			data.StageUserInfo = append(data.StageUserInfo, info)
		}
	}

	sort.Slice(data.WinUserInfo, func(i, j int) bool { return data.WinUserInfo[i].GetWin() > data.WinUserInfo[j].GetWin() })

	if len(data.WinUserInfo) > logic.WinListCount {
		data.WinUserInfo = data.WinUserInfo[:logic.WinListCount]
	}

	m.MsgData = data

	return m
}

// 總下注資訊
func (logic *Logic) mBetAreaInfo() *msg.Message {

	m := new(msg.Message)
	m.BClassID = int32(netproto.MessageBClassID_Game)
	m.SClassID = int32(netproto.JZYQ_GameMessageClassID_JZYQBetAreaInfoID)

	data := new(netproto.JZYQ_BetAreaInfo)

	for t, n := range logic.Game.BetSummary {
		bet := new(netproto.JZYQ_Bet)
		bet.Type = TransforBetType(t).Enum()
		bet.Money = proto.Int64(n)
		data.Bets = append(data.Bets, bet)
	}

	for _, u := range logic.Game.Users {
		uBet := new(netproto.JZYQ_BoardCastBet)
		uBet.UserID = proto.Int32(u.UserID)

		for t, n := range u.BetSummary {
			bet := new(netproto.JZYQ_Bet)
			bet.Type = TransforBetType(t).Enum()
			bet.Money = proto.Int64(n)
			uBet.Bets = append(uBet.Bets, bet)
		}

		data.UserBets = append(data.UserBets, uBet)
	}

	m.MsgData = data

	return m
}
