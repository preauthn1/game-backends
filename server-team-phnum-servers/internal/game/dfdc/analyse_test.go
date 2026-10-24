package dfdc

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

		b, _ := proto.Marshal(detail)

		_m := new(rpcmsg.RPCMessage)
		_m.MsgData = b

		mm := new(msg.Message)
		mm.BClassID = int32(netproto.MessageBClassID_Game)
		mm.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadRetID)
		mm.MsgData = _m

		logic.OnRecvRPCMessage(mm)
	}

	handlers[int32(netproto.GameMessageClassID_SlotJackpotNoLinkID)] = func() {

		all := new(netproto.Slot_JackpotAllData)
		for i := 0; i <= 3; i++ {
			detail := new(netproto.Slot_JackpotData)
			detail.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
			detail.TableNo = proto.Int32(0)
			f, _ := strconv.ParseFloat(logic.TableManager.Room.Flag, 64)
			detail.FlagID = proto.Int32(int32(f))
			detail.Level = proto.Int32(int32(i + 1))
			detail.Contribution = proto.Int64(0)
			detail.TotalContribution = proto.Int64(0)
			detail.AwardTotal = proto.Int64(0)
			all.AllData = append(all.AllData, detail)
		}

		b, _ := proto.Marshal(all)

		_m := new(rpcmsg.RPCMessage)
		_m.MsgData = b

		mm := new(msg.Message)
		mm.BClassID = int32(netproto.MessageBClassID_Game)
		mm.SClassID = int32(netproto.GameMessageClassID_SlotJackpotNoLinkRetID)
		mm.MsgData = _m

		logic.OnRecvRPCMessage(mm)
	}

	handlers[int32(netproto.DFDC_GameMessageClassID_DFDCJackpotBonusGameRetID)] = func() {

		data := new(netproto.DFDC_JackpotBonusGame)

		data.Money = proto.Int64(0)

		b, _ := proto.Marshal(data)

		_m := new(rpcmsg.RPCMessage)
		_m.MsgData = b

		mm := new(msg.Message)
		mm.BClassID = int32(netproto.MessageBClassID_Game)
		mm.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCJackpotBonusDetailID)
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

	var TotalPayMoney, TotalWinMoney, TotalWinCount, TotalJackpotWinMoney, TotalJackpotWinCount, TotalFreeSpinCount, TotalMainGameCount int64

	var WorkerCount = 4 * numCPUs
	var TotalCount = 10

	var BetMoney int64 = 1000
	var BetRate int64 = 88

	logic := createBasicTestLogic(SlotPattern)
	time.Sleep(1 * time.Second)

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
		if m.SClassID == int32(netproto.DFDC_GameMessageClassID_DFDCBetRetID) {
			data := m.MsgData.(*netproto.DFDC_BetRet)
			if data.GetRet() == 0 {
				TotalPayMoney += data.GetMoney()
				if data.GetMoney() == 0 {
					TotalFreeSpinCount++
				} else {
					TotalMainGameCount++
				}
			}
			return
		}
		if m.SClassID == int32(netproto.DFDC_GameMessageClassID_DFDCGameResult) {
			data := m.MsgData.(*netproto.DFDC_GameResultReel)
			TotalWinMoney += data.GetWinMoney()
			if data.GetWinMoney() > 0 {
				TotalWinCount++
			}
			return
		}
		if m.SClassID == int32(netproto.DFDC_GameMessageClassID_DFDCBonusGameID) {
			data := m.MsgData.(*netproto.DFDC_BonusGame)
			TotalJackpotWinMoney += data.GetMoney()
			TotalJackpotWinCount++
			return
		}
	}

	for i := 0; i < TotalCount; i++ {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCGameStartID)

		m := new(netproto.DFDC_StartBet)
		m.Money = proto.Int64(BetMoney)
		m.GoldenSymbol = proto.Int32(int32(BetRate))
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
	fmt.Printf("TotalMainGameCount: %d\n", TotalMainGameCount)
	fmt.Printf("TotalFreeSpinCount: %d\n", TotalFreeSpinCount)
	fmt.Printf("TotalPayMoney: %d\n", TotalPayMoney)
	fmt.Printf("TotalWinMoney: %d\n", TotalWinMoney)
	fmt.Printf("TotalWinCount: %d\n", TotalWinCount)
	fmt.Printf("TotalJackpotWinMoney: %d\n", TotalJackpotWinMoney)
	fmt.Printf("TotalJackpotWinCount: %d\n", TotalJackpotWinCount)
	fmt.Printf("RTP: %.4f%%\n", float64(TotalWinMoney+TotalJackpotWinMoney)/float64(TotalPayMoney)*100)
	fmt.Println("==================================================")

}
