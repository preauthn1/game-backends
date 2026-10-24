package gamelogic

type UserQueueState struct {
}

func (ust *UserQueueState) OnUserFree(user *GameUser, room *Room) bool {
	return false
}

func (ust *UserQueueState) OnUserSit(ur *GameUser, tableno int32, seatno int32, room *Room) bool {
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

func (ust *UserQueueState) OnUserQueue(user *GameUser, room *Room) bool {
	return false
}

func (ust *UserQueueState) OnUserReady(user *GameUser, room *Room) bool {
	return false
}

func (ust *UserQueueState) OnUserGame(user *GameUser, room *Room) bool {
	return false
}

func (ust *UserQueueState) OnGameEnd(user *GameUser, room *Room) bool {
	return false
}

func (ust *UserQueueState) OnUserLeave(user *GameUser, room *Room) bool {
	user.UserState = freeState
	return true
}

func (ust *UserQueueState) OnUserOffLine(user *GameUser, room *Room) bool {
	return false
}

func (ust *UserQueueState) StateName() string {
	return StateName_Queue
}

func (ust *UserQueueState) StateValue() int32 {
	return UserState_Queue
}
