package analysis

import (
	"fmt"
	"io"
	"math/rand"
	"os"
	"runtime/debug"
	"strconv"
	"time"

	"example.invalid/spf13/cobra"
	"example.invalid/ServerTeam/Servers/internal/game/jjbx"
	"example.invalid/ServerTeam/Servers/internal/game/jjbx/robot"
	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/fixtures"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/robot/robotcomm"
	"example.invalid/ServerTeam/Servers/pkg/robot/robotlogic"
	"example.invalid/ServerTeam/Servers/pkg/roomserver"
	"example.invalid/ServerTeam/Servers/pkg/slot/helper"
	"example.invalid/ServerTeam/Servers/pkg/slot/mock"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

// GameName 遊戲名稱
var GameName string

var path string
var proto string
var pattern int32

var times int
var random bool

var freespinOnly bool
var lightningOnly bool

var skip int
var betRate int64

func init() {
	AnalysisCommand.PersistentFlags().StringVarP(&path, "config", "c", "config", "config folder")
	AnalysisCommand.PersistentFlags().StringVar(&proto, "proto", "protodata", "protodata folder")
	AnalysisCommand.PersistentFlags().Int32VarP(&pattern, "pattern", "p", -1, "slot pattern")
	AnalysisCommand.PersistentFlags().IntVarP(&times, "times", "t", 100, "bet spin count")
	AnalysisCommand.PersistentFlags().BoolVarP(&random, "random", "r", false, "random bet")
	AnalysisCommand.PersistentFlags().BoolVarP(&freespinOnly, "select-freespin", "f", false, "free spin only when selection mode")
	AnalysisCommand.PersistentFlags().BoolVarP(&lightningOnly, "select-lightning", "l", false, "lightning only when selection mode")
	AnalysisCommand.PersistentFlags().IntVarP(&skip, "select-skip", "s", 0, "Select number of Option")
	AnalysisCommand.PersistentFlags().Int64Var(&betRate, "bet-rate", -1, "Bet Rate")

	debug.SetMaxStack(1e10)
}

// AnalysisCommand 分析工具
var AnalysisCommand = &cobra.Command{
	Use:   "analysis",
	Short: "分析工具",
	RunE: func(cmd *cobra.Command, args []string) error {

		// Load Config
		data, err := config.LoadConfigGroup(path)
		if err != nil {
			return err
		}

		// Load Proto
		Proto, err := protodata.NewProtoData(proto, jjbx.ProtoOptions...)
		if err != nil {
			return err
		}
		if err := Proto.Reload(); err != nil {
			return err
		}

		jjbx.Protodatas = Proto

		// 手動設定 Slot Pattern
		if pattern != -1 {
			section := data.GetConfigSection("slotconfig", "slotsetting")
			section["slotpattern"] = strconv.Itoa(int(pattern))
		}

		helper.AnalysisModeConfig(data)

		roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
			return jjbx.NewLogic(t, tfManager, cfgs)
		}

		roomserver.RobotLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) robotlogic.RobotLogic {
			rbt := robot.NewAnalysisRobot(table, tfManager, robotCfg, configs, glc, evtChannel, times, random, freespinOnly, lightningOnly)
			rbt.SelectSkip = skip
			rbt.MainBetRate = betRate
			return rbt
		}

		// 每 5 秒 更新一次種子
		go func() {
			for {
				rand.Seed(time.Now().UnixNano())
				time.Sleep(5 * time.Second)
			}
		}()

		conf.CLogLevel = mlog.WarnLevel
		conf.ClearLog = true
		conf.AnalysisMode = true

		grs := fixtures.NewTestGameRoomServerWithConfig(data)
		logic := grs.GameLogicManager.GetGameLogic(0).(*jjbx.Logic)
		rbtlogic := grs.RobotService.GetRobotLogicManager().GetRobotLogic(0).(*robot.AnalyseRobot)

		// helper.PrintLogicProto(cmd.OutOrStdout(), logic.SlotLogicBase, true)

		robot := fixtures.GetRobots()[0]
		user := robot.UserInfo
		user.Score *= 1000

		logic.SendMessgeFunc = func(userid int32, m *msg.Message) {
			rbtlogic.OnGameMessage(robot, m)
		}

		var logs []*netproto.Slot_MSG_GameLogAdd

		logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
			if int32(netproto.Slot_GameMessageClassID_Slot_JackpotNoLinkReadID) == m.SClassID {
				mm := mock.EmptyJackpotPoolRPCMessage(logic.SlotLogicBase, user)
				logic.OnRecvRPCMessage(mm)
				return
			}
			if int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadID) == m.SClassID {
				mm := mock.EmptyFreeSpinRPCMessage(logic.SlotLogicBase, user)
				logic.OnRecvRPCMessage(mm)
				return
			}
			if int32(netproto.Slot_GameMessageClassID_Slot_LightningReadID) == m.SClassID {
				mm := mock.EmptyLightningRPCMessage(logic.SlotLogicBase, user)
				logic.OnRecvRPCMessage(mm)
				return
			}
			if int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadID) == m.SClassID {
				mm := mock.EmptySelectionRPCMessage(logic.SlotLogicBase, user)
				logic.OnRecvRPCMessage(mm)
				return
			}
			if int32(netproto.Slot_GameMessageClassID_Slot_BonusGameReadID) == m.SClassID {
				detail := mock.EmptyBonusGameDetail(logic.SlotLogicBase, user)
				mm := mock.MarshalBonusGame(detail)
				logic.OnRecvRPCMessage(mm)
				return
			}
			if int32(netproto.Slot_GameMessageClassID_Slot_GameLogBatchAddID) == m.SClassID {
				logs = append(logs, m.MsgData.(*netproto.Slot_MSG_BatchGameLogAdd).GetLogs()...)
				return
			}
			if int32(netproto.Slot_GameMessageClassID_Slot_GameLogAddID) == m.SClassID {
				logs = append(logs, m.MsgData.(*netproto.Slot_MSG_GameLogAdd))
				return
			}
		}

		go func() {
			logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
		}()

		for {
			select {
			case <-rbtlogic.Terminate:

				time.Sleep(1 * time.Second)

				key := fmt.Sprintf("[%s]-%s", robot.UserInfo.NickName, time.Now().Format("20060102_15_04_05"))

				rpf, err := os.Create(fmt.Sprintf("%s-%s-report", GameName, key))
				if err != nil {
					return err
				}
				defer rpf.Close()

				csvf, err := os.Create(fmt.Sprintf("%s-%s-data.csv", GameName, key))
				if err != nil {
					return err
				}
				defer csvf.Close()

				w := io.MultiWriter(cmd.OutOrStdout(), rpf)

				rbtlogic.PrintReportByLogs(w, logs)
				helper.OutputHistory(csvf, logs)

				return nil
			default:
				time.Sleep(100 * time.Millisecond)
			}
		}
	},
}
