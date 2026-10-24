package gamelogic

//用户站立状态
type UserFreeState struct {
}

func (ust *UserFreeState) OnUserFree(user *GameUser, room *Room) bool {
	return false
}

func (ust *UserFreeState) OnUserSit(ur *GameUser, tableno int32, seatno int32, room *Room) bool {
	//只有机器人可以由站立状态坐下
	if !ur.IsRobot() {
		return false
	}

	t := room.GetTableByNo(tableno)
	if t == nil {
		return false
	}

	st := t.GetSeatByNo(seatno)
	if st == nil {
		return false
	}

	if st.SeatUser != nil {
		return false
	}

	st.SeatUser = ur

	ur.TableNo = tableno
	ur.SeatNo = seatno

	ur.UserState = sitState

	return true
}

func (ust *UserFreeState) OnUserQueue(user *GameUser, room *Room) bool {
	user.UserState = queueState
	return true
}

func (ust *UserFreeState) OnUserReady(user *GameUser, room *Room) bool {
	return false
}

func (ust *UserFreeState) OnUserGame(user *GameUser, room *Room) bool {
	return false
}

func (ust *UserFreeState) OnGameEnd(user *GameUser, room *Room) bool {
	return false
}

func (ust *UserFreeState) OnUserLeave(user *GameUser, room *Room) bool {
	return true
}

func (ust *UserFreeState) OnUserOffLine(user *GameUser, room *Room) bool {
	return false
}

func (ust *UserFreeState) StateName() string {
	return StateName_Free
}

func (ust *UserFreeState) StateValue() int32 {
	return UserState_Free
}
