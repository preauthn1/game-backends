package task

import (
	"example.invalid/ServerTeam/Servers/pkg/db"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

import (
	"example.invalid/golang/protobuf/proto"
)

//服务器注册处理器
type UpdateUserNotifyIdxHandler struct {

}

//创建服务器注册处理器
func NewUpdateUserNotifyHandler() *UpdateUserNotifyIdxHandler {
	handler := new(UpdateUserNotifyIdxHandler)
	return handler
}

func (h *UpdateUserNotifyIdxHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_NotifyServer) && sclsid == int32(netproto.NotifyServerClassID_UpdateUserNotifyIdxH2NID)
}

func (h *UpdateUserNotifyIdxHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	defer util.PanicErrStack()
	mmbyte := m.MsgData.([]byte)

	mm := &netproto.UpdateUserNotifyIdx{}
	err := proto.Unmarshal(mmbyte, mm)

	if err != nil {
		ctx.Error("解析notifyserver收到大廳更新線上用戶错误,%v", err)
		ctx.Server.CloseConn(clt)
		return nil
	}

	d := GetDatabase()
	ps := db.NewSqlParameters()
	ps.AddIntInput("intUserID", mm.GetUserID())
	ps.AddBigIntInput("intIdx", mm.GetIdx())
	proc := db.NewProcedure("PrNs_UpdateNotifyIdx", ps)
	dbret, err := d.ExecProc(proc)
	if err != nil {
		mlog.Error("执行存储过程PrNs_UpdateNotifyIdx错误, %v", err)
	}
	if dbret.GetReturnValue() != 1 {
		mlog.Error("执行存储过程PrNs_UpdateNotifyIdx错误, %d", dbret.GetReturnValue())
	}
	return nil
}
