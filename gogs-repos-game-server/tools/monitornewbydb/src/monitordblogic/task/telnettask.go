package task

import (
	"fmt"
	"mlog"
	"net"
	"regexp"
	"time"
	"util"
)

type TelnetTask struct {
	pushInterval int
	throttle     int
}

func NewTelnetTask(pushInterval int, throttle int) *TelnetTask {
	ann := new(TelnetTask)
	ann.pushInterval = pushInterval
	ann.throttle = throttle

	return ann
}

func (st *TelnetTask) Run(ctx *TaskRunCtx) {
	for i := 0; i < st.throttle; i++ {
		go func() {
			time.Sleep(time.Duration(st.pushInterval) * time.Second)
			st.getTelnet()
		}()
	}
}

func (st *TelnetTask) getTelnet() {
	defer util.PanicErrStack()
	d := GetDatabase()
	re := regexp.MustCompile(`(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)){3}`)

	if re.MatchString(d.Connstring) {
		//mlog.Error("str: %v", re.Find(d.Connstring))
		for _, v := range re.FindAllString(d.Connstring, -1) {
			addr := fmt.Sprintf("%v:1433", v)
			conn, err := net.DialTimeout("tcp", addr, time.Duration(30)*time.Second)
			if err != nil {
				msg := fmt.Sprintf("telnet %v:1433 error:%v", v, err.Error())
				mlog.Error(msg)
				SendMsgToTelegram(msg)
			} else {
				defer conn.Close()
				break
			}
		}
	}
}

func (ann *TelnetTask) GetTaskName() string {
	return "更新"
}
