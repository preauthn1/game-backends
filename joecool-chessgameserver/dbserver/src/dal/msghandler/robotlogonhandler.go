package msghandler

import (
	"db"
	"fmt"
	"module"
	"msg"
	"netproto"
	"network"
	"rpcmsg"
)

import (
	proto "example.invalid/golang/protobuf/proto"
	"mlog"
)

//机器人登录处理器
type RobotLogonHandler struct {
}

func (h *RobotLogonHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_DBServer) && sclsid == int32(netproto.DBServerClassID_LogonRobotID)
}

func (h *RobotLogonHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.LogonRobot{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.LogonRobot)

	example.invalid("收到机器人登录数据%v", mm)

	ps := db.NewSqlParameters()
	ps.AddIntInput("intServerID", mm.GetServerID())
	ps.AddIntInput("intCurrentRobotCount", mm.GetCurrentRobotCount())
	ps.AddBigIntInput("lngMinMoneyAmount", mm.GetMinMoneyAmount())
	ps.AddBigIntInput("lngMaxMoneyAmount", mm.GetMaxMoneyAmount())
	ps.AddIntOutput("intLogoutRobotCount", 0)
	proc := db.NewProcedure("PrGs_RobotLogon", ps)
	dbret, err := DbUtil.ExecProc(proc)
	//mlog.Error(proc.GetSqlString())
	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_DBServer), int32(netproto.DBServerClassID_LogonRobotRetID), nil)
	retdata := new(netproto.LogonRobotRet)
	mlog.Debug("%s", proc.GetSqlString())
	if err != nil {
		strMsg := fmt.Sprintf("执行存储过程PrGs_RobotLogon错误, %v", err)
		mlog.Error(strMsg)
	} else {
		logonRobotCount := int32(dbret.GetOutputParamValue("intLogoutRobotCount").(int64))
		if logonRobotCount == 0 && mm.GetCurrentRobotCount() == 0 {
			mlog.Warn("目前機器人數量0, 加載機器人數量0, proc:%s", proc.GetSqlString())
		}

		if logonRobotCount > 0 && dbret.GetRetTableCount() > 1 {
			tbRobot := dbret[0]
			for i := 0; i < len(tbRobot.Rows); i++ {
				robotData := new(netproto.RobotData)
				robotData.UserID = proto.Int32(int32(tbRobot.GetValueByColName(i, "UserID").(int64)))
				robotData.UserType = proto.Int32(int32(tbRobot.GetValueByColName(i, "UserType").(int64)))
				robotData.NickName = proto.String(tbRobot.GetValueByColName(i, "NickName").(string))
				robotData.FaceID = proto.Int32(int32(tbRobot.GetValueByColName(i, "FaceID").(int64)))
				robotData.Sex = proto.Int32(int32(tbRobot.GetValueByColName(i, "Sex").(int64)))
				robotData.CashAmount = proto.Int64(tbRobot.GetValueByColName(i, "CashAmount").(int64))
				robotData.WinCount = proto.Int32(int32(tbRobot.GetValueByColName(i, "WinCount").(int64)))
				robotData.LoseCount = proto.Int32(int32(tbRobot.GetValueByColName(i, "LoseCount").(int64)))
				robotData.DrawCount = proto.Int32(int32(tbRobot.GetValueByColName(i, "DrawCount").(int64)))
				robotData.RobotLevelID = proto.Int32(int32(tbRobot.GetValueByColName(i, "LevelID").(int64)))

				retdata.UserData = append(retdata.UserData, robotData)
			}
		}

		retdata.RobotCount = proto.Int32(logonRobotCount)
	}

	retmsg.MsgData = retdata

	example.invalid("返回机器人登录数据%v", retdata)

	return []*msg.Message{retmsg}
}
