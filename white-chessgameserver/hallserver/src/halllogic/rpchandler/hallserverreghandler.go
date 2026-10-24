package rpchandler

import (
	"module"
	"msg"
	"netproto"
	"rpcmsg"
	"fmt"
	proto "example.invalid/golang/protobuf/proto"
	"mlog"
)

//大厅启动
type HallServerRegHandler struct {
	clientAddr  string
	notifyAddr  string
	httpAddr    string
	serverid    int32
	rpcmsgqueue *rpcmsg.RPCMsgQueue

	hallServerRegRet *netproto.HallServerStartRet
}

//创建大厅服务器注册处理器对象
func NewHallServerRegHandler(clientaddr string, notifyaddr string, httpaddr string, serverid int32, dbmsgqueue *rpcmsg.RPCMsgQueue) *HallServerRegHandler {
	hreg := new(HallServerRegHandler)
	hreg.clientAddr = clientaddr
	hreg.notifyAddr = notifyaddr
	hreg.httpAddr = httpaddr
	hreg.serverid = serverid
	hreg.rpcmsgqueue = dbmsgqueue
	hreg.hallServerRegRet = nil

	return hreg
}

func (hsreg *HallServerRegHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_DBServer) && sclsid == int32(netproto.DBServerClassID_HallServerStartRetID)
}

func (hsreg *HallServerRegHandler) OnRPCMessage(m *msg.Message, ctx *module.MouduleCtx) {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.HallServerStartRet{}
		return mm
	})

	mm := rmm.(*netproto.HallServerStartRet)

	hsreg.hallServerRegRet = mm

	example.invalid("收到dbserver返回注册信息.code:%d,msg:%s,android ver:%s,ios ver:%s,", mm.GetCode(), mm.GetMessage(), mm.GetAndroidVersion(), mm.GetIOSVersion())

	if mm.GetCode() == 0 {
		mlog.NewDefaultLogger().SetData(fmt.Sprintf("[HallServerRegHandler] 向DBServer注册失败.%s 請儘速洽詢資訊人員", mm.GetMessage())).Error("HallServer壓測時")
		ctx.Error("向DBServer注册失败.%s", mm.GetMessage())
	} else {
		example.invalid("向DBServer注册成功")
	}
}

//通知服务器启动
func (hsreg *HallServerRegHandler) SendDBServerStartMsg() {
	svrmsg := new(msg.Message)
	svrmsg.BClassID = int32(netproto.MessageBClassID_DBServer)
	svrmsg.SClassID = int32(netproto.DBServerClassID_HallServerStartID)

	svrdata := new(netproto.HallServerStart)
	svrdata.ClientAddr = proto.String(hsreg.clientAddr)
	svrdata.NotifyAddr = proto.String(hsreg.notifyAddr)
	svrdata.ServerID = proto.Int32(hsreg.serverid)
	svrdata.HttpAddr = proto.String(hsreg.httpAddr)

	svrmsg.MsgData = svrdata

	hsreg.rpcmsgqueue.PushMessage(netproto.CreateEmptyRPCInfo(hsreg.serverid), false, svrmsg)

}

type RPCHallVerProvider struct {
	hallRegHandler *HallServerRegHandler
}

//创建平台版本提供器对象
func NewRPCHallVerProvider(hallRegHandler *HallServerRegHandler) *RPCHallVerProvider {
	rpch := new(RPCHallVerProvider)
	rpch.hallRegHandler = hallRegHandler

	return rpch
}

func (p *RPCHallVerProvider) GetVer(areaId int64) *netproto.HallVerInfo {
	v := new(netproto.HallVerInfo)

	regret := p.hallRegHandler.hallServerRegRet
	if regret.GetCode() == 1 {
		v.AndroidVersion = regret.AndroidVersion
		v.IOSVersion = regret.IOSVersion
		v.UpdateDomain = regret.UpdateDomain
		v.BillDomain = regret.BillDomain
		v.ClubDomain = regret.ClubDomain
		v.CustomerDomain = regret.CustomerDomain
	}
	for _, row := range regret.AreaConfigList {
		if row.GetAreaID() == areaId {
			v.AreaConfig = row.AreaConfig
		}
	}
	return v
}

type RPCHallVerLoader struct {
	hallRegHandler *HallServerRegHandler
}

//创建平台版本加载器对象
func NewRPCHallVerLoader(hallRegHandler *HallServerRegHandler) *RPCHallVerLoader {
	verLoader := new(RPCHallVerLoader)
	verLoader.hallRegHandler = hallRegHandler

	return verLoader
}

func (ver *RPCHallVerLoader) LoadHallVer() {
	ver.hallRegHandler.SendDBServerStartMsg()
}
