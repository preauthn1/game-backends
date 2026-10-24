package task

import (
	"db"
	"fmt"
	"mlog"
	"time"
	"util"
)

type UpdateMoneyTask struct {
	pushInterval int
	throttle     int
}

func NewUpdateMoneyTask(pushInterval int, throttle int) *UpdateMoneyTask {
	ann := new(UpdateMoneyTask)
	ann.pushInterval = pushInterval
	ann.throttle = throttle

	return ann
}

func (st *UpdateMoneyTask) Run(ctx *TaskRunCtx) {
	for i := 0; i < st.throttle; i++ {
		go func() {
			defer util.PanicErrStack()

			for {
				time.Sleep(time.Duration(st.pushInterval) * time.Second)
				st.getFromDB()
			}
		}()
	}
}

func (ann *UpdateMoneyTask) GetTaskName() string {
	return "更新"
}

func (st *UpdateMoneyTask) getFromDB() {
	defer util.PanicErrStack()

	d := GetDatabase()
	ps := db.NewSqlParameters()
	ps.AddIntInput("intMsgID", 0)
	ps.AddIntInput("intUserID", 0)
	ps.AddIntInput("lngUpdateAmount", 0)
	ps.AddIntInput("lngTaxAmount", 0)
	ps.AddIntInput("intServerID", 0)
	ps.AddIntInput("lngCouponAmount", 0)

	proc := db.NewProcedure("PrGs_UpdateUserMoney", ps)
	_, err := d.ExecProc(proc)

	if err != nil {
		msg := fmt.Sprintf("[更新]执行存储过程错误, %v", err)
		mlog.Error(msg)
		SendMsgToTelegram(msg)
	} else {
		mlog.Debug("[weihao] 更新成功")
	}
}