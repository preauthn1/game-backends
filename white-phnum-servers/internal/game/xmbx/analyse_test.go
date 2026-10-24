package xmbx

import (
	"fmt"
	"math/rand"
	"runtime"
	"strconv"
	"testing"
	"time"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/fixtures"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/player"
	"example.invalid/ServerTeam/Servers/pkg/roomserver"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
)

func createBasicTestLogic(slotpattern uint32) *Logic {

	conf.TimerFastMode = 1000 //40
	conf.TestMode = true

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	user := fixtures.GetGameUser()[0]
	user.Score = 1000000000000
	user.UserType = gamelogic.UserType_Normal
	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	conn := &network.ConnInfo{}
	p := new(player.Player)
	p.UserID = user.UserID
	p.Conn = conn
	p.UserData = user
	grs.PlayerManager.SetPlayer(p)

	handlers := make(map[int32]func())
	handlers[int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadID)] = func() {

		detail := new(netproto.Slot_MSG_FreeGameRecord)
		example.invalid = new(netproto.Slot_FreeSpinDetail)
		example.invalid.UserID = proto.Int32(user.UserID)
		example.invalid.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
		f, _ := strconv.ParseFloat(logic.TableManager.Room.Flag, 64)
		example.invalid.FlagID = proto.Int32(int32(f))
		example.invalid.UserBet = proto.Int64(0)
		example.invalid.FreeSpinGroup = proto.Int32(0)
		example.invalid.Times = proto.Int32(0)
		example.invalid.ChooseRate = proto.Int32(0)
		example.invalid.WinMoney = proto.Int64(0)
		example.invalid.IsCoupon = proto.Bool(false)
		example.invalid.OriginID = proto.Int64(0)

		b, _ := proto.Marshal(detail)

		_m := new(rpcmsg.RPCMessage)
		_m.MsgData = b

		mm := new(msg.Message)
		mm.BClassID = int32(netproto.MessageBClassID_Game)
		mm.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadRetID)
		mm.MsgData = _m

		logic.OnRecvRPCMessage(mm)
	}

	handlers[int32(netproto.Slot_GameMessageClassID_Slot_JackpotNoLinkReadID)] = func() {
		detail := new(netproto.Slot_MSG_JackpotRecord)

		jp1 := new(netproto.Slot_JackpotData)
		jp1.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
		f, _ := strconv.ParseFloat(logic.TableManager.Room.Flag, 64)
		jp1.FlagID = proto.Int32(int32(f))
		jp1.TableNo = proto.Int32(logic.TableManager.TableNo)
		jp1.Contribution = proto.Int64(0)
		jp1.TotalContribution = proto.Int64(0)
		jp1.AwardTotal = proto.Int64(0)
		jp1.Level = proto.Int32(1)

		jp2 := new(netproto.Slot_JackpotData)
		jp2.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
		f, _ = strconv.ParseFloat(logic.TableManager.Room.Flag, 64)
		jp2.FlagID = proto.Int32(int32(f))
		jp2.TableNo = proto.Int32(logic.TableManager.TableNo)
		jp2.Contribution = proto.Int64(0)
		jp2.TotalContribution = proto.Int64(0)
		jp2.AwardTotal = proto.Int64(0)
		jp2.Level = proto.Int32(2)

		jp3 := new(netproto.Slot_JackpotData)
		jp3.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
		f, _ = strconv.ParseFloat(logic.TableManager.Room.Flag, 64)
		jp3.FlagID = proto.Int32(int32(f))
		jp3.TableNo = proto.Int32(logic.TableManager.TableNo)
		jp3.Contribution = proto.Int64(0)
		jp3.TotalContribution = proto.Int64(0)
		jp3.AwardTotal = proto.Int64(0)
		jp3.Level = proto.Int32(3)

		detail.Records = append(detail.Records, jp1)
		detail.Records = append(detail.Records, jp2)
		detail.Records = append(detail.Records, jp3)

		b, _ := proto.Marshal(detail)

		_m := new(rpcmsg.RPCMessage)
		_m.MsgData = b

		mm := new(msg.Message)
		mm.BClassID = int32(netproto.MessageBClassID_Game)
		mm.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_JackpotNoLinkReadRetID)
		mm.MsgData = _m

		logic.OnRecvRPCMessage(mm)
	}

	handlers[int32(netproto.XMBX_GameMessageClassID_XMBX_LightningReadID)] = func() {

		detail := new(netproto.XMBX_MSG_LightningRecord)
		detail.UserID = proto.Int32(user.UserID)
		detail.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
		f, _ := strconv.ParseFloat(logic.TableManager.Room.Flag, 64)
		detail.FlagID = proto.Int32(int32(f))
		detail.Data = proto.String("")

		b, _ := proto.Marshal(detail)

		_m := new(rpcmsg.RPCMessage)
		_m.MsgData = b

		mm := new(msg.Message)
		mm.BClassID = int32(netproto.MessageBClassID_Game)
		mm.SClassID = int32(netproto.XMBX_GameMessageClassID_XMBX_LightningReadRetID)
		mm.MsgData = _m

		logic.OnRecvRPCMessage(mm)
	}

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
		if handlers != nil {
			if f, ok := handlers[m.SClassID]; ok {
				f()
			}
		}
	}

	return logic
}

