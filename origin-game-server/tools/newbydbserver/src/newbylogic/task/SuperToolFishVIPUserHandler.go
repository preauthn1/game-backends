package task

import (
	"db"
	"example.invalid/golang/protobuf/proto"
	"mlog"
	"module"
	"msg"
	"netproto"
	"network"
	"util"
)

type SuperToolFishVIPUserHandler struct {
}

//捕鱼启动程序
func NewSuperToolFishVIPUserHandler() *SuperToolFishVIPUserHandler {
	handler := new(SuperToolFishVIPUserHandler)
	return handler
}

func (h *SuperToolFishVIPUserHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_NewByDBServer) && sclsid == int32(netproto.NewBYDBServerClassID_SuperToolFishVIPUserID)
}

func (h *SuperToolFishVIPUserHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	defer util.PanicErrStack()
	mmbyte := m.MsgData.([]byte)

	mm := &netproto.SuperToolFishVIPUserMsg{}
	err := proto.Unmarshal(mmbyte, mm)

	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_NewByDBServer), int32(netproto.NewBYDBServerClassID_SuperToolFishVIPUserRetID), nil)
	retdata := new(netproto.SuperToolFishGetVIPUserRewardDetail)

	if err != nil {
		ctx.Error("解析PrGs_SuperTool_Fish_UpdVIPUser错误,%v", err)
		retmsg.MsgData = retdata
		return []*msg.Message{retmsg}
	}

	d := GetDatabase()
	ps := db.NewSqlParameters()
	ps.AddIntInput("intGameID", mm.GetGameID())
	ps.AddIntInput("intUserID", mm.GetUserID())
	ps.AddIntInput("intCompleteAmount", mm.GetCompleteAmount())

	proc := db.NewProcedure("PrGs_SuperTool_Fish_UpdVIPUser", ps)
	ret, err := d.ExecProc(proc)

	defer func() {
		if err := recover(); err != nil {
			errorStr := util.GetStackInfo()
			mlog.NewDefaultLogger().Error("发生错误%s, 執行存儲過程%s", errorStr, proc.GetSqlString())
			panic(err)
		}
	}()

	retcode := int32(ret.GetReturnValue())
	if retcode == 1 {
		if ret.GetRetTableCount() > 0 {
			tbData := ret[0]
			for i:=0; i<len(tbData.Rows); i++ {
				vipUserList := new(netproto.FishVIPUserRewardDataList)
				vipUserList.UserID = proto.Int32(int32(tbData.GetValueByColName(i, "UserID").(int64)))
				vipUserList.AssignAmount = proto.Int64((tbData.GetValueByColName(i, "AssignAmount").(int64)))
				vipUserList.Rate = proto.Int32(int32(tbData.GetValueByColName(i, "AssignRate").(int64)))
				vipUserList.FinishAmount = proto.Int64(tbData.GetValueByColName(i, "CompleteAmount").(int64))
				vipUserList.Status = proto.Int32(int32(tbData.GetValueByColName(i, "Status").(int64)))
				vipUserList.SumAssignAmount = proto.Int64(tbData.GetValueByColName(i, "SumAssignAmount").(int64))

				retdata.FishVIPUserList = append(retdata.FishVIPUserList, vipUserList)
			}
		}
	}

	retmsg.MsgData = retdata
	return []*msg.Message{retmsg}
}


