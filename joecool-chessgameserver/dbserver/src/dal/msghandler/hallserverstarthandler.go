package msghandler

import (
	"fmt"

	proto "example.invalid/golang/protobuf/proto"
)

import (
	"db"
	"mlog"
	"module"
	"msg"
	"netproto"
	"network"
	"rpcmsg"
)

//大厅启动
type HallServerStartHandler struct {
}

func (rh *HallServerStartHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_DBServer) && sclsid == int32(netproto.DBServerClassID_HallServerStartID)
}

func (rh *HallServerStartHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.HallServerStart{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.HallServerStart)

	serverid := mm.GetServerID()
	clientaddr := mm.GetClientAddr()
	notifyaddr := mm.GetNotifyAddr()
	httpaddr := mm.GetHttpAddr()

	ps := db.NewSqlParameters()
	ps.AddIntInput("sintServerID", serverid)
	ps.AddVarcharInput("chvClientAddr", clientaddr, 32)
	ps.AddVarcharInput("chvNotifyAddr", notifyaddr, 32)
	ps.AddVarcharInput("chvHttpAddr", httpaddr, 32)

	proc := db.NewProcedure("PrPs_ServerStart", ps)
	dbret, err := DbUtil.ExecProc(proc)
	mlogWithSQL := mlog.NewDefaultLogger().SetData(proc.GetSqlString())
	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_DBServer), int32(netproto.DBServerClassID_HallServerStartRetID), nil)
	var code int32
	code = 1
	message := "服务器启动成功"
	if err != nil {
		message = fmt.Sprintf("执行存储过程PrPs_ServerStart错误, %v", err)
		mlogWithSQL.Error(message)
		code = 0
	}
	retdata := new(netproto.HallServerStartRet)
	retdata.Code = proto.Int32(code)
	retdata.Message = proto.String(message)

	if dbret.GetRetTableCount() >= 1 {
		retdata.AndroidVersion = proto.String(dbret[0].GetSingleValue("AndroidVersion").(string))
		retdata.IOSVersion = proto.String(dbret[0].GetSingleValue("IOSVersion").(string))
	} else {
		mlogWithSQL.Error("未获取到平台版本信息")
		retdata.Code = proto.Int32(0)
		retdata.Message = proto.String("未获取到平台版本信息")
	}

	if dbret.GetRetTableCount() >= 2 {
		retdata.UpdateDomain = proto.String(dbret[1].GetSingleValueString("UpdateDomain"))
		retdata.BillDomain = proto.String(dbret[1].GetSingleValueString("BillDomain"))
		retdata.ClubDomain = proto.String(dbret[1].GetSingleValueString("ClubDomain"))
		retdata.CustomerDomain = proto.String(dbret[1].GetSingleValueString("CustomerDomain"))
	} else {
		mlogWithSQL.Error("未获取到其他域名信息")
	}

	if dbret.GetRetTableCount() >= 3 {
		grt := dbret[2]
		grt.ForEachRow(func(r db.RecordRow) {
			areaConfig := new(netproto.AreaConfig)
			areaConfig.AreaID = proto.Int64(r[0].(int64))
			areaConfig.AreaConfig = proto.String(r[1].(string))
			retdata.AreaConfigList = append(retdata.AreaConfigList, areaConfig)
		})
	} else {
		mlogWithSQL.Error("未获取到渠道配置信息",)
	}

	retmsg.MsgData = retdata

	return []*msg.Message{retmsg}
}
