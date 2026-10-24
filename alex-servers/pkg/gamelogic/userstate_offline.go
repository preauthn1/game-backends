package gamelogic

//用户游戏状态
type UserOfflineState struct {
}

func (ust *UserOfflineState) OnUserFree(user *GameUser, room *Room) bool {
	return false
}

func (ust *UserOfflineState) OnUserSit(ur *GameUser, tableno int32, seatno int32, room *Room) bool {
	return false
}

func (ust *UserOfflineState) OnUserQueue(user *GameUser, room *Room) bool {
	return false
}

func (ust *UserOfflineState) OnUserReady(user *GameUser, room *Room) bool {
	return false
}

func (ust *UserOfflineState) OnUserGame(user *GameUser, room *Room) bool {
	user.UserState = gameState
	return false
}

func (ust *UserOfflineState) OnGameEnd(user *GameUser, room *Room) bool {
	return true
}

func (ust *UserOfflineState) OnUserOffLine(user *GameUser, room *Room) bool {
	return false
}

func (ust *UserOfflineState) OnUserLeave(user *GameUser, room *Room) bool {
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
	user.UserState = freeState
	user.TableNo = FreeTableNo
	user.SeatNo = FreeSeatNo

	return true
}

func (ust *UserOfflineState) StateName() string {
	return StateName_Offline
}

func (ust *UserOfflineState) StateValue() int32 {
	return UserState_Offline
}
