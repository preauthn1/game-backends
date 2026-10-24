package xmbx

import (
	"fmt"
	"math/rand"
	"runtime"
	"strconv"
	"strings"
	"sync"
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
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
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

		detail := new(netproto.Slot_MSG_LightningRecord)
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

	var TotalPayMoney, TotalWinMoney, TotalWinCount, BetFailCount int64
	var TotalFreeSpinCount, TotalMainGameCount int64
	var TotalMainGameWinCount, TotalMainGameWinMoney int64
	var TotalFreeSpinWinCount, TotalFreeSpinWinMoney int64
	var TotalLightningWinCount, TotalLightningWinMoney int64
	var TotalLightningMainCount, TotalLightningFreeCount int64
	var TotalLightningMainMoney, TotalLightningFreeMoney int64
	var TotalJackpotWinCount, TotalJackpotWinMoney int64
	var TotalJackpotMajorCount, TotalJackpotMinorCount, TotalJackpotMiniCount int64

	var WorkerCount = 4 * numCPUs
	var TotalCount = 5

	GoldBallMap := make(map[string]int)
	var mu sync.Mutex

	var BetMoney int64 = 1
	var BetRate int64 = 1

	logic := createBasicTestLogic(SlotPattern)
	time.Sleep(1 * time.Second)

	t.Log("MaxBetCount: ", logic.MaxBetCount)
	t.Log("MaxBetRate: ", logic.MaxBetRate)
	t.Log("JackpotMajor: ", logic.Jackpots[slot.JackpotMajor].GetCurrentPool())
	t.Log("JackpotMinor: ", logic.Jackpots[slot.JackpotMinor].GetCurrentPool())
	t.Log("JackpotMini: ", logic.Jackpots[slot.JackpotMini].GetCurrentPool())

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
		if m.SClassID == int32(netproto.XMBX_GameMessageClassID_XMBX_LightningPrepareID) {
			data := m.MsgData.(*netproto.Slot_MSG_LightningPrepare)

			_m := new(msg.Message)
			_m.BClassID = int32(netproto.MessageBClassID_Game)
			_m.SClassID = int32(netproto.XMBX_GameMessageClassID_XMBX_ActionLightningSpinID)

			m := new(netproto.Slot_MSG_LightningShow)
			m.LightningStep = proto.Int32(data.GetLightningStep())
			b, _ := proto.Marshal(m)

			_m.MsgData = b

			logic.GameMessage(logic.GameUser, _m)
			time.Sleep(1 * time.Millisecond)

			return
		}
		if m.SClassID == int32(netproto.XMBX_GameMessageClassID_XMBX_LightningResultID) {
			data := m.MsgData.(*netproto.Slot_MSG_LightningResult)
			TotalWinMoney += data.GetWinMoney()
			TotalLightningWinMoney += data.GetWinMoney()
			if data.GetWinMoney() > 0 {
				TotalLightningWinCount++

				for _, row := range data.GetPayGroups() {
					if row.GetType() == "jackpot" {
						TotalJackpotWinCount++
						TotalJackpotWinMoney += row.GetWinMoney()

						if logic.Jackpots[slot.JackpotMajor].GetCurrentPool() == row.GetWinMoney() {
							TotalJackpotMajorCount++
						}
						if logic.Jackpots[slot.JackpotMinor].GetCurrentPool() == row.GetWinMoney() {
							TotalJackpotMinorCount++
						}
						if logic.Jackpots[slot.JackpotMini].GetCurrentPool() == row.GetWinMoney() {
							TotalJackpotMiniCount++
						}
					}
				}

				if logic.LightningGame.IsFreeGame {
					TotalLightningFreeCount++
					TotalLightningFreeMoney += data.GetWinMoney()
				} else {
					TotalLightningMainCount++
					TotalLightningMainMoney += data.GetWinMoney()
				}

				for _, row := range data.GetReelResult() {
					symbol := row.GetSymbol()
					if strings.HasPrefix(symbol.GetSymbolName(), "g") {
						mu.Lock()
						GoldBallMap[symbol.GetSymbolName()]++
						mu.Unlock()
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
			} else {
				BetFailCount++
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
	fmt.Printf("BetFailCount: %d (%.2f%%)\n", BetFailCount, float64(BetFailCount)/float64(TotalCount)*100)
	fmt.Printf("TotalMainGameCount: %d (%.2f%%)\n", TotalMainGameCount, float64(TotalMainGameCount)/float64(TotalCount)*100)
	fmt.Printf("TotalFreeSpinCount: %d (%.2f%%)\n", TotalFreeSpinCount, float64(TotalFreeSpinCount)/float64(TotalCount)*100)
	fmt.Printf("TotalPayMoney: %d\n", TotalPayMoney)
	fmt.Printf("TotalWinMoney: %d\n", TotalWinMoney)
	fmt.Printf("TotalWinCount: %d\n", TotalWinCount)
	fmt.Printf("TotalMainGameWinCount: %d\n", TotalMainGameWinCount)
	fmt.Printf("TotalMainGameWinMoney: %d (%.4f%%)\n", TotalMainGameWinMoney, float64(TotalMainGameWinMoney)/float64(TotalWinMoney)*100)
	fmt.Printf("TotalFreeSpinWinCount: %d\n", TotalFreeSpinWinCount)
	fmt.Printf("TotalFreeSpinWinMoney: %d (%.4f%%)\n", TotalFreeSpinWinMoney, float64(TotalFreeSpinWinMoney)/float64(TotalWinMoney)*100)
	fmt.Printf("TotalLightningWinCount: %d (Main: %d + Free: %d)\n", TotalLightningWinCount, TotalLightningMainCount, TotalLightningFreeCount)
	fmt.Printf("TotalLightningWinMoney: %d (%.4f%%) (Incloud Jackpot Money) (Main: %d + Free: %d)\n", TotalLightningWinMoney, float64(TotalLightningWinMoney)/float64(TotalWinMoney)*100, TotalLightningMainMoney, TotalLightningFreeMoney)
	fmt.Printf("TotalJackpotWinCount: %d (Major: %d, Minor: %d, Mini: %d)\n", TotalJackpotWinCount, TotalJackpotMajorCount, TotalJackpotMinorCount, TotalJackpotMiniCount)
	fmt.Printf("TotalJackpotWinMoney: %d (%.4f%%)\n", TotalJackpotWinMoney, float64(TotalJackpotWinMoney)/float64(TotalWinMoney)*100)
	fmt.Printf("RTP: %.4f%%\n", float64(TotalWinMoney)/float64(TotalPayMoney)*100)
	fmt.Println("==================================================")
	fmt.Printf("Lightning Gold Ball Appear:\n")
	fmt.Printf("  -- G1: %d\n", GoldBallMap["g1"])
	fmt.Printf("  -- G2: %d\n", GoldBallMap["g2"])
	fmt.Printf("  -- G3: %d\n", GoldBallMap["g3"])
	fmt.Printf("  -- G4: %d\n", GoldBallMap["g4"])
	fmt.Printf("  -- G5: %d\n", GoldBallMap["g5"])
	fmt.Printf("  -- G6: %d\n", GoldBallMap["g6"])
	fmt.Printf("  -- G7: %d\n", GoldBallMap["g7"])
	fmt.Printf("  -- G8: %d\n", GoldBallMap["g8"])
	fmt.Printf("  -- G9: %d\n", GoldBallMap["g9"])
	fmt.Printf("  -- G10: %d\n", GoldBallMap["g10"])
	fmt.Printf("  -- G11: %d\n", GoldBallMap["g11"])
	fmt.Printf("  -- G12: %d\n", GoldBallMap["g12"])
	fmt.Printf("  -- G13: %d\n", GoldBallMap["g13"])
	fmt.Printf("  -- G14: %d\n", GoldBallMap["g14"])
	fmt.Printf("  -- G15: %d\n", GoldBallMap["g15"])
	fmt.Printf("  -- G16: %d\n", GoldBallMap["g16"])
	fmt.Printf("  -- G17: %d\n", GoldBallMap["g17"])
	fmt.Printf("  -- G18: %d\n", GoldBallMap["g18"])
}

func Test_Analyse_LightningGameSpin603(t *testing.T) {
	conf.CLogLevel = 3
	conf.TimerFastMode = 1000 //40
	conf.TestMode = true

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	user := fixtures.GetGameUser()[0]
	user.Score = 10000
	user.UserType = gamelogic.UserType_Normal
	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	result := slot.GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Symbol: proto.String("g1")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("g1")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("g1")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("g1")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("g1")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Symbol: proto.String("g1")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p7")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p8")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p9")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p10")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p11")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p12")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p13")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p14")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p15")},
		},
	}
	manager, _ := slot.NewGameResultManager(5, 3)
	manager.GameResult = result

	//logic_test.go:91: id:603 groupId:603 respinCount:3 respinType:"Lightning" respinValue:603
	respinTable := &protodata.RespinTable{Id: proto.Uint32(603), GroupId: proto.Int32(603), RespinCount: proto.Uint32(3), RespinType: proto.String("Lightning"), RespinValue: proto.Uint32(603)}

	//logic_test.go:81: id:262 groupId:6 linkType:"any" symbol:"g1" payType:"lightning" count:6 payout:"603.0" useType:"normal"
	rule := &protodata.SlotPayTable{Id: proto.Int32(262), GroupId: proto.Int32(6), LinkType: proto.String("any"), Symbol: proto.String("g1"), PayType: proto.String("lightning"), Count: proto.Int32(6), Payout: proto.String("603"), UseType: proto.String("normal")}

	var GoldBallMap map[string]int
	GoldBallMap = make(map[string]int)
	var mu sync.Mutex

	for i := 0; i < 10; i++ {
		logic.LightningGame = NewLightningGame(logic, false)
		logic.LightningGame.AddStep(NewLightningStep(respinTable, rule, manager, int(respinTable.GetRespinCount())))
		logic.LightningGame.Spin()

		step := logic.LightningGame.GetLastStep()

		for _, row := range step.GameResultManager.GameResult {
			for _, symbol := range row {
				if strings.HasPrefix(symbol.GetSymbol(), "g") {
					mu.Lock()
					GoldBallMap[symbol.GetSymbol()]++
					mu.Unlock()
				}
			}
		}

		mu.Lock()
		GoldBallMap["g1"] -= 6
		mu.Unlock()

		logic.LightningGame = nil
	}

	fmt.Printf("Lightning Gold Ball Appear:\n")
	fmt.Printf("  -- G1: %d\n", GoldBallMap["g1"])
	fmt.Printf("  -- G2: %d\n", GoldBallMap["g2"])
	fmt.Printf("  -- G3: %d\n", GoldBallMap["g3"])
	fmt.Printf("  -- G4: %d\n", GoldBallMap["g4"])
	fmt.Printf("  -- G5: %d\n", GoldBallMap["g5"])
	fmt.Printf("  -- G6: %d\n", GoldBallMap["g6"])
	fmt.Printf("  -- G7: %d\n", GoldBallMap["g7"])
	fmt.Printf("  -- G8: %d\n", GoldBallMap["g8"])
	fmt.Printf("  -- G9: %d\n", GoldBallMap["g9"])
	fmt.Printf("  -- G10: %d\n", GoldBallMap["g10"])
	fmt.Printf("  -- G11: %d\n", GoldBallMap["g11"])
	fmt.Printf("  -- G12: %d\n", GoldBallMap["g12"])
	fmt.Printf("  -- G13: %d\n", GoldBallMap["g13"])
	fmt.Printf("  -- G14: %d\n", GoldBallMap["g14"])
	fmt.Printf("  -- G15: %d\n", GoldBallMap["g15"])
	fmt.Printf("  -- G16: %d\n", GoldBallMap["g16"])
	fmt.Printf("  -- G17: %d\n", GoldBallMap["g17"])
	fmt.Printf("  -- G18: %d\n", GoldBallMap["g18"])

}

