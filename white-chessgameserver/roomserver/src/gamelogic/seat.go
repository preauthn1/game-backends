package gamelogic

import (
	"fmt"
)

//座位
type Seat struct {
	SeatNo   int32     //座位号
	SeatUser *GameUser //用户
}

//创建座位对象
func NewSeat(seatno int32) *Seat {
	st := new(Seat)
	st.SeatNo = seatno
	st.SeatUser = nil

	return st
}

func (seat *Seat) IsEmpty() bool {
	return seat.SeatUser == nil
}

func (seat *Seat) String() string {
	return fmt.Sprintf("seatno:%d, user:%v", seat.SeatNo, seat.SeatUser)
}
