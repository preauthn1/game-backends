package msghandler

import (
	"fmt"
	"util"

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

	defer func() {
		if err := recover(); err != nil {
			errorStr := util.GetStackInfo()
			mlog.NewDefaultLogger().SetData(dbret).Error("发生错误%s, 執行存儲過程%s", errorStr, proc.GetSqlString())
			panic(err)
		}
	}()

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
		retdata.AndroidVersion = proto.String(dbret[0].GetSingleValueString("AndroidVersion"))
		retdata.IOSVersion = proto.String(dbret[0].GetSingleValueString("IOSVersion"))
		retdata.AndroidVersion2 = proto.String(dbret[0].GetSingleValueString("AndroidVersion2"))
		retdata.IOSVersion2 = proto.String(dbret[0].GetSingleValueString("IOSVersion2"))
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

	//if dbret.GetRetTableCount() >= 4 {
	//	tbData := dbret[3]
	//	for i:=0; i<len(tbData.Rows); i++ {
	//		pareaShowGame := new(netproto.AreaShowGameFlag)
	//		pareaShowGame.AreaID = proto.Int32(int32(tbData.GetValueByColName(i, "AreaID").(int64)))
	//		pareaShowGame.GameID = proto.Int32(int32(tbData.GetValueByColName(i, "GameID").(int64)))
	//		pareaShowGame.FlagID = proto.Int32(int32(tbData.GetValueByColName(i, "FlagID").(int64)))
	//		pareaShowGame.ServerID = proto.Int32(int32(tbData.GetValueByColName(i, "ServerID").(int64)))
	//		statusBool := tbData.GetValueByColName(0, "Status").(bool)
	//		status := 1
	//		if statusBool == false {
	//			status = 0
	//		}
	//		pareaShowGame.Status = proto.Int32(int32(status))
	//		retdata.AreaShowGameFlagList = append(retdata.AreaShowGameFlagList, pareaShowGame)
	//	}
	//}

	retmsg.MsgData = retdata

	return []*msg.Message{retmsg}
}
