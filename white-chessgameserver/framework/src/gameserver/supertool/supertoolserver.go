package supertool

import (
	"example.invalid/golang/protobuf/proto"
	"msg"
	"netproto"
	"rpcmsg"
	"strconv"
)

type GameSuperToolServer struct {
	DBMsgQueue *rpcmsg.RPCMsgQueue //数据库消息队列
	ServerId   int
	GameId int
	FlagId string
}

//超端 創建配桌型
func NewDynamicSuperTool(dbMsg *rpcmsg.RPCMsgQueue, serverId int, gameId int, flagId string) *GameSuperToolServer {
	st := new(GameSuperToolServer)
	st.DBMsgQueue = dbMsg
	st.ServerId = serverId
	st.GameId = gameId
	st.FlagId = flagId
	return st
}

//啟動服務器
func (st *GameSuperToolServer) Start() {
	st.syncFishSysReward()
	st.syncFishVIPUserReward()
	st.startFishSysPool()
	st.startPlayerPool()
}

func (st *GameSuperToolServer) Stop() {

}

func (st *GameSuperToolServer) syncFishSysReward() {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.GameMessageClassID_SuperToolFishUpdSystemRewardID)
	pdata := new(netproto.SuperToolFishUpdSystemRewardDetail)
	flagId, _ := strconv.Atoi(st.FlagId)
	pdata.FlagID = proto.Int32(int32(flagId))
	dataList := new(netproto.SuperToolFishUpdSystemRewardDataList)
	dataList.FishID = proto.Int32(999)
	dataList.FinishAmount = proto.Int64(0)

	pdata.FishRewardList = append(pdata.FishRewardList, dataList)
	pmsg.MsgData = pdata

	rpcinfo := netproto.CreateEmptyRPCInfo(int32(st.ServerId))
	st.DBMsgQueue.PushMessage(rpcinfo, false, pmsg)
}

func (st *GameSuperToolServer) startFishSysPool(){
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.GameMessageClassID_FishSysPoolID)
	pdata := new(netproto.FishSysPoolData)
	flagId, _ := strconv.Atoi(st.FlagId)
	pdata.FlagID = proto.Int32(int32(flagId))
	pdata.SumBetMoney = proto.Int64(0)
	pdata.SumBullet = proto.Int64(0)
	pdata.SysSumWinLoss = proto.Int64(0)

	pmsg.MsgData = pdata

	rpcinfo := netproto.CreateEmptyRPCInfo(int32(st.ServerId))
	st.DBMsgQueue.PushMessage(rpcinfo, false, pmsg)
}

func (st *GameSuperToolServer) startPlayerPool(){
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.GameMessageClassID_FishPlayerPoolID)
	pdata := new(netproto.FishPlayerPoolDetail)
	flagId, _ := strconv.Atoi(st.FlagId)
	pdata.FlagID = proto.Int32(int32(flagId))
	dataList := new(netproto.FishPlayerPoolDetailList)
	dataList.UserID = proto.Int32(0)
	dataList.HitMoney = proto.Int64(0)
	dataList.SpendMoney = proto.Int64(0)
	pdata.FishRewardList = append(pdata.FishRewardList, dataList)
	pmsg.MsgData = pdata

	rpcinfo := netproto.CreateEmptyRPCInfo(int32(st.ServerId))
	st.DBMsgQueue.PushMessage(rpcinfo, false, pmsg)
}

func (st *GameSuperToolServer) syncFishVIPUserReward() {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.GameMessageClassID_SuperToolFishUpdVIPUserRewardID)
	pdata := new(netproto.SuperToolfishUpdVIPUserRewardDetail)
	pdata.GameID = proto.Int32(int32(st.GameId))
	dataList := new(netproto.SuperToolFishUpdVIPUserRewardDataList)
	dataList.UserID = proto.Int32(0)
	dataList.FinishAmount = proto.Int64(0)
	pdata.FishUserRewardList = append(pdata.FishUserRewardList, dataList)
	pmsg.MsgData = pdata

	rpcinfo := netproto.CreateEmptyRPCInfo(int32(st.ServerId))
	st.DBMsgQueue.PushMessage(rpcinfo, false, pmsg)
}
