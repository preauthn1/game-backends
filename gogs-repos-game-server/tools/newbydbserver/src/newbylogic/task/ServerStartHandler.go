package task

import (
	"db"
	"fmt"
	"example.invalid/golang/protobuf/proto"
	"mlog"
	"module"
	"msg"
	"netproto"
	"network"
	"util"
)

type NewBYServerStartHandler struct {
}

//捕鱼启动程序
func NewNewBYServerStartHandler() *NewBYServerStartHandler {
	handler := new(NewBYServerStartHandler)
	return handler
}

func (h *NewBYServerStartHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_NewByDBServer) && sclsid == int32(netproto.NewBYDBServerClassID_ServerStartID)
}

func (h *NewBYServerStartHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	defer util.PanicErrStack()
	mmbyte := m.MsgData.([]byte)

	mm := &netproto.NewBYServerStartMsg{}
	err := proto.Unmarshal(mmbyte, mm)

	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_NewByDBServer), int32(netproto.NewBYDBServerClassID_ServerStartRetID), nil)
	retdata := new(netproto.NewByDBRegMsg)

	if err != nil {
		ctx.Error("解析UpdateUserMoney错误,%v", err)

		retdata.Code = proto.Int32(-1)
		retdata.ChvMsg = proto.String("解析UpdateUserMoney错误")
		retmsg.MsgData = retdata
		return []*msg.Message{retmsg}
	}

	d := GetDatabase()
	ps := db.NewSqlParameters()
	ps.AddIntInput("sintGameID", mm.GetGameID())
	ps.AddVarcharInput("chvServerName", mm.GetServerName(), 64)
	ps.AddVarcharInput("chvShowName", mm.GetShowName(), 64)
	ps.AddVarcharInput("chvClientAddr", mm.GetClientAddr(), 32)
	ps.AddVarcharInput("chvNotifyAddr", mm.GetNotifyAddr(), 32)
	ps.AddVarcharInput("chvHttpAddr", mm.GetHttpAddr(), 32)
	ps.AddIntInput("lngLoginMoney", mm.GetLoginMoney())
	ps.AddIntInput("sintServerID", mm.GetServerID())
	ps.AddVarcharOutput("chvErrMsg", "")

	proc := db.NewProcedure("PrGs_ServerStart", ps)
	dbret, err := d.ExecProc(proc)

	defer func() {
		if err := recover(); err != nil {
			errorStr := util.GetStackInfo()
			mlog.NewDefaultLogger().Error("发生错误%s, 執行存儲過程%s", errorStr, proc.GetSqlString())
			panic(err)
		}
	}()

	var code int32
	code = 1
	message := "服务器启动成功"

	if err != nil {
		message = fmt.Sprintf("执行存储过程PrGs_ServerStart错误, %v", err)
		ctx.Error(message)
		code = -1
	} else if dbret.GetReturnValue() == 0 {
		message = dbret.GetOutputParamValue("chvErrMsg").(string)
		code = 0
	} else {
		code = int32(dbret.GetReturnValue())
		message = dbret.GetOutputParamValueString("chvErrMsg")
	}

	retdata.Code = proto.Int32(code)
	retdata.ChvMsg = proto.String(message)
	retmsg.MsgData = retdata

	return []*msg.Message{retmsg}
}
