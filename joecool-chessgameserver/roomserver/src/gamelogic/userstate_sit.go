package gamelogic

//用户坐下状态
type UserSitState struct {
}

func (ust *UserSitState) OnUserFree(user *GameUser, room *Room) bool {
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

	user.LastTableNo = tno
	user.LastSeatNo = sno
	user.GameCountOnTheTable = 0

	user.UserState = freeState

	return true
}

func (ust *UserSitState) OnUserSit(ur *GameUser, tableno int32, seatno int32, room *Room) bool {
	return false
}

func (ust *UserSitState) OnUserReady(user *GameUser, room *Room) bool {
	user.UserState = readyState
	return true
}

func (ust *UserSitState) OnUserQueue(user *GameUser, room *Room) bool {
	ust.OnUserFree(user, room)

	return user.UserState.OnUserQueue(user, room)
}

func (ust *UserSitState) OnUserGame(user *GameUser, room *Room) bool {
	return false
}

func (ust *UserSitState) OnGameEnd(user *GameUser, room *Room) bool {
	return false
}

func (ust *UserSitState) OnUserOffLine(user *GameUser, room *Room) bool {
	return false
}

func (ust *UserSitState) OnUserLeave(user *GameUser, room *Room) bool {
	ust.OnUserFree(user, room)
	return true
}

func (ust *UserSitState) StateName() string {
	return StateName_Sit
}

func (ust *UserSitState) StateValue() int32 {
	return UserState_Sit
}