func Test_Analyse_LightningGameSpin604(t *testing.T) {
	conf.CLogLevel = 3
	conf.TimerFastMode = 1000 //40
	conf.TestMode = true

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	user := fixtures.GetGameUser()[0]
	user.Score = 10000
	user.UserType = gamelogic.UserType_Normal
	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	result := slot.GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Symbol: proto.String("g1")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("g1")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("g1")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("g1")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("g1")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Symbol: proto.String("g1")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p7")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p8")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p9")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p10")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p11")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p12")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p13")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p14")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p15")},
		},
	}
	manager, _ := slot.NewGameResultManager(5, 3)
	manager.GameResult = result

	//logic_test.go:91: id:604 groupId:604 respinCount:3 respinType:"Lightning" respinValue:604
	respinTable := &protodata.RespinTable{Id: proto.Uint32(604), GroupId: proto.Int32(604), RespinCount: proto.Uint32(3), RespinType: proto.String("Lightning"), RespinValue: proto.Uint32(604)}

	//logic_test.go:81: id:271 groupId:6 linkType:"any" symbol:"g1" payType:"lightning" count:6 payout:"604.0" useType:"freespin"
	rule := &protodata.SlotPayTable{Id: proto.Int32(271), GroupId: proto.Int32(6), LinkType: proto.String("any"), Symbol: proto.String("g1"), PayType: proto.String("lightning"), Count: proto.Int32(6), Payout: proto.String("604"), UseType: proto.String("freespin")}

	var GoldBallMap map[string]int
	GoldBallMap = make(map[string]int)
	var mu sync.Mutex

	for i := 0; i < 10; i++ {
		logic.LightningGame = NewLightningGame(logic, false)
		logic.LightningGame.AddStep(NewLightningStep(respinTable, rule, manager, int(respinTable.GetRespinCount())))
		logic.LightningGame.Spin()

		step := logic.LightningGame.GetLastStep()

		for _, row := range step.GameResultManager.GameResult {
			for _, symbol := range row {
				if strings.HasPrefix(symbol.GetSymbol(), "g") {
					mu.Lock()
					GoldBallMap[symbol.GetSymbol()]++
					mu.Unlock()
				}
			}
		}

		mu.Lock()
		GoldBallMap["g1"] -= 6
		mu.Unlock()

		logic.LightningGame = nil
	}

	fmt.Printf("Lightning Gold Ball Appear:\n")
	fmt.Printf("  -- G1: %d\n", GoldBallMap["g1"])
	fmt.Printf("  -- G2: %d\n", GoldBallMap["g2"])
	fmt.Printf("  -- G3: %d\n", GoldBallMap["g3"])
	fmt.Printf("  -- G4: %d\n", GoldBallMap["g4"])
	fmt.Printf("  -- G5: %d\n", GoldBallMap["g5"])
	fmt.Printf("  -- G6: %d\n", GoldBallMap["g6"])
	fmt.Printf("  -- G7: %d\n", GoldBallMap["g7"])
	fmt.Printf("  -- G8: %d\n", GoldBallMap["g8"])
	fmt.Printf("  -- G9: %d\n", GoldBallMap["g9"])
	fmt.Printf("  -- G10: %d\n", GoldBallMap["g10"])
	fmt.Printf("  -- G11: %d\n", GoldBallMap["g11"])
	fmt.Printf("  -- G12: %d\n", GoldBallMap["g12"])
	fmt.Printf("  -- G13: %d\n", GoldBallMap["g13"])
	fmt.Printf("  -- G14: %d\n", GoldBallMap["g14"])
	fmt.Printf("  -- G15: %d\n", GoldBallMap["g15"])
	fmt.Printf("  -- G16: %d\n", GoldBallMap["g16"])
	fmt.Printf("  -- G17: %d\n", GoldBallMap["g17"])
	fmt.Printf("  -- G18: %d\n", GoldBallMap["g18"])

}
