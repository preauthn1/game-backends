package main

import (
	"fmt"
	"os"
	"strconv"

	"example.invalid/spf13/cobra"

	"example.invalid/ServerTeam/Servers/cmd/game/wlzbjp/analysis"
	"example.invalid/ServerTeam/Servers/internal/game/wlzbjp"

	"example.invalid/ServerTeam/Servers/pkg/command"
	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/roomserver"
	"example.invalid/ServerTeam/Servers/pkg/slot/baselogic"
	"example.invalid/ServerTeam/Servers/pkg/slot/helper"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

var path string
var proto string
var pattern int32

var rootCmd = &cobra.Command{
	Use:   "csdjp",
	Short: "財神到_彩金版",
	RunE: func(cmd *cobra.Command, args []string) error {

		// Load Config
		data, err := config.LoadConfigGroup(path)
		if err != nil {
			return err
		}

		// Load Proto
		Proto, err := protodata.NewProtoData(proto, wlzbjp.ProtoOptions...)
		if err != nil {
			return err
		}
		if err := Proto.Reload(); err != nil {
			return err
		}

		wlzbjp.Protodatas = Proto

		// 手動設定 Slot Pattern
		if pattern != -1 {
			section := data.GetConfigSection("slotconfig", "slotsetting")
			section["slotpattern"] = strconv.Itoa(int(pattern))
		}

		roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
			return wlzbjp.NewLogic(t, tfManager, cfgs)
		}

		gs, roomlogic, _, err := roomserver.SetupGameServer(data)
		if err != nil {
			mlog.Error("初始化服務器錯誤, %v", err)
			return err
		}

		helpercmd := helper.NewSlotHelperCmd(roomlogic)
		helpercmd.BaseLogicFunc = func(gl gamelogic.GameLogic) *baselogic.SlotLogicBase {
			return gl.(*wlzbjp.Logic).SlotLogicBase
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
		cmd.Println(conf.Version())
	},
}

func init() {
	cobra.MousetrapHelpText = ""

	rootCmd.AddCommand(versionCmd)
	analysis.GameName = "CSDJP"
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
