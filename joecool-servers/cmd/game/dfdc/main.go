package main

import (
	"fmt"
	"os"
	"strconv"
	"strings"

	"example.invalid/spf13/cobra"

	"example.invalid/ServerTeam/Servers/cmd/game/dfdc/analysis"
	"example.invalid/ServerTeam/Servers/internal/game/dfdc"

	"example.invalid/ServerTeam/Servers/pkg/command"
	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/roomserver"
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/baselogic"
	"example.invalid/ServerTeam/Servers/pkg/slot/defender"
	"example.invalid/ServerTeam/Servers/pkg/slot/helper"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

var path string
var proto string
var pattern int32

var rootCmd = &cobra.Command{
	Use:   "dfdc",
	Short: "多福多財",
	RunE: func(cmd *cobra.Command, args []string) error {

		// Load Config
		data, err := config.LoadConfigGroup(path)
		if err != nil {
			return err
		}

		// Load Proto
		Proto, err := protodata.NewProtoData(proto, dfdc.ProtoOptions...)
		if err != nil {
			return err
		}
		if err := Proto.Reload(); err != nil {
			return err
		}

		dfdc.Protodatas = Proto

		// 手動設定 Slot Pattern
		if pattern != -1 {
			section := data.GetConfigSection("slotconfig", "slotsetting")
			section["slotpattern"] = strconv.Itoa(int(pattern))
		}

		// 設定 Jackpot Defender
		{
			section := data.GetConfigSection("slotconfig", "jackpot-defender")
			if len(section) != 0 {
				var increment int64
				var target []slot.JackpotLevel
				if val, err := strconv.ParseFloat(section["increment"], 64); err == nil {
					increment = int64(val)
				} else {
					cmd.Printf("slotconfig.dat -> [jackpot-defender] -> increment parse error, %v\n", err)
				}
				strs := strings.Split(section["target"], ",")
				for _, s := range strs {
					target = append(target, slot.JackpotLevel(s))
				}
				if increment > 0 && len(target) > 0 {
					defender.JPDefender = defender.NewJackpotDefender(increment, target...)
					command.AddCommand(new(defender.JackpotDefenderCmd))
				}
			}
		}

		roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
			return dfdc.NewLogic(t, tfManager, cfgs)
		}
		// 可於需要的時候打開使用 by Rory 2019.12.26
		//roomserver.RobotLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) robotlogic.RobotLogic {
		//	return dfdc.NewMyRobotLogic(table, tfManager, robotCfg, configs, glc, evtChannel)
		//}

		gs, roomlogic, _, err := roomserver.SetupGameServer(data)
		if err != nil {
			mlog.Error("初始化服務器錯誤, %v", err)
			return err
		}

		helpercmd := helper.NewSlotHelperCmd(roomlogic)
		helpercmd.BaseLogicFunc = func(gl gamelogic.GameLogic) *baselogic.SlotLogicBase {
			return gl.(*dfdc.Logic).SlotLogicBase
		}
		command.AddCommand(helpercmd)

		// 啟動服務器
		err = gs.Start()
		if err != nil {
			mlog.Error("服務器啟動錯誤,%v", err)
			return err
		}

		mlog.System("關閉 Logger")
		mlog.GetCLogger().Enable(false)
		select {}
	},
}

var versionCmd = &cobra.Command{
	Use:   "version",
	Short: "顯示版本資訊",
	Run: func(cmd *cobra.Command, args []string) {
		cmd.Printf("===版本資訊===\n當前版本：%s GitHash：%s\n建置時間：%s\n建置編號：%s\n", conf.GitTag, conf.GitHash, conf.BuildTime, conf.BuildNum)
	},
}

func init() {
	cobra.MousetrapHelpText = ""

	rootCmd.AddCommand(versionCmd)
	analysis.GameName = "DFDC"
	rootCmd.AddCommand(analysis.AnalysisCommand)

	rootCmd.PersistentFlags().StringVarP(&path, "config", "c", "config", "config folder")
	rootCmd.PersistentFlags().StringVar(&proto, "proto", "protodata", "protodata folder")
	rootCmd.PersistentFlags().Int32VarP(&pattern, "pattern", "p", -1, "slot pattern")
}

func main() {

	if err := rootCmd.Execute(); err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(1)
	}

}
