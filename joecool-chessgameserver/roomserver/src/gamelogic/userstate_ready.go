package gamelogic

//用户准备状态
type UserReadyState struct {
}

func (ust *UserReadyState) OnUserFree(user *GameUser, room *Room) bool {
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
	user.GameCountOnTheTable = 0

	return true
}

func (ust *UserReadyState) OnUserSit(ur *GameUser, tableno int32, seatno int32, room *Room) bool {
	return false
}

func (ust *UserReadyState) OnUserQueue(user *GameUser, room *Room) bool {
	ust.OnUserFree(user, room)
	return user.UserState.OnUserQueue(user, room)
}

func (ust *UserReadyState) OnUserReady(user *GameUser, room *Room) bool {
	return false
}

func (ust *UserReadyState) OnUserGame(user *GameUser, room *Room) bool {
	user.UserState = gameState
	//设置用户在同一桌游戏的局数
	user.GameCountOnTheTable++

	//设置用户最后一局桌号
	user.LastTableNo = user.TableNo
	user.LastSeatNo = user.SeatNo

	return true
}

func (ust *UserReadyState) OnGameEnd(user *GameUser, room *Room) bool {
	return false
}

func (ust *UserReadyState) OnUserOffLine(user *GameUser, room *Room) bool {
	return false
}

func (ust *UserReadyState) OnUserLeave(user *GameUser, room *Room) bool {
	ust.OnUserFree(user, room)
	return true
}

func (ust *UserReadyState) StateName() string {
	return StateName_Ready
}

func (ust *UserReadyState) StateValue() int32 {
	return UserState_Ready
}
