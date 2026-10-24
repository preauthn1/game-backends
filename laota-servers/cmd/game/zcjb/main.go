package main

import (
	"fmt"
	"os"
	"strconv"

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
	"example.invalid/ServerTeam/Servers/pkg/slot/baselogic"
	"example.invalid/ServerTeam/Servers/pkg/slot/helper"
)

var path string
var pattern int32

var rootCmd = &cobra.Command{
	Use:   "zcjb",
	Short: "招財進寶",
	RunE: func(cmd *cobra.Command, args []string) error {

		data := new(config.ConfigGroup)

		fi, err := os.Stat(path)
		if err != nil {
			return fmt.Errorf("Find config folder %s Error", path)
		}

		if !fi.IsDir() {
			return fmt.Errorf("%s is not a folder", path)
		}

		data.LoadConfig(path, 0)

		if data == nil {
			return fmt.Errorf("Can not find config folder, please put setting files on this folder")
		}

		if len(data.Configs) == 0 {
			return fmt.Errorf("Can not load configs, path: %s", data.DirName)
		}

		// 手動設定 Slot Pattern
		if pattern != -1 {
			section := data.GetConfigSection("slotconfig", "slotsetting")
			section["slotpattern"] = strconv.Itoa(int(pattern))
		}

		// 檢查protodata位置
		if err := dfdc.InitProtoData(); err != nil {
			return err
		}

		roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
			return dfdc.NewLogic(t, tfManager, cfgs)
		}

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
	analysis.GameName = "ZCJB"
	rootCmd.AddCommand(analysis.AnalysisCommand)

	rootCmd.PersistentFlags().StringVarP(&path, "config", "c", "config", "config folder")
	rootCmd.PersistentFlags().Int32VarP(&pattern, "pattern", "p", -1, "slot pattern")
}

func main() {

	if err := rootCmd.Execute(); err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(1)
	}

}
