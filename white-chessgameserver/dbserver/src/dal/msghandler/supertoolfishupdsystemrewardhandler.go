package msghandler

import (
	"netproto"
	"module"
	"network"
	"msg"
	"rpcmsg"
	"example.invalid/golang/protobuf/proto"
	"db"
)

//捕魚 更新系統放分並取得放分
type SuperToolFishUpdSystemRewardHandler struct {}

func (fsu *SuperToolFishUpdSystemRewardHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Game) && sclsid == int32(netproto.GameMessageClassID_SuperToolFishUpdSystemRewardID)
}

func (fsu *SuperToolFishUpdSystemRewardHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {

	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.SuperToolFishUpdSystemRewardDetail{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.SuperToolFishUpdSystemRewardDetail)
	var retcode int32 = 1
	var ret db.RecordSet
	var err error
	for _, v := range mm.FishRewardList {
		ps := db.NewSqlParameters()
		ps.AddIntInput("intFlagID", mm.GetFlagID())
		ps.AddIntInput("intFishID", v.GetFishID())
		ps.AddBigIntInput("intFinishAmount", v.GetFinishAmount())
		proc := db.NewProcedure("PrGs_SuperTool_Fish_UpdSystemReward", ps)
		ret, err = DbUtil.ExecProc(proc)

		if err != nil {
			ctx.Error("执行存储过程PrGs_SuperTool_Fish_UpdSystemReward错误, %v, %s", err, proc.GetSqlString())
			retcode = 0
		}
		retcode = int32(ret.GetReturnValue())
	}

	var retmsg *msg.Message
	retmsg = msg.NewMessage(int32(netproto.MessageBClassID_Game), int32(netproto.GameMessageClassID_SuperToolFishUpdSystemRewardRetID), nil)
	retdata := new(netproto.SuperToolFishGetSystemRewardDetail)

	if retcode == 1 {
		if ret.GetRetTableCount() > 0 {
			tbData := ret[0]
			for i:=0; i<len(tbData.Rows); i++ {
				fishRewardList := new(netproto.FishSystemRewardDataList)
				fishRewardList.FishID = proto.Int32(int32(tbData.GetValueByColName(i, "FishID").(int64)))
				fishRewardList.Amount = proto.Int64(tbData.GetValueByColName(i, "Amount").(int64))
				fishRewardList.FinishAmount = proto.Int64(tbData.GetValueByColName(i, "FinishAmount").(int64))
				fishRewardList.Rate = proto.Int32(int32(tbData.GetValueByColName(i, "Rate").(int64)))
				fishRewardList.Status = proto.Int32(int32(tbData.GetValueByColName(i, "Status").(int64)))

				retdata.FishRewardList = append(retdata.FishRewardList, fishRewardList)
			}
		}
	}

	retmsg.MsgData = retdata
	return []*msg.Message{retmsg}
}