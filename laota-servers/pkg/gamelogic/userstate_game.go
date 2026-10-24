package gamelogic

//用户游戏状态
type UserGameState struct {
}

func (ust *UserGameState) OnUserFree(user *GameUser, room *Room) bool {
	return false
}

func (ust *UserGameState) OnUserSit(ur *GameUser, tableno int32, seatno int32, room *Room) bool {
	return false
}

func (ust *UserGameState) OnUserQueue(user *GameUser, room *Room) bool {
	return false
}

func (ust *UserGameState) OnUserReady(user *GameUser, room *Room) bool {
	return false
}

func (ust *UserGameState) OnUserGame(user *GameUser, room *Room) bool {
	return false
}

func (ust *UserGameState) OnGameEnd(user *GameUser, room *Room) bool {
	user.UserState = sitState
	return true
}

func (ust *UserGameState) OnUserLeave(user *GameUser, room *Room) bool {
	tno := user.TableNo
	sno := user.SeatNo

	t := room.GetTableByNo(tno)
	if t == nil {
		return false
	}

	st := t.GetSeatByNo(sno)
	if st == nil {
		return false
	}

	st.SeatUser = nil
	user.TableNo = FreeTableNo
	user.SeatNo = FreeSeatNo
	user.UserState = freeState

	return true
}

func (ust *UserGameState) OnUserOffLine(user *GameUser, room *Room) bool {
	user.UserState = offlineState
	return true
}

func (ust *UserGameState) StateName() string {
	return StateName_Game
}

func (ust *UserGameState) StateValue() int32 {
	return UserState_Game
}
