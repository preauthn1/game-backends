package candyhouse

import (
	"fmt"
	"gamelogic"
	"msg"
	"runtime"
	"testing"

	"example.invalid/server/pkg/slot"
)

func createBasicTestLogic(slotpattern uint32) *Logic {

	logic := new(Logic)
	logic.ComputeGameLogicBase = gamelogic.NewComputeGameLogicBase(nil, nil, nil)
	logic.GameUser = &gamelogic.GameUser{UserID: 9936738, Score: 100000}
	logic.TableManager = &gamelogic.Table{
		Room: &gamelogic.Room{
			Flag: "1",
			GameInfo: &gamelogic.Game{
				GameID:   26,
				GameName: "糖果屋",
			},
		},
	}
	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {}
	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {}

	logic.SlotPatternID = slotpattern

	logic.loadProtoDatas()

	// 設定預設值
	logic.DefaultReelNum = int32(logic.MainGameTable.GetReelNum())
	logic.DefaultRowNum = int32(logic.MainGameTable.GetRowNum())

	// 設定賠付線
	logic.PayLineMode = slot.PayLineMode(logic.SlotPattern.GetLineType())

	switch logic.PayLineMode {
	case slot.PayLineModeAll:
		logic.PaylineMatcher = slot.NewPaylineAllMatcher(logic.SymbolAttributes)
	case slot.PayLineModeFixed:
		logic.PaylineMatcher = slot.NewPaylineFixedMatcher(logic.SymbolAttributes, logic.SlotPayLine)
	}

	logic.BetCounts = []int32{10, 20, 30, 50, 100}
	logic.MaxBetCount = 1000
	logic.MinCarryMoney = 100

	switch logic.PayLineMode {
	case slot.PayLineModeAll:
		logic.PaylineMatcher = slot.NewPaylineAllMatcher(logic.SymbolAttributes)
	case slot.PayLineModeFixed:
		logic.PaylineMatcher = slot.NewPaylineFixedMatcher(logic.SymbolAttributes, logic.SlotPayLine)
	}

	logic.initJackpot()

	return logic
}

func Test_Analyse_Logic_Spin(t *testing.T) {

	numCPUs := runtime.NumCPU()

	var SlotPattern uint32 = 2

	var TotalPayMoney, TotalWinMoney, TotalWinCount, WinBonusCount, WinBonusMoney, WinJackpotCount, WinJackpotMoney int64

	var WorkerCount = 4 * numCPUs
	var TotalCount = 100

	var JobChan = make(chan int64, TotalCount)
	var ResultChan = make(chan *Logic, TotalCount)
	var BetMoney int64 = 10

	var Worker = func(id int, jobs chan int64, results chan *Logic) {
		for job := range jobs {
			logic := createBasicTestLogic(SlotPattern)

			logic.Bet(logic.GameUser, job)
			logic.Spin(logic.GameUser)

			results <- logic
		}
	}

	for i := 0; i < WorkerCount; i++ {
		go Worker(i, JobChan, ResultChan)
	}

	for i := 0; i < TotalCount; i++ {
		JobChan <- BetMoney
	}
	close(JobChan)

	for i := 0; i < TotalCount; i++ {
		logic := <-ResultChan

		TotalPayMoney += logic.GamePayMoney
		TotalWinMoney += logic.GameWinMoney
		if logic.GameWinMoney > 0 {
			TotalWinCount++
		}
		if logic.GameWinJackpot > 0 {
			WinJackpotCount++
			WinJackpotMoney += logic.GameWinJackpot
		}
		if logic.GameWinBonusGame {
			WinBonusCount++
			WinBonusMoney += logic.BonusGame.WinMoney
			TotalWinMoney += logic.BonusGame.WinMoney
		}

	}

	fmt.Println("==================================================")
	fmt.Printf("TotalCount: %d, WorkerCount: %d\n", TotalCount, WorkerCount)
	fmt.Printf("SlotPattern: %d\n", SlotPattern)
	fmt.Println("==================================================")
	fmt.Printf("Each BetMoney: %d\n", BetMoney)
	fmt.Printf("TotalPayMoney: %d\n", TotalPayMoney)
	fmt.Printf("TotalWinMoney: %d\n", TotalWinMoney)
	fmt.Printf("TotalWinCount: %d\n", TotalWinCount)
	fmt.Printf("WinJackpotCount: %d, WinJackpotMoney: %d, HitRate: %.4f%%\n", WinJackpotCount, WinJackpotMoney, float64(WinJackpotCount)/float64(TotalCount)*100)
	fmt.Printf("WinBonusCount: %d, WinBonusMoney: %d, HitRate: %.4f%%\n", WinBonusCount, WinBonusMoney, float64(WinBonusCount)/float64(TotalCount)*100)
	fmt.Printf("RTP: %.4f%%\n", float64(TotalWinMoney)/float64(TotalPayMoney)*100)
	fmt.Println("==================================================")

}
