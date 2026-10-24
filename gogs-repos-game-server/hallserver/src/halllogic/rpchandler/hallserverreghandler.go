package rpchandler

import (
	"fmt"
	"gameserver/serverhandler"
	proto "example.invalid/golang/protobuf/proto"
	"mlog"
	"module"
	"msg"
	"netproto"
	"rpcmsg"
	"util"
	"player"
	"network"
)

//大厅启动
type HallServerRegHandler struct {
	clientAddr       string
	notifyAddr       string
	httpAddr         string
	serverid         int32
	rpcmsgqueue      *rpcmsg.RPCMsgQueue
	hallServerRegRet *netproto.HallServerStartRet
	AreaConfigMap    map[int64]*netproto.AreaConfig
	NotifyHandler    *serverhandler.NotifyHandler
	ChanAreaConfigStatus chan bool
}

//创建大厅服务器注册处理器对象
func NewHallServerRegHandler(clientaddr string, notifyaddr string, httpaddr string, serverid int32, dbmsgqueue *rpcmsg.RPCMsgQueue, nshandler *serverhandler.NotifyHandler) *HallServerRegHandler {
	hreg := new(HallServerRegHandler)
	hreg.clientAddr = clientaddr
	hreg.notifyAddr = notifyaddr
	hreg.httpAddr = httpaddr
	hreg.serverid = serverid
	hreg.rpcmsgqueue = dbmsgqueue
	hreg.hallServerRegRet = new(netproto.HallServerStartRet)
	hreg.AreaConfigMap = make(map[int64]*netproto.AreaConfig, 0)
	hreg.ChanAreaConfigStatus = make(chan bool, 1)
	hreg.NotifyHandler = nshandler

	return hreg
}

func (hsreg *HallServerRegHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_DBServer) &&
		(sclsid == int32(netproto.DBServerClassID_HallServerStartRetID) ||
			sclsid == int32(netproto.DBServerClassID_GetAreaConfigRetID) ||
			sclsid == int32(netproto.DBServerClassID_GetAreaServerRelationsRetID))
}

func (hsreg *HallServerRegHandler) OnRPCMessage(m *msg.Message, ctx *module.MouduleCtx) {

	if m.SClassID == int32(netproto.DBServerClassID_HallServerStartRetID) {
		_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
			mm := &netproto.HallServerStartRet{}
			return mm
		})

		mm := rmm.(*netproto.HallServerStartRet)

		//hsreg.hallServerRegRet = mm
		hsreg.hallServerRegRet.Code = mm.Code
		hsreg.hallServerRegRet.Message = mm.Message
		hsreg.hallServerRegRet.AndroidVersion = mm.AndroidVersion
		hsreg.hallServerRegRet.AndroidVersion2 = mm.AndroidVersion2
		hsreg.hallServerRegRet.IOSVersion = mm.IOSVersion
		hsreg.hallServerRegRet.IOSVersion2 = mm.IOSVersion2
		hsreg.hallServerRegRet.UpdateDomain = mm.UpdateDomain
		hsreg.hallServerRegRet.BillDomain = mm.BillDomain
		hsreg.hallServerRegRet.ClubDomain = mm.ClubDomain
		hsreg.hallServerRegRet.CustomerDomain = mm.CustomerDomain
		hsreg.hallServerRegRet.AreaConfigList = mm.AreaConfigList

		example.invalid("收到dbserver返回注册信息.code:%d,msg:%s,android ver:%s,ios ver:%s,", mm.GetCode(), mm.GetMessage(), mm.GetAndroidVersion(), mm.GetIOSVersion())

		if mm.GetCode() == 0 {
			mlog.NewDefaultLogger().SetData(fmt.Sprintf("[HallServerRegHandler] 向DBServer注册失败.%s 請儘速洽詢資訊人員", mm.GetMessage())).Error("HallServer壓測時")
			ctx.Error("向DBServer注册失败.%s", mm.GetMessage())
		} else {
			example.invalid("向DBServer注册成功")
		}
	}

	if m.SClassID == int32(netproto.DBServerClassID_GetAreaConfigRetID) {
		_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
			mm := &netproto.AreaConfig{}
			return mm
		})

		mm := rmm.(*netproto.AreaConfig)

		hsreg.AreaConfigMap[mm.GetAreaID()] = mm
		
		if !mm.GetNotifyRefresh() {
			hsreg.ChanAreaConfigStatus <- true
		}
	}

	if m.SClassID == int32(netproto.DBServerClassID_GetAreaServerRelationsRetID) {
		_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
			mm := &netproto.GetAreaShowGameFlagListMsg{}
			return mm
		})

		mm := rmm.(*netproto.GetAreaShowGameFlagListMsg)
		hsreg.hallServerRegRet.AreaShowGameFlagList = make([]*netproto.AreaShowGameFlag, 0)
		hsreg.hallServerRegRet.AreaShowGameFlagList = append(hsreg.hallServerRegRet.AreaShowGameFlagList , mm.AreaShowGameFlagList... )
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

//取得遊戲渠道
func (hsreg *HallServerRegHandler) SendDBGetServerRelationsMsg() {
	svrmsg := new(msg.Message)
	svrmsg.BClassID = int32(netproto.MessageBClassID_DBServer)
	svrmsg.SClassID = int32(netproto.DBServerClassID_GetAreaServerRelationsID)
	hsreg.rpcmsgqueue.PushMessage(netproto.CreateEmptyRPCInfo(hsreg.serverid), false, svrmsg)
}

