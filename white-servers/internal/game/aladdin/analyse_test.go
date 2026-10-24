package aladdin

import (
	"fmt"
	"math/rand"
	"runtime"
	"strconv"
	"sync"
	"testing"
	"time"

	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/slot/baselogic"
)

var mu sync.Mutex

func createBasicTestLogic(slotpattern uint32) *Logic {

	mu.Lock()
	defer mu.Unlock()

	cfg.Configs["slotconfig"].Sections["slotsetting"]["slotpattern"] = strconv.Itoa(int(slotpattern))

	logic := new(Logic)
	logic.SlotLogicBase = baselogic.NewSlotLogicBase(nil, nil, cfg, protodatas)
	logic.GameUser = &gamelogic.GameUser{UserID: 9936738, Score: 100000}
	logic.TableManager = &gamelogic.Table{
		Room: &gamelogic.Room{
			Flag: "1",
			GameInfo: &gamelogic.Game{
				GameID:   27,
				GameName: "阿拉丁",
			},
		},
	}
	logic.UpdateUserScoreFunc = func(userid int32, score int64, taxamount int64, coupon int64) {}
	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {}
	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {}

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

	var SlotPattern uint32 = 3

	var TotalPayMoney, TotalWinMoney, TotalWinCount, TotalStepCount, TotalRespinCount, TotalExpandCount int64

	var WorkerCount = 4 * numCPUs
	var TotalCount = 100

	var JobChan = make(chan int64, TotalCount)
	var ResultChan = make(chan *Logic, TotalCount)
	var BetMoney int64 = 1

	var Worker = func(id int, jobs chan int64, results chan *Logic) {
		for job := range jobs {
			logic := createBasicTestLogic(SlotPattern)

			logic.Bet(logic.GameUser, job)
			logic.Spin(1, false, logic.DefaultSymbolGroupID, nil)

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

		for _, step := range logic.StepResults {
			TotalStepCount++
			if step.IsRespin {
				TotalRespinCount++
			}
			if step.IsExpand {
				TotalExpandCount++
			}
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
	fmt.Printf("TotalStepCount: %d\n", TotalStepCount)
	fmt.Printf("TotalExpandCount: %d\n", TotalExpandCount)
	fmt.Printf("TotalRespinCount: %d\n", TotalRespinCount)
	fmt.Printf("RTP: %.4f%%\n", float64(TotalWinMoney)/float64(TotalPayMoney)*100)
	fmt.Println("==================================================")

}
