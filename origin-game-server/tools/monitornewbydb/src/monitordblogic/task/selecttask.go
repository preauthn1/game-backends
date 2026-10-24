package task

import (
	"db"
	"fmt"
	"mlog"
	"time"
	"util"
)

//查询
type SelectTask struct {
	pushInterval int
	throttle     int
}

//创建对象
func NewSelectTask(pushInterval int, throttle int) *SelectTask {
	ann := new(SelectTask)
	ann.pushInterval = pushInterval
	ann.throttle = throttle

	return ann
}

func (st *SelectTask) Run(ctx *TaskRunCtx) {
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

func (ann *SelectTask) GetTaskName() string {
	return "查询"
}

func (st *SelectTask) getFromDB() bool {
	defer util.PanicErrStack()

	d := GetDatabase()
	ps := db.NewSqlParameters()
	ps.AddIntInput("intUserID", 0)
	proc := db.NewProcedure("PrPs_GetShanFuKey", ps)
	_, err := d.ExecProc(proc)

	if err != nil {
		msg := fmt.Sprintf("[查询]执行存储过程错误, %v", err)
		mlog.Error(msg)
		SendMsgToTelegram(msg)
		return false
	} else {
		mlog.Debug("[weihao] 查询成功")
		return true
	}
}