//刷新AreaConfig暫存資料
func (hsreg *HallServerRegHandler) SendAreaConfigRefreshMsg() {
	for _, v := range hsreg.AreaConfigMap {
		svrmsg := new(msg.Message)
		svrmsg.BClassID = int32(netproto.MessageBClassID_DBServer)
		svrmsg.SClassID = int32(netproto.DBServerClassID_GetAreaConfigID)
		svrdata := new(netproto.GetHallAreaMsg)
		svrdata.AreaID = proto.Int64(v.GetAreaID())
		svrdata.NotifyRefresh = proto.Bool(true)
		svrmsg.MsgData = svrdata
		hsreg.rpcmsgqueue.PushMessage(netproto.CreateEmptyRPCInfo(hsreg.serverid), false, svrmsg)
	}
}

type RPCHallVerProvider struct {
	hallRegHandler *HallServerRegHandler
	PlayerManager *player.PlayerManager
	evtChans       chan func()
}

//创建平台版本提供器对象
func NewRPCHallVerProvider(hallRegHandler *HallServerRegHandler, playerManager *player.PlayerManager) *RPCHallVerProvider {
	rpch := new(RPCHallVerProvider)
	rpch.hallRegHandler = hallRegHandler
	rpch.PlayerManager = playerManager

	rpch.evtChans = make(chan func(), 10000)
	for i := 0; i < 10; i++ {
		go func() {
			for {
				callfunc := <-rpch.evtChans
				if callfunc != nil {
					util.SafeCall(callfunc)
				}
			}
		}()
	}

	return rpch
}

func (p *RPCHallVerProvider) GetShowGameFlagByArea(areaId int64) []*netproto.AreaShowGameFlag {
	resultList := make([]*netproto.AreaShowGameFlag, 0)
	for _, v := range p.hallRegHandler.hallServerRegRet.AreaShowGameFlagList {
		if v.GetAreaID() == int32(areaId) {
			resultList = append(resultList, v)
		}
	}
	return resultList
}

func (p *RPCHallVerProvider) GetVer(ctx *module.MouduleCtx, clt *network.ConnInfo, areaId int64) *netproto.HallVerInfo {
	v := new(netproto.HallVerInfo)

	regret := p.hallRegHandler.hallServerRegRet
	if regret.GetCode() == 1 {
		v.AndroidVersion = regret.AndroidVersion
		v.IOSVersion = regret.IOSVersion
		v.AndroidVersion2 = regret.AndroidVersion2
		v.IOSVersion2 = regret.IOSVersion2
		v.UpdateDomain = regret.UpdateDomain
		v.BillDomain = regret.BillDomain
		v.ClubDomain = regret.ClubDomain
		v.CustomerDomain = regret.CustomerDomain
	}

	ok, areaConfigList := p.GetAreaConfig(areaId)

	if ok {
		v.AreaConfig = areaConfigList.AreaConfig
		return v
	} else {
		p.SendDBAreaIDMsg(areaId)
	}

	if !ok {
		if pr := p.PlayerManager.GetPlayerByConnID(clt.ConnID());pr != nil || !pr.IsGuest(){
			evtfunc := func() {
				select {
					case <- p.hallRegHandler.ChanAreaConfigStatus:
						ok, areaConfigList = p.GetAreaConfig(areaId)
						if ok {
							v.AreaConfig = areaConfigList.AreaConfig
						}

						vermsg := new(msg.Message)
						vermsg.BClassID = int32(netproto.MessageBClassID_Hall)
						vermsg.SClassID = int32(netproto.HallMsgClassID_HallVerInfoID)
						vermsg.MsgData = v

						ctx.Server.SendMessage(clt, vermsg)
				}
			}
			p.evtChans <- evtfunc
		}
	}

	return nil
}

//大廳取得DB的AredID, AreaConfig
func (hsreg *RPCHallVerProvider) SendDBAreaIDMsg(areaId int64) {
	svrmsg := new(msg.Message)
	svrmsg.BClassID = int32(netproto.MessageBClassID_DBServer)
	svrmsg.SClassID = int32(netproto.DBServerClassID_GetAreaConfigID)

	svrdata := new(netproto.GetHallAreaMsg)
	svrdata.AreaID = proto.Int64(areaId)
	svrdata.NotifyRefresh = proto.Bool(false)

	svrmsg.MsgData = svrdata
	hsreg.hallRegHandler.rpcmsgqueue.PushMessage(netproto.CreateEmptyRPCInfo(hsreg.hallRegHandler.serverid), false, svrmsg)
}

//取得AreaConfig
func (p *RPCHallVerProvider) GetAreaConfig(areaId int64) (bool, *netproto.AreaConfig) {
	areaConfigList := new(netproto.AreaConfig)

	if val, ok := p.hallRegHandler.AreaConfigMap[areaId]; ok {
		return true, val
	}

	return false, areaConfigList
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
	ver.hallRegHandler.SendAreaConfigRefreshMsg()
	ver.hallRegHandler.SendDBGetServerRelationsMsg()
}
