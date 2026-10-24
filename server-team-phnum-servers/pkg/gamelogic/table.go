package gamelogic

import (
	"bytes"
	"fmt"
)

//桌子
type Table struct {
	Room      *Room
	TableNo   int32     //桌子号
	Seats     []*Seat   //座位
	GameLogic GameLogic //游戏逻辑
}

//创建桌子对象
func NewTable(seatcount int32, tno int32) *Table {
	t := new(Table)
	t.TableNo = tno

	var i int32
	for i = 0; i < seatcount; i++ {
		st := NewSeat(i)
		t.Seats = append(t.Seats, st)
	}

	return t
}

// 新增座位
func (t *Table) AddNewSeat(iSeatNo int) {
	if iSeatNo < len(t.Seats) {
		return
	}

	pSeat := NewSeat(int32(iSeatNo))
	t.Seats = append(t.Seats, pSeat)
}

//根据座位号获取座位
func (t *Table) GetSeatByNo(seatno int32) *Seat {
	for _, s := range t.Seats {
		if s.SeatNo == seatno {
			return s
		}
	}

	return nil
}

//获取桌子上的用户
func (t *Table) GetTableUser() []*GameUser {
	var urs []*GameUser
	for _, st := range t.Seats {
		if st.SeatUser != nil {
			urs = append(urs, st.SeatUser)
		}
	}

	return urs
}

//获取桌子上的真实用户
func (t *Table) GetTableRealUser() []*GameUser {
	var urs []*GameUser
	for _, st := range t.Seats {
		if st.SeatUser != nil && !st.SeatUser.IsRobot() {
			urs = append(urs, st.SeatUser)
		}
	}

	return urs
}

//获取桌子上的真实用户
func (t *Table) GetTableRealRobot() []*GameUser {
	var urs []*GameUser
	for _, st := range t.Seats {
		if st.SeatUser != nil && st.SeatUser.IsRobot() {
			urs = append(urs, st.SeatUser)
		}
	}

	return urs
}

//桌子上的用户数
func (t *Table) TableUserCount() int {
	return len(t.GetTableUser())
}

//桌子上的用户数
func (t *Table) TableRealUserCount() int {
	return len(t.GetTableRealUser())
}

//桌子上的robot用户数
func (t *Table) TableRealRobotCount() int {
	return len(t.GetTableRealRobot())
}

//获取桌子上举手的用户
func (t *Table) ReadyUser() []*GameUser {
	var urs []*GameUser
	for _, st := range t.Seats {
		if st.SeatUser != nil && st.SeatUser.IsReady() {
			urs = append(urs, st.SeatUser)
		}
	}

	return urs
}

//获取一个空闲的座位
func (t *Table) GetFreeSeat() *Seat {
	for _, st := range t.Seats {
		if st.SeatUser == nil {
			return st
		}
	}

	return nil
}

func (t *Table) GetBYFreeSeat() *Seat {
	for _, st := range t.Seats {
		if st.SeatNo <= 1 {
			continue
		}
		if st.SeatUser == nil {
			return st
		}
	}

	return nil
}

//根据用户id获取用户
func (t *Table) GetUserByID(userid int32) *GameUser {
	for _, st := range t.Seats {
		if st.SeatUser != nil && st.SeatUser.UserID == userid {
			return st.SeatUser
		}
	}

	return nil
}

func (t *Table) String() string {
	var buf bytes.Buffer
	buf.WriteString(fmt.Sprintf("tableno:%d", t.TableNo))
	for _, st := range t.Seats {
		buf.WriteString(st.String() + "\r\n")
	}

	return buf.String()
}
