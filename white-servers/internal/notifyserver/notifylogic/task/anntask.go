package task

import (
	"time"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/db"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

//公告推送
type AnnTask struct {
	pushInterval int
	throttle     int
}

//创建公告推送对象
func NewAnnTask(pushInterval int, throttle int) *AnnTask {
	ann := new(AnnTask)
	ann.pushInterval = pushInterval
	ann.throttle = throttle

	return ann
}

func (ann *AnnTask) Run(ctx *TaskRunCtx) {
	for i := 0; i < ann.throttle; i++ {
		go func() {
			defer util.PanicErrStack()

			for {
				time.Sleep(time.Duration(ann.pushInterval) * time.Second)

				anns := ann.getAnnFromDB()
				if anns != nil && len(anns.Ann) > 0 {
					annmsg := new(msg.Message)
					annmsg.BClassID = int32(netproto.MessageBClassID_NotifyServer)
					annmsg.SClassID = int32(netproto.NotifyServerClassID_AnnID)

					annmsg.MsgData = anns

					ctx.SendHallMessage(annmsg)

					example.invalid("推送公告 %d 条", len(anns.Ann))
				} else {
					example.invalid("推送公告,但当前没有需要推送的公告信息")
				}
			}
		}()
	}
}

func (ann *AnnTask) getAnnFromDB() *netproto.AnnList {
	defer util.PanicErrStack()

	d := GetDatabase()
	ps := db.NewSqlParameters()

	proc := db.NewProcedure("PrNs_GetNotice", ps)
	dbret, err := d.ExecProc(proc)

	if err != nil {
		mlog.Error("执行存储过程PrNs_GetNotice错误, %v", err)

		return nil
	} else {
		annList := new(netproto.AnnList)
		if dbret.GetRetTableCount() > 0 {
			tbAnn := dbret[0]
			for i := 0; i < len(tbAnn.Rows); i++ {
				ann := new(netproto.AnnInfo)
				ann.ID = proto.Int32(int32(tbAnn.GetValueByColName(i, "Id").(int64)))
				ann.MsgContent = proto.String(tbAnn.GetValueByColName(i, "MsgContent").(string))
				ann.PlatformID = proto.Int32(int32(tbAnn.GetValueByColName(i, "PlatformID").(int64)))

				annList.Ann = append(annList.Ann, ann)
			}
		}

		return annList
	}
}

func (ann *AnnTask) GetTaskName() string {
	return "公告"
}