func Test_Analyse_Logic_Spin(t *testing.T) {

	go func() {
		for {
			rand.Seed(time.Now().UnixNano())
			time.Sleep(100 * time.Nanosecond)
		}
	}()

	numCPUs := runtime.NumCPU()

	var SlotPattern uint32 = 1

	var TotalPayMoney, TotalWinMoney, TotalWinCount int64
	var TotalFreeSpinCount, TotalMainGameCount int64
	var TotalMainGameWinCount, TotalMainGameWinMoney int64
	var TotalFreeSpinWinCount, TotalFreeSpinWinMoney int64
	var TotalLightningWinCount, TotalLightningWinMoney int64
	var TotalJackpotWinCount, TotalJackpotWinMoney int64

	var WorkerCount = 4 * numCPUs
	var TotalCount = 10

	var BetMoney int64 = 5
	var BetRate int64 = 1

	logic := createBasicTestLogic(SlotPattern)
	time.Sleep(1 * time.Second)

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
		if m.SClassID == int32(netproto.XMBX_GameMessageClassID_XMBX_LightningPrepareID) {
			data := m.MsgData.(*netproto.XMBX_MSG_LightningPrepare)

			_m := new(msg.Message)
			_m.BClassID = int32(netproto.MessageBClassID_Game)
			_m.SClassID = int32(netproto.XMBX_GameMessageClassID_XMBX_ActionLightningSpinID)

			m := new(netproto.XMBX_MSG_LightningShow)
			m.LightningStep = proto.Int32(data.GetLightningStep())
			b, _ := proto.Marshal(m)

			_m.MsgData = b

			logic.GameMessage(logic.GameUser, _m)
			time.Sleep(1 * time.Millisecond)

			return
		}
		if m.SClassID == int32(netproto.XMBX_GameMessageClassID_XMBX_LightningResultID) {
			data := m.MsgData.(*netproto.XMBX_MSG_LightningResult)
			TotalWinMoney += data.GetWinMoney()
			TotalLightningWinMoney += data.GetWinMoney()
			if data.GetWinMoney() > 0 {
				TotalLightningWinCount++

				for _, row := range data.GetPayGroups() {
					if row.GetType() == "jackpot" {
						TotalJackpotWinCount++
						TotalJackpotWinMoney += row.GetWinMoney()
					}
				}
			}
			return
		}
		if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_BetStatusID) {
			data := m.MsgData.(*netproto.Slot_MSG_BetStatus)

			if data.GetRet() == 0 {
				if data.GetMoney() == 0 {
					TotalFreeSpinCount++
				} else {
					TotalMainGameCount++
				}
				TotalPayMoney += data.GetMoney()
			}

			return
		}
		if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_GameResultID) {
			data := m.MsgData.(*netproto.Slot_MSG_GameResult)

			TotalWinMoney += data.GetWinMoney()

			if data.GetBetMoney() == 0 {
				TotalFreeSpinWinMoney += data.GetWinMoney()
				if data.GetWinMoney() > 0 {
					TotalFreeSpinWinCount++
				}
			} else {
				TotalMainGameWinMoney += data.GetWinMoney()
				if data.GetWinMoney() > 0 {
					TotalMainGameWinCount++
				}
			}

			if data.GetWinMoney() > 0 {
				TotalWinCount++
			}

			return
		}
	}

	for i := 0; i < TotalCount; i++ {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(BetMoney)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)
		time.Sleep(1 * time.Millisecond)
	}

	fmt.Println("==================================================")
	fmt.Printf("TotalCount: %d, WorkerCount: %d\n", TotalCount, WorkerCount)
	fmt.Printf("SlotPattern: %d\n", SlotPattern)
	fmt.Println("==================================================")
	fmt.Printf("Each BetMoney: %d, BetRate: %d\n", BetMoney, BetRate)
	fmt.Printf("TotalMainGameCount: %d (%.2f%%)\n", TotalMainGameCount, float64(TotalMainGameCount)/float64(TotalCount)*100)
	fmt.Printf("TotalFreeSpinCount: %d (%.2f%%)\n", TotalFreeSpinCount, float64(TotalFreeSpinCount)/float64(TotalCount)*100)
	fmt.Printf("TotalPayMoney: %d\n", TotalPayMoney)
	fmt.Printf("TotalWinMoney: %d\n", TotalWinMoney)
	fmt.Printf("TotalWinCount: %d\n", TotalWinCount)
	fmt.Printf("TotalMainGameWinCount: %d\n", TotalMainGameWinCount)
	fmt.Printf("TotalMainGameWinMoney: %d (%.4f%%)\n", TotalMainGameWinMoney, float64(TotalMainGameWinMoney)/float64(TotalWinMoney)*100)
	fmt.Printf("TotalFreeSpinWinCount: %d\n", TotalFreeSpinWinCount)
	fmt.Printf("TotalFreeSpinWinMoney: %d (%.4f%%)\n", TotalFreeSpinWinMoney, float64(TotalFreeSpinWinMoney)/float64(TotalWinMoney)*100)
	fmt.Printf("TotalLightningWinCount: %d\n", TotalLightningWinCount)
	fmt.Printf("TotalLightningWinMoney: %d (%.4f%%) (Incloud Jackpot Money)\n", TotalLightningWinMoney, float64(TotalLightningWinMoney)/float64(TotalWinMoney)*100)
	fmt.Printf("TotalJackpotWinCount: %d\n", TotalJackpotWinCount)
	fmt.Printf("TotalJackpotWinMoney: %d (%.4f%%)\n", TotalJackpotWinMoney, float64(TotalJackpotWinMoney)/float64(TotalWinMoney)*100)
	fmt.Printf("RTP: %.4f%%\n", float64(TotalWinMoney)/float64(TotalPayMoney)*100)
	fmt.Println("==================================================")

}
