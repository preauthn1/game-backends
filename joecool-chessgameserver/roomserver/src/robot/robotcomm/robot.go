package robotcomm

import (
	"fmt"
	"gamelogic"
)

//机器人
type Robot struct {
	UserInfo      *gamelogic.GameUser //用户信息
	RebotLevel    int32               //机器人等级
	LeaveState    int32               //1 = 離開
	BuchuBombCard bool                //是否不出狀態
}

func (r *Robot) String() string {
	return fmt.Sprintf("%v,level:%d,LeaveState:%d", r.UserInfo.String(), r.RebotLevel, r.LeaveState)
}

//是否被凍結住
func (r *Robot) IsLeave() bool {
	if r.LeaveState == 1 {
		return true
	}
	return false
}

func (r *Robot) SetLeave() {
	r.LeaveState = 1
}
